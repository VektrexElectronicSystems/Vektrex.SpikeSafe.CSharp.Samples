// Goal: 
// Connect to a SpikeSafe and run Pulsed mode into a 10Ω resistor. Take voltage measurements from the pulsed output using the SpikeSafe PSMU's integrated Digitizer
// 
// Expectation: 
// Channel 1 will be driven with 100mA with a forward voltage of ~1V during this time

using System;
using System.Collections.Generic;
using System.Drawing;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.MakingIntegratedVoltageMeasurements.MeasureAllPulsedVoltages
{
    public class MeasureAllPulsedVoltages
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("MeasureAllPulsedVoltages.Run() started.");

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // abort digitizer in order get it into a known state. This is good practice when connecting to a SpikeSafe PSMU
                tcpSocket.SendScpiCommand("VOLT:ABOR");

                // set up Channel 1 for pulsed output. To find more explanation, see InstrumentExamples/RunPulsed
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP PULSED");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.001");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.009");
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");    
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");  
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");   
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");

                // set Digitizer voltage range to 10V since we expect to measure voltages significantly less than 10V
                tcpSocket.SendScpiCommand("VOLT:RANG 10");

                // set Digitizer aperture for 600µs. Aperture specifies the measurement time, and we want to measure a majority of the pulse's constant current output
                tcpSocket.SendScpiCommand("VOLT:APER 600");

                // set Digitizer trigger delay to 200µs. We want to give sufficient delay to omit any overshoot the current pulse may have
                tcpSocket.SendScpiCommand("VOLT:TRIG:DEL 200");

                // set Digitizer trigger source to hardware. When set to a hardware trigger, the digitizer waits for a trigger signal from the SpikeSafe to start a measurement
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");

                // set Digitizer trigger edge to rising. The Digitizer will start a measurement after the SpikeSafe's rising pulse edge occurs
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");

                // set Digitizer trigger count to 525, the maximum value. 525 rising edges of current pulses will correspond to 525 voltage readings
                tcpSocket.SendScpiCommand("VOLT:TRIG:COUN 525");

                // set Digitizer reading count to 1. This is the amount of readings that will be taken when the Digitizer receives its specified trigger signal
                tcpSocket.SendScpiCommand("VOLT:READ:COUN 1");

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is fully ramped before we take any digitizer measurements. We are looking to measure consistent voltage values
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // start Digitizer measurements
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // wait for the Digitizer measurements to complete. We need to wait for the data acquisition to complete before fetching the data
                DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);

                // fetch the Digitizer voltage readings
                List<DigitizerData> digitizerData = DigitizerDataFetch.FetchVoltageData(tcpSocket);

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // prepare digitizer voltage data to plot
                var plt = new ScottPlot.Plot();
                List<double> samples = new List<double>();
                List<double> voltageReadings = new List<double>();
                foreach (DigitizerData dd in digitizerData)
                {
                    samples.Add(dd.SampleNumber);
                    voltageReadings.Add(dd.VoltageReading);
                }

                // plot the pulse shape using the fetched voltage readings
                plt.YAxis.Label("Voltage (V)");
                plt.XAxis.Label("Sample Number (//)");
                plt.Title("Digitizer Voltage Readings - 525 pulses (1ms & 100mA)");
                // TODO. set axis plt.Axis([-25, 550, min(voltageReadings) - 0.1, max(voltageReadings) + 0.1]);
                plt.AddScatterLines(samples.ToArray(), voltageReadings.ToArray(), Color.Blue, 1);

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                _log.Info("MeasureAllPulsedVoltages.Run() completed.\n");   
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