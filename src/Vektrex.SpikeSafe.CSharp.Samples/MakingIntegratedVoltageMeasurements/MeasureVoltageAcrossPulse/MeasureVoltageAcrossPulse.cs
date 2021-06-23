// Goal: 
// Connect to a SpikeSafe and output a Single Pulse into a 10Ω resistor
// Take voltage measurements throughout that pulse using the SpikeSafe PSMU's integrated Digitizer to determine the pulse shape
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

namespace Vektrex.SpikeSafe.CSharp.Samples.MakingIntegratedVoltageMeasurements.MeasureVoltageAcrossPulse
{
    public class MeasureVoltageAcrossPulse
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("MeasureVoltageAcrossPulse.Run() started.");
        
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // abort digitizer in order get it into a known state. This is good practice when connecting to a SpikeSafe PSMU
                tcpSocket.SendScpiCommand("VOLT:ABOR");

                // set up Channel 1 for single pulse output. To find more explanation, see run_spikesafe_operating_modes/run_single_pulse
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP SINGLEPULSE");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.001");
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");    
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");  
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");   
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");

                // set Digitizer voltage range to 10V since we expect to measure voltages significantly less than 10V
                tcpSocket.SendScpiCommand("VOLT:RANG 10");

                // set Digitizer aperture for 2µs, the minimum value. Aperture specifies the measurement time, and we want to measure incrementally across the current pulse
                tcpSocket.SendScpiCommand("VOLT:APER 2");

                // set Digitizer trigger delay to 0µs. We want to take measurements as fast as possible
                tcpSocket.SendScpiCommand("VOLT:TRIG:DEL 0");

                // set Digitizer trigger source to hardware. When set to a hardware trigger, the digitizer waits for a trigger signal from the SpikeSafe to start a measurement
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");

                // set Digitizer trigger edge to rising. The Digitizer will start a measurement after the SpikeSafe's rising pulse edge occurs
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");

                // set Digitizer trigger count to 1. We are measuring the output of one current pulse
                tcpSocket.SendScpiCommand("VOLT:TRIG:COUN 1");

                // set Digitizer reading count to 525, the maximum value. We are measuring a 1ms pulse, and will take 525 measurements 2µs apart from each other
                tcpSocket.SendScpiCommand("VOLT:READ:COUN 525");

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // initialize the digitizer. Measurements will be taken once a current pulse is outputted
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is ready to pulse
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // output a current pulse for Channel 1
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // wait for the Digitizer measurements to complete
                DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);

                // fetch the Digitizer voltage readings using VOLT:FETC? query
                List<DigitizerData> digitizerData = DigitizerDataFetch.FetchVoltageData(tcpSocket);

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // put the fetched data in a plottable data format
                var plt = new ScottPlot.Plot();
                List<double> samples = new List<double>();
                List<double> voltageReadings = new List<double>();
                foreach (DigitizerData dd in digitizerData)
                {
                    samples.Add(dd.SampleNumber);
                    voltageReadings.Add(dd.VoltageReading);
                }

                // plot the pulse shape using the fetched voltage readings
                plt.AddScatterLines(samples.ToArray(), voltageReadings.ToArray(), Color.Blue, 1);
                plt.YAxis.Label("Voltage (V)");
                plt.XAxis.Label("Sample Number (#)");
                plt.Title("Digitizer Voltage Readings - 1ms 100mA Pulse");
                plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"single_pulse_digitizer_voltage.png"));

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                _log.Info("MeasureVoltageAcrossPulse.Run() completed.\n");
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

