using System;
using System.Threading;
using System.IO;  
using System. Text;  
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MakingTransientDualInterfaceMeasurement
{    
    public class MakingTransientDualInterfaceMeasurementExample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        
        private const int FAST_LOG_TOTAL_SAMPLE_COUNT = 525;
        private const int MEDIUM_LOG_TOTAL_SAMPLE_COUNT = 500;
        private const int SLOW_LOG_TOTAL_SAMPLE_COUNT = 460;
        private const int FAST_LOG_MODE = 1;
        private const int MEDIUM_LOG_MODE = 2;
        private const int SLOW_LOG_MODE = 3;
        private const int GREASE = 1;
        private const int NO_GREASE = 2;

        public void Run(string ipAddress, int portNumber)
        {
            TcpSocket tcpSocket = new TcpSocket();

            try
            {
                Console.WriteLine("Enter a number corresponding to one of the following options:");
                Console.WriteLine("1. Run Test");
                Console.WriteLine("2. Create Graph");
                int option = int.Parse(Console.ReadLine());

                if(1 == option)
                {
                    Console.WriteLine("Enter the option # for sampling mode:");
                    Console.WriteLine("1. FASTLOG");
                    Console.WriteLine("2. MEDIUMLOG");
                    Console.WriteLine("3. SLOWLOG");
                    Console.WriteLine("If this is the second test, please make sure the sampling mode is the same as the first test.");
                    int samplingMode = int.Parse(Console.ReadLine());
		
                    Console.WriteLine("Enter the option # for the 1st test:");
                    Console.WriteLine("1. Grease");
                    Console.WriteLine("2. No Grease");
                    int greaseInput = int.Parse(Console.ReadLine());
		
                    // connect
                    tcpSocket.Connect(ipAddress, portNumber);
                    Console.WriteLine("Connected to {0}", ipAddress);
		
                    // SpikeSafe set up
                    SpikeSafeSetup(tcpSocket, samplingMode);
		
                    // Digitizer wait for new data ready
                    DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);                
		
                    // fetch data
                    List<DigitizerData> digitizerData = DigitizerDataFetch.FetchVoltageData(tcpSocket);
		
                    // store voltage into file
                    WriteDigitizerVoltageToFile(greaseInput, digitizerData);
		
                    List<double> voltageReadings = new List<double>();
                    List<double> logScaleTime = new List<double>();
		
                    // populate voltageReadings and logScaleTime lists
                    foreach (DigitizerData dd in digitizerData)
                    {
                        voltageReadings.Add(dd.VoltageReading);
                    }
		
                    if(FAST_LOG_MODE == samplingMode)
                    {
                        logScaleTime = LogTimeData(FAST_LOG_TOTAL_SAMPLE_COUNT);
                    }
                    else if(MEDIUM_LOG_MODE == samplingMode)
                    {
                        logScaleTime = LogTimeData(MEDIUM_LOG_TOTAL_SAMPLE_COUNT);
                    }
                    else if(SLOW_LOG_MODE == samplingMode)
                    {
                        logScaleTime = LogTimeData(SLOW_LOG_TOTAL_SAMPLE_COUNT);
                    }
					
                    // plot the pulse shape using the fetched voltage readings and the light measurement readings overlaid
                    CreateSinglePlot(logScaleTime, voltageReadings, samplingMode);                    
                }
                else if(2 == option)
                {
                    CreateMultiplePlots();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Digitizer Log Sampling error: {0}", e.Message);
            }
            finally
            {
                if (tcpSocket.Socket.Connected)
                {
                    // stop channel
                    tcpSocket.SendScpiCommand("OUTP1 0");

                    // disconnect
                    tcpSocket.Disconnect();
                    Console.WriteLine("Disconnected from {0}", ipAddress);
                }

                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
            }            
        }

        private void CreateSinglePlot(List<double> logScaleTime, List<double> voltageReadings, int samplingMode)
        {
            var plt = new ScottPlot.Plot();

            // plot the pulse shape using the fetched voltage readings
            plt.AddScatterLines(ScottPlot.Tools.Log10(logScaleTime.ToArray()), voltageReadings.ToArray(), Color.Blue, 1);
            plt.YAxis.Label("Voltage (V)");
            plt.XAxis.Label("Time 10^ (s)");
            plt.XAxis.MinorLogScale(true);

            string plotFileName = "";
            if(SLOW_LOG_MODE == samplingMode)
            {
                plt.Title("Digitizer Slow Log Sampling");
                plotFileName = "slow_log_sampling_single_plot.png";
            }
            else if(MEDIUM_LOG_MODE == samplingMode)
            {
                plt.Title("Digitizer Medium Log Sampling");
                plotFileName = "medium_log_sampling_single_plot.png";
            }
            else if(FAST_LOG_MODE == samplingMode)
            {
                plt.Title("Digitizer Fast Log Sampling");
                plotFileName = "fast_log_sampling_single_plot.png";
            }
            plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), plotFileName));            
        }

        private void WriteDigitizerVoltageToFile(int greaseInput, List<DigitizerData> digitizerData)
        {
            string path = System.IO.Directory.GetCurrentDirectory(); 
            string fileName = "";
                    
            if(greaseInput == GREASE)
            {
                fileName = "\\digitizer_log_sampling_grease.txt";
            }
            else if(greaseInput == NO_GREASE)
            {
                fileName = "\\digitizer_log_sampling_noGrease.txt";
            }    

            // streamwrite voltageReadings to file
            using (StreamWriter sw = File.CreateText(path + fileName))
            {
                foreach (DigitizerData dd in digitizerData)
                {
                    sw.WriteLine(dd.VoltageReading);
                }
            }
        }

        private void SpikeSafeSetup(TcpSocket tcpSocket, int sampleMode)
        {
            tcpSocket.SendScpiCommand("VOLT:ABOR");
            // Set digitizer range to 10V
            tcpSocket.SendScpiCommand("VOLT:RANG 10");
            // Set digitizer sampling mode
            if(FAST_LOG_MODE == sampleMode)
            {
                tcpSocket.SendScpiCommand("VOLT:SAMPMODE FASTLOG");
            }
            else if(MEDIUM_LOG_MODE == sampleMode)
            {
                tcpSocket.SendScpiCommand("VOLT:SAMPMODE MEDIUMLOG");
            }
            else if(SLOW_LOG_MODE == sampleMode)
            {
                tcpSocket.SendScpiCommand("VOLT:SAMPMODE SLOWLOG");
            }
            // set digitizer trigger source to HARDWARE
            tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");
            // set digitizer trigger delay to 50us
            tcpSocket.SendScpiCommand("VOLT:TRIG:DEL 50");
            // SMU setting
            // Set DC Dynamic mode
            tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP DCDYNAMIC");
            // set MCV to 25V
            tcpSocket.SendScpiCommand("SOUR1:VOLT 25");
            // set auto range
            tcpSocket.SendScpiCommand("SOUR1:CURR:RANG:AUTO 1");
            // set currenty to 1A
            tcpSocket.SendScpiCommand("SOUR1:CURR 1");
            // set Ramp mode to Fast
            tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");
            // request SpikeSafe events and read data
            ReadAllEvents.ReadAllEventData(tcpSocket);
            // the trigger signal come from the voltage start up
            tcpSocket.SendScpiCommand("VOLT:INIT");
            // Start the channel
            tcpSocket.SendScpiCommand("OUTP1 ON");
            // wait for channel ready
            ReadAllEvents.ReadUntilEvent(tcpSocket, 100);
        }

        private void CreateMultiplePlots()
        {
            int totalSampleCount = 0;
            string readFilePath = System.IO.Directory.GetCurrentDirectory(); 
            string readFileNameNoGrease = "\\digitizer_log_sampling_noGrease.txt";
            string readFileNameGrease = "\\digitizer_log_sampling_grease.txt";
                
            List<double> voltageReadingsNoGrease = new List<double>();
            List<double> voltageReadingsGrease = new List<double>();
            List<double> logScaleTime = new List<double>();

            int sampleNumberNoGrease = 0;
            int sampleNumberGrease = 0;

            // If the file is not exist, the error message will be thrown "Digitizer Log Sampling error: ... "
            using (StreamReader sr = new StreamReader(readFilePath + readFileNameNoGrease))
            {
                string line;
                while ((line = sr.ReadLine()) != null) 
                {
                    voltageReadingsNoGrease.Add(Convert.ToDouble(line)); 
                    sampleNumberNoGrease++;
                }
            }

            using (StreamReader sr = new StreamReader(readFilePath + readFileNameGrease))
            {
                string line;
                while ((line = sr.ReadLine()) != null) 
                {
                    voltageReadingsGrease.Add(Convert.ToDouble(line)); 
                    sampleNumberGrease++;
                }
            }

            if(sampleNumberGrease == sampleNumberNoGrease)
            {
                totalSampleCount = sampleNumberGrease;
            }
            else
            {
                //Two test run in different mode. Throw error
                throw new InvalidOperationException("The Grease and no Grease testing were ran with different sampling mode.");
            }      

            logScaleTime = LogTimeData(totalSampleCount);

            var plt = new ScottPlot.Plot();
            // plot the pulse shape using the fetched voltage readings
            plt.AddScatterLines(ScottPlot.Tools.Log10(logScaleTime.ToArray()), voltageReadingsNoGrease.ToArray(), Color.Blue, 1, label: "No Grease");
            plt.AddScatterLines(ScottPlot.Tools.Log10(logScaleTime.ToArray()), voltageReadingsGrease.ToArray(), Color.Red, 1, label: "Grease");
            plt.YAxis.Label("Voltage (V)");
            plt.XAxis.Label("Time 10^ (s)");
            plt.XAxis.MinorLogScale(true);
            plt.Legend();

            string plotFileName = "";
            if(SLOW_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                plt.Title("Digitizer Slow Log Sampling");
                plotFileName = "slow_log_sampling.png";
            }
            else if(MEDIUM_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                plt.Title("Digitizer Medium Log Sampling");
                plotFileName = "medium_log_sampling.png"; 
            }
            else if(FAST_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                plt.Title("Digitizer Fast Log Sampling");
                plotFileName = "fast_log_sampling.png";
            }
            plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), plotFileName));
             
            // create subtracted plot
            List<double> subtractedVoltage = new List<double>();
            for(int sampleNumber=0; sampleNumber<totalSampleCount; sampleNumber++)
            {
                subtractedVoltage.Add(voltageReadingsGrease[sampleNumber] - voltageReadingsNoGrease[sampleNumber]);
            }
            var plt2 = new ScottPlot.Plot();
            plt2.AddScatterLines(ScottPlot.Tools.Log10(logScaleTime.ToArray()), subtractedVoltage.ToArray(), Color.Green, 1, label: "Voltage Difference");
            plt2.YAxis.Label("Voltage (V)");
            plt2.XAxis.Label("Time 10^ (s)");             
            plt2.XAxis.MinorLogScale(true);
            plt2.Legend();   

            string plotFileName2 = "";    
            if(SLOW_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                plt2.Title("Voltage Subtraction (Slow Log Sampling)");
                plotFileName2 = "slow_log_sampling_subtraction.png";
            }
            else if(MEDIUM_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                plt2.Title("Voltage Subtraction (Medium Log Sampling)");
                plotFileName2 = "medium_log_sampling_subtraction.png";
            }
            else if(FAST_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                plt2.Title("Voltage Subtraction (Fast Log Sampling)");
                plotFileName2 = "fast_log_sampling_subtraction.png";
            }   
            plt2.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), plotFileName2));
            System.Environment.Exit(0);    
        }

        private List<double> LogTimeData(int totalSampleCount)
        {
            var timeAxis = new List<double>();
            float timeUs = 0;

            if(SLOW_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                timeUs = 1000;
            }
            else if(MEDIUM_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                timeUs = 2;
            }
            else if(FAST_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
            {
                timeUs = 2;
            }

            for(int sampleNumber=1; sampleNumber<=totalSampleCount; sampleNumber++)
            {
                timeAxis.Add(timeUs/1000000);
                if(SLOW_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
                {
                    // log time scale
                    if( sampleNumber > 0 && sampleNumber <=99)
                    {
                        timeUs = timeUs + 1000;
                    }
                    else if(sampleNumber > 99 && sampleNumber <= 189 )
                    {
                        timeUs = timeUs + 10000;
                    }
                    else if(sampleNumber > 189 && sampleNumber <= 279)
                    {
                        timeUs = timeUs + 100000;
                    }
                    else if(sampleNumber > 279 && sampleNumber <= 369)
                    {
                        timeUs = timeUs + 1000000;
                    }
                    else if(sampleNumber > 369 && sampleNumber <= 459)
                    {
                        timeUs = timeUs + 10000000;
                    }
                }
                else if(MEDIUM_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
                {
                    if(sampleNumber > 0 && sampleNumber <=49)
                    {
                        timeUs = timeUs + 2;
                    }
                    else if(sampleNumber > 49 && sampleNumber <= 124)
                    {
                        timeUs = timeUs + 12;
                    }
                    else if(sampleNumber > 124 && sampleNumber <= 199)
                    {
                        timeUs = timeUs + 120;
                    }
                    else if(sampleNumber > 199 && sampleNumber <= 274)
                    {
                        timeUs = timeUs + 1200;
                    }       
                    else if(sampleNumber > 274 && sampleNumber <= 349)
                    {
                        timeUs = timeUs + 12000;
                    }
                    else if(sampleNumber > 349 && sampleNumber <= 424)         
                    {
                        timeUs = timeUs + 120000;
                    }
                    else if(sampleNumber > 424 && sampleNumber <= 500)         
                    {
                        timeUs = timeUs + 1200000;
                    }
                }
                else if(FAST_LOG_TOTAL_SAMPLE_COUNT == totalSampleCount)
                {
                    if(sampleNumber > 0 && sampleNumber <=99)
                    {
                        timeUs = timeUs + 2;
                    }
                    else if(sampleNumber > 99 && sampleNumber <= 179)
                    {
                        timeUs = timeUs + 10;
                    }
                    else if(sampleNumber > 179 && sampleNumber <= 269)
                    {
                        timeUs = timeUs + 100;
                    }
                    else if(sampleNumber > 269 && sampleNumber <= 359)
                    {
                        timeUs = timeUs + 1000;
                    }       
                    else if(sampleNumber > 359 && sampleNumber <= 449)
                    {
                        timeUs = timeUs + 10000;
                    }
                    else if(sampleNumber > 449 && sampleNumber <= 524)         
                    {
                        timeUs = timeUs + 100000;
                    }
                }
            }
            return timeAxis;
        }        
    }
}
