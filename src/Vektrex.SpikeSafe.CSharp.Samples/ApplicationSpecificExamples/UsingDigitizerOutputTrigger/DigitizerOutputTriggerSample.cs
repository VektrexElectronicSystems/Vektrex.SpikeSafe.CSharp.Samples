// Goal: 
// Demonstrate using the Digitizer Output Trigger as an input trigger to the SpikeSafe or an external instrument
// 
// Expectation: 
// The digitizer will output a trigger signal, the SpikeSafe will run a 3-pulse Multi Pulse sequence, and the voltages will be measured by the Digitizer and graphed

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.UsingDigitizerOutputTrigger
{
    public class DigitizerOutputTriggerSample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("DigitizerOutputTriggerSample.Run() started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // abort digitizer in order get it into a known state. This is good practice when connecting to a SpikeSafe PSMU
                tcpSocket.SendScpiCommand("VOLT:ABOR");

                // set up Channel 1 for Multi Pulse output. To find more explanation, see RunSpikeSafeOperationModes/RunMultiPulse
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP MULTIPULSE");
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");   
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 1");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 1");
                tcpSocket.SendScpiCommand("SOUR1:PULS:COUN 3");
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");    
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");  

                // set Channel 1's Input Trigger Source to External so an external trigger signal will start SpikeSafe current output
                tcpSocket.SendScpiCommand("OUTP1:TRIG:SOUR EXT");  

                // set Channel 1's Input Trigger Delay to 10µs (the minimum value). The SpikeSafe will output current 10µs after receiving the input trigger signal
                tcpSocket.SendScpiCommand("OUTP1:TRIG:DEL 10");  

                // set Channel 1's Input Trigger Polarity to rising. This should match the expected polarity of the trigger signal
                tcpSocket.SendScpiCommand("OUTP1:TRIG:POL RISING");   

                // set typical Digitizer settings to match SpikeSafe settings. For more explanation, see MakingIntegratedVoltageMeasurements
                tcpSocket.SendScpiCommand("VOLT:RANG 10");
                tcpSocket.SendScpiCommand("VOLT:APER 400000");
                tcpSocket.SendScpiCommand("VOLT:TRIG:DEL 200000");
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");
                tcpSocket.SendScpiCommand("VOLT:TRIG:COUN 6"); // two 3-pulse Multi Pulse sequences will output
                tcpSocket.SendScpiCommand("VOLT:READ:COUN 1"); 

                // set the Digitizer Hardware Trigger polarity to rising
                tcpSocket.SendScpiCommand("VOLT:OUTP:TRIG:EDGE RISING");  

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // initialize the digitizer. Measurements will be taken once a current pulse is outputted
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is ready to pulse
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // output the Digitizer hardware output trigger. 10µs after this signal is outputted, the Multi Pulse sequence will start
                tcpSocket.SendScpiCommand("VOLT:OUTP:TRIG");

                // check that the Multi Pulse output has ended
                string hasMultiPulseEnded = string.Empty;
                while (hasMultiPulseEnded != "TRUE")
                {
                    tcpSocket.SendScpiCommand("SOUR1:PULS:END?");
                    hasMultiPulseEnded =  tcpSocket.ReadData();
                    Threading.Wait(0.5);
                }

                // output the Digitizer hardware output trigger. As long as the SpikeSafe is ready to pulse, this can be done continuously
                tcpSocket.SendScpiCommand("VOLT:OUTP:TRIG");

                // check that the Multi Pulse output has ended
                hasMultiPulseEnded = string.Empty;
                while (hasMultiPulseEnded != "TRUE")
                {
                    tcpSocket.SendScpiCommand("SOUR1:PULS:END?");
                    hasMultiPulseEnded =  tcpSocket.ReadData();
                    Threading.Wait(0.5);
                }

                // wait for the Digitizer measurements to complete 
                DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);

                // fetch the Digitizer voltage readings using VOLT:FETC? query
                List<DigitizerData> digitizerData = new List<DigitizerData>();
                digitizerData = DigitizerDataFetch.FetchVoltageData(tcpSocket);

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
                plt.XAxis.Label("Sample Number (#)");
                plt.Title("Digitizer Voltage Readings - two 3-pulse Multi-Pulse outputs");
                plt.XAxis.ManualTickSpacing(1);
                plt.SetAxisLimits(0, 7, voltageReadings.Min() - 0.1, voltageReadings.Max() + 0.1);
                plt.AddScatterLines(samples.ToArray(), voltageReadings.ToArray(), Color.Blue, 1);
                plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"digitizer_readings_graph.png"));

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                _log.Info("DigitizerOutputTriggerSample.Run() completed.\n");
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

