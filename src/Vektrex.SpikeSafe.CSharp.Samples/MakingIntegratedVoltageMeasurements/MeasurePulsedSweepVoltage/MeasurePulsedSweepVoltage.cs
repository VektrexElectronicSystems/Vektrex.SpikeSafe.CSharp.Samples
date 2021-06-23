// Goal: 
// Connect to a SpikeSafe and run a Pulsed Sweep into a 10Ω resistor. Take voltage measurements from the pulsed output using the SpikeSafe PSMU's integrated Digitizer
// 
// Expectation: 
// Channel 1 will be driven with 100mA with a forward voltage of ~1V during this time

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

                // abort digitizer in order get it into a known state. This is good practice when connecting to a SpikeSafe PSMU
                tcpSocket.SendScpiCommand("VOLT:ABOR");

                // set up Channel 1 for pulsed sweep output. To find more explanation, see instrument_examples/run_spikesafe_operating_modes/run_pulsed
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP PULSEDSWEEP");
                tcpSocket.SendScpiCommand("SOUR1:CURR:STAR 0.02");
                tcpSocket.SendScpiCommand("SOUR1:CURR:STOP 0.2");   
                tcpSocket.SendScpiCommand("SOUR1:CURR:STEP 100");    
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");   
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.0001");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.0099");
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");   

                // set Digitizer voltage range to 10V since we expect to measure voltages significantly less than 10V
                tcpSocket.SendScpiCommand("VOLT:RANG 10");

                // set Digitizer aperture for 60µs. Aperture specifies the measurement time, and we want to measure a majority of the pulse's constant current output
                tcpSocket.SendScpiCommand("VOLT:APER 60");

                // set Digitizer trigger delay to 20µs. We want to give sufficient delay to omit any overshoot the current pulse may have
                tcpSocket.SendScpiCommand("VOLT:TRIG:DEL 20");

                // set Digitizer trigger source to hardware. When set to a hardware trigger, the digitizer waits for a trigger signal from the SpikeSafe to start a measurement
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");

                // set Digitizer trigger edge to rising. The Digitizer will start a measurement after the SpikeSafe's rising pulse edge occurs
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");

                // set Digitizer trigger count to 100. We want to take one voltage reading for every step in the pulsed sweep
                tcpSocket.SendScpiCommand("VOLT:TRIG:COUN 100");

                // set Digitizer reading count to 1. This is the amount of readings that will be taken when the Digitizer receives its specified trigger signal
                tcpSocket.SendScpiCommand("VOLT:READ:COUN 1");

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is fully ramped so we can send a trigger command for a pulsed sweep
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // start Digitizer measurements. We want the digitizer waiting for triggers before starting the pulsed sweep
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // trigger Channel 1 to start the pulsed sweep output
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // wait for the Digitizer measurements to complete. We need to wait for the data acquisition to complete before fetching the data
                DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);

                // fetch the Digitizer voltage readings
                List<DigitizerData> digitizerData = DigitizerDataFetch.FetchVoltageData(tcpSocket);

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

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
                plt.AddScatterLines(currentSteps.ToArray(), voltageReadings.ToArray(), Color.Blue, 1);
                plt.YAxis.Label("Voltage (V)");
                plt.XAxis.Label("Set Current (mA)");
                plt.Title("Digitizer Voltage Readings - Pulsed Sweep (20mA to 200mA)");
                plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"pulsed_sweep_voltages.png"));

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();    

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

