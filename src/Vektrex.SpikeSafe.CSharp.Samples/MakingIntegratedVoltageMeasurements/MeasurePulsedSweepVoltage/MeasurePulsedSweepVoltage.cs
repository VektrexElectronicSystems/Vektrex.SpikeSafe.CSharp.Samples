// Goal: 
// Connect to a SpikeSafe and run a Pulsed Sweep into a 10Ω resistor. Take voltage measurements from the pulsed output using the SpikeSafe PSMU's integrated Digitizer
// 
// Expectation: 
// Channel 1 will be driven with 100mA with a forward voltage of ~1V during this time

using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.MakingIntegratedVoltageMeasurements.MeasurePulsedSweepVoltage
{
    public class MeasurePulsedSweepVoltage
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("MeasurePulsedSweepVoltage.Run() started.");
    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // abort digitizer in order get it into a known state. This is good practice when connecting to a SpikeSafe PSMU.  VOLT:ABOR is included with RST command on newer SMU's.
                tcpSocket.SendScpiCommand("VOLT:ABOR");

                // Parse SpikeSafe information for later use
                SpikeSafeInfo spikeSafeInfo = SpikeSafeInfoParser.Parse(tcpSocket);

                // set up Channel 1 for pulsed sweep output. To find more explanation, see InstrumentExamples/RunSpikeSafeOperatingModes/RunPulsed
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP PULSEDSWEEP");
                tcpSocket.SendScpiCommand("SOUR1:CURR:STAR 0.02");
                tcpSocket.SendScpiCommand("SOUR1:CURR:STOP 0.2");   
                tcpSocket.SendScpiCommand("SOUR1:CURR:STEP 100");
                double complianceVoltage = 20;
                tcpSocket.SendScpiCommand($"SOUR1:VOLT {Precision.GetPreciseComplianceVoltageCommandArgument(complianceVoltage)}");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.0001");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.0099");
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");   

                // set Digitizer voltage range to 10V since we expect to measure voltages significantly less than 10V
                tcpSocket.SendScpiCommand("VOLT:RANG 10");

                // set Digitizer aperture. Aperture specifies the measurement time, and we want to measure a majority of the pulse's constant current output
                int apertureMicroseconds = 60;
                tcpSocket.SendScpiCommand($"VOLT:APER {Precision.GetPreciseTimeMicrosecondsCommandArgument(apertureMicroseconds)}");

                // set Digitizer trigger delay. We want to give sufficient delay to omit any overshoot the current pulse may have
                int hardwareTriggerDelayMicroseconds = 20;
                tcpSocket.SendScpiCommand($"VOLT:TRIG:DEL {Precision.GetPreciseTimeMicrosecondsCommandArgument(hardwareTriggerDelayMicroseconds)}");

                // set Digitizer trigger source to hardware. When set to a hardware trigger, the digitizer waits for a trigger signal from the SpikeSafe to start a measurement
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");

                // set Digitizer trigger edge to rising. The Digitizer will start a measurement after the SpikeSafe's rising pulse edge occurs
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");

                // set Digitizer trigger count. We want to take one voltage reading for every step in the pulsed sweep
                int hardwareTriggerCount = 100;
                tcpSocket.SendScpiCommand($"VOLT:TRIG:COUN {hardwareTriggerCount}");

                // set Digitizer reading count. This is the amount of readings that will be taken when the Digitizer receives its specified trigger signal
                int readingCount = 1;
                tcpSocket.SendScpiCommand($"VOLT:READ:COUN {readingCount}");

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is fully ramped so we can send a trigger command for a pulsed sweep
                ReadAllEvents.ReadUntilEvent(tcpSocket, (int)SpikeSafeEvents.CHANNEL_READY); // event 100 is "Channel Ready"

                // start Digitizer measurements. We want the digitizer waiting for triggers before starting the pulsed sweep
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // trigger Channel 1 to start the pulsed sweep output
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Get estimated completion time for Digitizer measurements to occur. Estimating completion time minimizes digitizer polling during DigitizerDataFetch.
                double estimatedCompleteTimeSeconds = DigitizerDataFetch.GetNewVoltageDataEstimatedCompleteTime(
                    apertureMicroseconds: apertureMicroseconds,
                    readingCount: readingCount,
                    hardwareTriggerCount: hardwareTriggerCount,
                    hardwareTriggerDelayMicroseconds: hardwareTriggerDelayMicroseconds);

                // fetch voltage data from Digitizer containing sample number and voltage reading
                List<DigitizerData> digitizerData = null;

                try
                {
                    // wait for the Digitizer measurements to complete. We need to wait for the data acquisition to complete before fetching the data
                    DigitizerDataFetch.WaitForNewVoltageData(
                        spikeSafeSocket: tcpSocket,
                        waitTime: estimatedCompleteTimeSeconds,
                        enableLogging: null,
                        timeout: 10,
                        digitizerNumber: null);

                    // fetch complete data
                    digitizerData = DigitizerDataFetch.FetchVoltageData(
                        spikeSafeSocket: tcpSocket, 
                        enableLogging: null, 
                        digitizerNumber: null);

                    _log.Info("Complete VOLT:FETC? Response returned with {0} readings", digitizerData.Count);
                }
                catch (Exception err)
                {
                    // on timeout or error, abort the digitizer and fetch any partial readings

                    // attempt to abort partial digitizer readings
                    tcpSocket.SendScpiCommand("VOLT:ABOR:PART", tcpSocket.EnableLogging);

                    // wait for the Digitizer partial measurements to complete. It's expected that the wait time here will be small since we are fetching partial data after an abort.
                    DigitizerDataFetch.WaitForNewVoltageData(
                        spikeSafeSocket: tcpSocket,
                        waitTime: 0.01,
                        enableLogging: null,
                        timeout: 10,
                        digitizerNumber: null);

                    // fetch whatever data is available
                    digitizerData = DigitizerDataFetch.FetchVoltageData(
                        spikeSafeSocket: tcpSocket,
                        enableLogging: null,
                        digitizerNumber: null);

                    _log.Info("Partial VOLT:FETC? Response after error returned with {0} readings", digitizerData.Count);
                }

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // wait for Channel 1 to fully discharge to ensure safe conditions before re-starting channel or disconnecting the load
                Discharge.WaitForSpikeSafeChannelDischarge(
                    spikeSafeSocket: tcpSocket, 
                    spikeSafeInfo: spikeSafeInfo,
                    complianceVoltage: complianceVoltage,
                    channelNumber: 1);

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                // put the fetched data in a plottable data format
                var plt = new ScottPlot.Plot();
                List<double> voltageReadings = new List<double>();
                List<double> currentSteps = new List<double>();
                double startCurrentMilliamps = 20;
                double stepSizeMilliamps = 1.82;  // 1.82mA = Step Size = (StopCurrent - StartCurrent)/(StepCount - 1)
                foreach (DigitizerData dd in digitizerData)
                {
                    voltageReadings.Add(dd.VoltageReading);
                    currentSteps.Add(startCurrentMilliamps + stepSizeMilliamps * (dd.SampleNumber - 1));
                }

                // plot the pulse shape using the fetched voltage readings
                var scatter = plt.Add.ScatterLine(currentSteps.ToArray(), voltageReadings.ToArray());
                scatter.Color = Colors.Blue;
                scatter.LineWidth = 1;
                plt.YLabel("Voltage (V)");
                plt.XLabel("Set Current (mA)");
                plt.Title("Digitizer Voltage Readings - Pulsed Sweep (20mA to 200mA)");
                plt.SavePng(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"pulsed_sweep_voltages.png"), 800, 600);

                _log.Info("MeasurePulsedSweepVoltage.Run() completed.\n");
            }
            catch(SpikeSafeException e)
            {
                // print any SpikeSafe-specific error to both the terminal and the log file, then exit the application
                string errorMessage = string.Format("SpikeSafe error: {0}\n", e.Message);
                _log.Error(errorMessage);
                Console.WriteLine(errorMessage);
            }
            catch(Exception e)
            {
                // print any general exception to both the terminal and the log file, then exit the application
                string errorMessage = string.Format("Program error: {0}\n", e.Message);
                _log.Error(errorMessage);
                Console.WriteLine(errorMessage);
            }
        }
    }
}

