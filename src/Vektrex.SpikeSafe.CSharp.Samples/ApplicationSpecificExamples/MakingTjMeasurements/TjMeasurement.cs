// Goal: 
// Make a junction temperature measurement on an LED using the Electrical Test Method specified by the Joint Electron Device Engineering Council
//
// Expectation:
// 1.) A K-factor will be measured by comparing voltages at two controlled temperatures
// 2.) The LED will be heated using its operational current until it reaches a stable operating temperature
// 3.) The SpikeSafe will be run in CDBC mode and the digitizer will make voltage readings at the beginning of an Off Time cycle
// after step 3, readings will be graphed with a logarithmic x-axis

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MakingTjMeasurements
{
    public class TjMeasurement
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("TjMeasurement.Run() started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // abort digitizer in order get it into a known state. This is good practice when connecting to a SpikeSafe PSMU
                tcpSocket.SendScpiCommand("VOLT:ABOR");

                // set up Channel 1 for Bias Current output to determine the K-factor
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP BIAS");
                tcpSocket.SendScpiCommand("SOUR0:CURR:BIAS 0.033");
                tcpSocket.SendScpiCommand("SOUR1:VOLT 40");
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");    
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");  

                LogAndPrintToConsole("\nConfigured SpikeSafe to Bias Current mode to obtain K-factor. Starting current output.");

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is ready to pulse
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                LogAndPrintToConsole("\nMeasurement Current is currently outputting to the DUT.\n\nPress \'Enter\' in the console once temperature has been stabilized at T1, then record V1 and T1.");
                Console.ReadLine();
                LogAndPrintToConsole("Enter T1 (in °C):");
                double temperatureOne = double.Parse(ReceiveUserInputAndLog());
                LogAndPrintToConsole("Enter V1 (in V):");
                double voltageOne = double.Parse(ReceiveUserInputAndLog());

                Threading.Wait(2);

                LogAndPrintToConsole("\nMeasurement Current is currently outputting to the DUT.\n\nChange the control temperature to T2.\n\nPress \'Enter\' in the console once temperature has been stabilized at T2, then record V2 and T2.");
                Console.ReadLine();
                LogAndPrintToConsole("Enter T2 (in °C):");
                double temperatureTwo = double.Parse(ReceiveUserInputAndLog());
                LogAndPrintToConsole("Enter V2 (in V):");
                double voltageTwo = double.Parse(ReceiveUserInputAndLog());

                double kFactor = (voltageTwo - voltageOne)/(temperatureTwo - temperatureOne);
                LogAndPrintToConsole(string.Format("K-factor: {0} V/°C", kFactor));

                // turn off Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 0");

                LogAndPrintToConsole("\nK-factor values obtained. Stopped bias current output. Configuring to perform Electrical Test Method measurement.");

                // set up Channel 1 for CDBC output to make the junction temperature measurement
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP BIASPULSEDDYNAMIC");
                tcpSocket.SendScpiCommand("SOUR1:CURR 3.5");
                tcpSocket.SendScpiCommand("SOUR0:CURR:BIAS 0.033");
                tcpSocket.SendScpiCommand("SOUR1:VOLT 40");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 1");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.001");
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");    
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");  

                // set Digitizer settings to take a series of quick measurements during the Off Time of CDBC operation
                tcpSocket.SendScpiCommand("VOLT:RANG 100");
                tcpSocket.SendScpiCommand("VOLT:APER 2");
                tcpSocket.SendScpiCommand("VOLT:TRIG:DEL 0");
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE FALLING");
                tcpSocket.SendScpiCommand("VOLT:TRIG:COUN 1");
                tcpSocket.SendScpiCommand("VOLT:READ:COUN 500");

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is ready to pulse
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                LogAndPrintToConsole("\nHeating Current is being outputted.\n\nWait until temperature has stabilized, then press \'Enter\' in the console to take voltage measurements.");
                Console.ReadLine();

                // initialize the digitizer. Measurements will be taken once a current pulse is outputted
                tcpSocket.SendScpiCommand("VOLT:INIT");

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

                LogAndPrintToConsole("Voltage readings are graphed above. Determine the x-values at which the graph is linear by hovering the mouse over graph and noting the \'x=\' in the bottom right.\n\nTake note of the starting x-value and the last x-value (maximum = 500) at which the graph is linear. Once these values are written down, close the graph and enter those values in the console.\n");

                // plot the pulse shape using the fetched voltage readings
                plt.YAxis.Label("Voltage (V)");
                plt.XAxis.Label("Sample number after Heating Current output [logarithmic] (#)");
                plt.Title("Digitizer Voltage Readings - Vf(0) Extrapolation");

                // The graph zoom offset is used to zoom in or out to better visualize data in the final graph to make it easier to determine Vf(0). Value is in volts
                // A value of zero corresponds to a completely zoomed in graph. Increase the value to zoom out. Recommended values are between 0.001 and 0.100
                double graphZoomOffset = 0.01;

                // Setting the axes so all data can be effectively visualized. For the y-axis, graphZoomOffset = 0.01 by default. 
                // Modify as necessary at the top of this sequence so that Vf(0) can effectively be estimated using this graph               
                plt.SetAxisLimits(1, 500, voltageReadings.Min() - graphZoomOffset, voltageReadings.Last() + graphZoomOffset);

                plt.AddScatterLines(ScottPlot.Tools.Log10(samples.ToArray()), voltageReadings.ToArray(), Color.Blue, 1);
                plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"vf0_extrapolation_graph.png"));

                LogAndPrintToConsole("Enter the Start sample (of the linear portion of the graph):");
                int firstLinearSample = int.Parse(ReceiveUserInputAndLog());
                LogAndPrintToConsole("Enter the End sample (of the linear portion of the graph) [max=500]:");
                int lastLinearSample = int.Parse(ReceiveUserInputAndLog());

                double Vf0 = calculateVf0(firstLinearSample, lastLinearSample, digitizerData);
                double junctionTemperature = temperatureOne + ((Vf0 - voltageOne) / kFactor);

                LogAndPrintToConsole(string.Format("\nExtrapolated Vf(0) value: {0} V", Math.Round(Vf0, 4)));
                LogAndPrintToConsole(string.Format("Calculated junction temperature (Tj): {0} °C\n", Math.Round(junctionTemperature, 4)));

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                _log.Info("TjMeasurement.Run() completed.\n");
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

        private void LogAndPrintToConsole(string messageString)
        {
            _log.Info(messageString.Replace('\n','\0'));
            Console.WriteLine(messageString);
        }

        private string ReceiveUserInputAndLog()
        {
            string inputText = Console.ReadLine();
            _log.Info(inputText);
            return inputText;
        }
        
        private double calculateVf0(int startPoint, int endPoint, List<DigitizerData> digitizerDataList)
        {
            // only want to reference the data within the straight line
            List<DigitizerData> straightLineDigitizerData = new List<DigitizerData>();
            for (int index = startPoint; index < endPoint; index++)
                straightLineDigitizerData.Add(digitizerDataList[index]);

            List<int> sampleList = new List<int>();
            List<double> sqrtSampleList = new List<double>();
            List<double> voltageList = new List<double>();
            foreach (DigitizerData dd in straightLineDigitizerData)
            {
                sampleList.Add(dd.SampleNumber);
                sqrtSampleList.Add(Math.Sqrt(dd.SampleNumber));
                voltageList.Add(dd.VoltageReading);
            }

            // this method uses data from the time square root axis to extrapolate to T=0    
            int numberOfDataPoints = straightLineDigitizerData.Count;
            double sqrtSampleAverage = sqrtSampleList.Average();
            double voltageAverage = voltageList.Average();

            int sumOfSamples = sampleList.Sum();
            double sumOfPointProducts = 0.0;
            for (int index = 0; index < numberOfDataPoints; index++)
                sumOfPointProducts += sqrtSampleList[index] * voltageList[index];

            // We extrapolate Vf0 by using a line-of-best fit 
            double lineOfBestFitSlope = (sumOfPointProducts / numberOfDataPoints - sqrtSampleAverage * voltageAverage) / (sumOfSamples / numberOfDataPoints - sqrtSampleAverage * sqrtSampleAverage);
            double Vf0 = -1 * (lineOfBestFitSlope * sqrtSampleAverage - voltageAverage);
            return Vf0;
        }
    }
}
