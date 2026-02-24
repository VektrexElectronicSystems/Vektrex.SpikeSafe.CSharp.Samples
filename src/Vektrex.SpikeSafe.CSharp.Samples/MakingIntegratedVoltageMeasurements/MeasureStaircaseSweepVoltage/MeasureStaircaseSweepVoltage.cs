// Goal: 
// Connect to a SpikeSafe and run a Staircase Sweep into a 10Ω resistor. Take voltage measurements from the staircase output using the SpikeSafe PSMU's integrated Digitizer
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

namespace Vektrex.SpikeSafe.CSharp.Samples.MakingIntegratedVoltageMeasurements.MeasureStaircaseSweepVoltage
{
    public class MeasureStaircaseSweepVoltage
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                double startCurrentAmps = 0.02;
                double stopCurrentAmps = 0.2;
                int currentStepCount = 10;
                double complianceVoltageVolts = 20;
                int stepOnTimeMilliseconds = 1;

                double digitizerVoltageRangeVolts = 10;
                int digitizerApertureMicroseconds = 514;
                int digitizerHardwareTriggerDelayMicroseconds = 150;
                int digitizerHardwareTriggerCount = currentStepCount;
                int digitizerReadingCount = 1;

                _log.Info("MeasureStaircaseSweepVoltage.Run() started.");
    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events, this will automatically abort digitizer in order get it into a known state
                // This is good practice when connecting to a SpikeSafe PSMU, and is best practice to check for errors after sending each command        
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // Parse SpikeSafe information for later use
                SpikeSafeInfo spikeSafeInfo = SpikeSafeInfoParser.Parse(tcpSocket, enableLogging: null);

                // set Channel 1's pulse mode to Staircase Sweep and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP STAIRCASESWEEP");

                // set Channel 1's Staircase Sweep parameters to match the test expectation
                tcpSocket.SendScpiCommand($"SOUR1:CURR:STA:SWE:STAR {Precision.GetPreciseCurrentCommandArgument(startCurrentAmps)}");
                tcpSocket.SendScpiCommand($"SOUR1:CURR:STA:SWE:STOP {Precision.GetPreciseCurrentCommandArgument(stopCurrentAmps)}");
                tcpSocket.SendScpiCommand($"SOUR1:CURR:STA:SWE:STEP {currentStepCount}");
                tcpSocket.SendScpiCommand($"SOUR1:PULS:STA:SWE:TON {stepOnTimeMilliseconds}");

                // set Channel 1's voltage
                tcpSocket.SendScpiCommand($"SOUR1:VOLT {Precision.GetPreciseComplianceVoltageCommandArgument(complianceVoltageVolts)}");

                // set Channel 1's compensation settings to High/Fast
                // For higher power loads or shorter pulses, these settings may have to be adjusted to obtain ideal pulse shape
                (SpikeSafeEnums.LoadImpedance loadImpedance, SpikeSafeEnums.RiseTime riseTime) = Compensation.GetOptimumCompensation(
                    spikesafeModelMaxCurrentAmps: spikeSafeInfo.MaximumSetCurrent,
                    setCurrentAmps: stopCurrentAmps,
                    pulseOnTimeSeconds: stepOnTimeMilliseconds / 1000.0);
                tcpSocket.SendScpiCommand($"SOUR1:PULS:CCOM {(int)loadImpedance}");
                tcpSocket.SendScpiCommand($"SOUR1:PULS:RCOM {(int)riseTime}");

                // set Channel 1's Ramp mode to Fast
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");

                // set Channel 1's External Source Trigger Out to Always
                tcpSocket.SendScpiCommand("SOUR1:PULS:TRIG ALWAYS");

                // set Channel 1's External Source Trigger Out to Positive

                tcpSocket.SendScpiCommand("OUTP1:TRIG:SLOP POS");

                // set Channel 1's trigger source to Internal (so that the SpikeSafe triggers the Staircase Sweep when the OUTP:TRIG command is sent)
                tcpSocket.SendScpiCommand("OUTP1:TRIG:SOUR INT");

                // set Digitizer voltage range to 10V since we expect to measure voltages significantly less than 10V
                tcpSocket.SendScpiCommand($"VOLT:RANG {digitizerVoltageRangeVolts}");

                // set Digitizer aperture. Aperture specifies the measurement time, and we want to measure a majority of the pulse's constant current output
                tcpSocket.SendScpiCommand($"VOLT:APER {Precision.GetPreciseTimeMicrosecondsCommandArgument(digitizerApertureMicroseconds)}");

                // set Digitizer trigger delay. We want to give sufficient delay to omit any overshoot the current pulse may have
                tcpSocket.SendScpiCommand($"VOLT:TRIG:DEL {Precision.GetPreciseTimeMicrosecondsCommandArgument(digitizerHardwareTriggerDelayMicroseconds)}");

                // set Digitizer trigger source to hardware. When set to a hardware trigger, the digitizer waits for a trigger signal from the SpikeSafe to start a measurement
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");

                // set Digitizer trigger edge to rising. The Digitizer will start a measurement after the SpikeSafe's rising pulse edge occurs
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");

                // set Digitizer trigger count. We want to take one voltage reading for every step in the pulsed sweep
                tcpSocket.SendScpiCommand($"VOLT:TRIG:COUN {digitizerHardwareTriggerCount}");

                // set Digitizer reading count. This is the amount of readings that will be taken when the Digitizer receives its specified trigger signal
                tcpSocket.SendScpiCommand($"VOLT:READ:COUN {digitizerReadingCount}");

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // start Digitizer measurements. We want the digitizer waiting for triggers before starting the pulsed sweep
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is fully ramped so we can send a trigger command for a pulsed sweep
                ReadAllEvents.ReadUntilEvent(tcpSocket, SpikeSafeEvents.CHANNEL_READY); // event 100 is "Channel Ready"

                // trigger Channel 1 to start the pulsed sweep output
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Get estimated completion time for Digitizer measurements to occur. Estimating completion time minimizes digitizer polling during DigitizerDataFetch.
                double estimatedCompleteTimeSeconds = DigitizerDataFetch.GetNewVoltageDataEstimatedCompleteTime(
                    apertureMicroseconds: digitizerApertureMicroseconds,
                    readingCount: digitizerReadingCount,
                    hardwareTriggerCount: digitizerHardwareTriggerCount,
                    hardwareTriggerDelayMicroseconds: digitizerHardwareTriggerDelayMicroseconds);

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
                    _log.Error("Complete VOLT:FETC? Response error: {0}", err.Message);

                    try
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

                        // check if partial measurements were a result of a SpikeSafe error
                        ReadAllEvents.LogAllEvents(tcpSocket);
                    }
                    catch (TimeoutException e)
                    {
                        // Timeout error will occur if no partial measurements were taken
                        _log.Error("Partial VOLT:FETC? Response error: {0}", e.Message);

                        // check if no partial measurements were a result of a SpikeSafe error
                        ReadAllEvents.ReadAllEventData(tcpSocket);
                    }
                    catch (Exception)
                    {
                        // All other errors, exit the script
                        throw;
                    }
                }

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // wait for Channel 1 to fully discharge to ensure safe conditions before re-starting channel or disconnecting the load
                Discharge.WaitForSpikeSafeChannelDischarge(
                    spikeSafeSocket: tcpSocket, 
                    spikeSafeInfo: spikeSafeInfo,
                    complianceVoltage: complianceVoltageVolts,
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
                plt.Title("Digitizer Voltage Readings - Staircase Sweep (20mA to 200mA)");
                plt.SavePng(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"staircase_sweep_voltages.png"), 800, 600);

                _log.Info("MeasureStaircaseSweepVoltage.Run() completed.\n");
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

