// Goal: 
// Connect to a SpikeSafe and run Pulsed Sweep mode on Channel 1 into an LED
// Measure the emitted light using a Spectrometer
// Graph the results of the light (L), current (I), and voltage (V) measurements
//
// Expectation: 
// Channel 1 will run a sweep from 20mA to 200mA, which will take 100ms. Expecting a low (<1V) forward voltage
// Using external CAS DLL, control the spectrometer to make light measurements throughout the Pulsed Sweep

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.RunningLivSweeps
{
    public class LIVSweepExample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private StringBuilder sb = new StringBuilder(256);

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("LIVSweepExample.Run() started.");

                ////// set these before starting application

                // LIV Sweep SpikeSafe parameters
                double livStartCurrentMilliamps = 20;
                double livStopCurrentMilliamps = 200;
                double livSweepStepCount = 19;

                double complianceVoltageVolts = 20;
                double pulseOnTimeSeconds = 0.02;
                double pulseOffTimeSeconds = 0.0; // setting too small of an off time may result in missed measurements by the CAS4

                // CAS4 measurement settings
                double cas4IntegrationTimeMilliseconds = 10;
                double cas4TriggerDelayMilliseconds = 5;

                // CAS4 interface mode
                int cas4InterfaceMode = 3;
                // cas4InterfaceMode: int
                // - 1: PCI
                // - 3: Demo (No hardware)
                // - 5: USB
                // - 10: PCIe
                // - 11: Ethernet

                ////// CAS4 Spectrometer Connection/Initialization

                // Implements the external CAS4x64.dll file provided by Instrument Systems to configure a spectrometer for LIV sweep operation

                // creates a CAS4 device context to be used for all following configuration
                int deviceId = CAS4DLL.casCreateDeviceEx(cas4InterfaceMode, 0);

                // connect to the CAS4 interface
                if (cas4InterfaceMode != 3)
                {
                    int deviceInterface = CAS4DLL.casGetDeviceTypeOption(cas4InterfaceMode, deviceId);
                    deviceId = CAS4DLL.casCreateDeviceEx(cas4InterfaceMode, deviceInterface);
                }

                // check for errors on the CAS4
                CheckCASError(deviceId);

                string livSweepsFolder = "ApplicationSpecificExamples\\RunningLivSweeps";

                // specify and configure the .INI configuration and .ISC calibration file to initialize the CAS4
                Console.WriteLine("Enter .INI configuration file (must be located in src\\ApplicationSpecificExamples\\RunningLivSweeps) to be used for CAS operation:");
                string iniFileString = Console.ReadLine();
                if (iniFileString.EndsWith(".ini") == false)
                    iniFileString += ".ini";
                string iniFilePath = Path.Combine(Directory.GetCurrentDirectory(), livSweepsFolder, iniFileString);

                Console.WriteLine("Enter .ISC calibration file (must be located in src\\ApplicationSpecificExamples\\RunningLivSweeps) to be used for CAS operation:");
                string iscFileString = Console.ReadLine();
                if (iscFileString.EndsWith(".isc") == false)
                    iscFileString += ".isc";
                string iscFilePath = Path.Combine(Directory.GetCurrentDirectory(), livSweepsFolder, iscFileString);

                CAS4DLL.casSetDeviceParameterString(deviceId, CAS4DLL.dpidConfigFileName, iniFilePath);
                CAS4DLL.casSetDeviceParameterString(deviceId, CAS4DLL.dpidCalibFileName, iscFilePath);

                // initialize the CAS4 using the configuration and calibration files specified by the user, and check if any errors occur
                CheckCASError(CAS4DLL.casInitialize(deviceId, CAS4DLL.InitForced));


                ////// CAS4 Configuration

                // turn off CAS4 Autoranging so we can define our own integration time
                CAS4DLL.casSetOptionsOnOff(deviceId, CAS4DLL.coAutorangeMeasurement, 0);
                
                // set the CAS4 measurement integration time to 10ms to match the Pulsed Sweep parameters
                CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidIntegrationTime, cas4TriggerDelayMilliseconds);

                // set the CAS4 trigger mode to a hardware (i.e. flip-flop) trigger
                CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidTriggerSource, CAS4DLL.trgFlipFlop);

                // set the CAS4 trigger delay time to 5ms to match the Pulsed Sweep parameters
                CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidTriggerDelayTime, cas4IntegrationTimeMilliseconds);

                // set the CAS4 trigger delay time to 10 seconds
                CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidTriggerTimeout, 10000);

                 // prepare the CAS4 density filter if necessary
                double casNeedsToSetDensityFilter = CAS4DLL.casGetDeviceParameter(deviceId, CAS4DLL.dpidNeedDensityFilterChange);
                if (casNeedsToSetDensityFilter != 0)
                    CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidDensityFilter, CAS4DLL.casGetMeasurementParameter(deviceId, CAS4DLL.mpidNewDensityFilter));

                // perform a dark current measurement on the CAS4 if necessary
                int casNeedsDarkCurrentMeasurement = (int)CAS4DLL.casGetDeviceParameter(deviceId, CAS4DLL.dpidNeedDarkCurrent);
                if (casNeedsDarkCurrentMeasurement != 0)
                {
                    CAS4DLL.casSetShutter(deviceId, 1);
                    CAS4DLL.casMeasureDarkCurrent(deviceId);
                    CAS4DLL.casSetShutter(deviceId, 0);
                    CheckCASError(deviceId);
                }

                // prepare the CAS4 for measurement and verify that there are no resulting errors
                CheckCASError(CAS4DLL.casPerformAction(deviceId, CAS4DLL.paPrepareMeasurement));


                ////// SpikeSafe Connection and Configuration (Start of typical sequence)

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,    
                tcpSocket.SendScpiCommand("*RST"); 
                tcpSocket.SendScpiCommand("VOLT:ABOR");               
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set up SpikeSafe Channel 1 for Pulsed Sweep output. To find more explanation, see InstrumentExamples/RunPulsedSweep
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP PULSEDSWEEP");
                tcpSocket.SendScpiCommand(string.Format("SOUR1:CURR:STAR {0}", (livStartCurrentMilliamps) / 1000));
                tcpSocket.SendScpiCommand(string.Format("SOUR1:CURR:STOP {0}", (livStopCurrentMilliamps) / 1000));
                tcpSocket.SendScpiCommand(string.Format("SOUR1:CURR:STEP {0}", livSweepStepCount));   
                tcpSocket.SendScpiCommand(string.Format("SOUR1:VOLT {0}", complianceVoltageVolts));   
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:TON {0}", pulseOnTimeSeconds));
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:TOFF {0}", pulseOffTimeSeconds)); 

                // Check for any errors with SpikeSafe initialization commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set up SpikeSafe Digitizer to measure Pulsed Sweep output. To find more explanation, see MakingIntegratedVoltageMeasurements/MeasurePulsedSweepVoltage
                tcpSocket.SendScpiCommand("VOLT:RANG 100");
                tcpSocket.SendScpiCommand(string.Format("VOLT:APER {0}", pulseOnTimeSeconds * 600000)); // we want to measure 60% of the pulse
                tcpSocket.SendScpiCommand(string.Format("VOLT:TRIG:DEL {0}", pulseOnTimeSeconds * 200000)); // we want to skip the first 20% of the pulse
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");
                tcpSocket.SendScpiCommand(string.Format("VOLT:TRIG:COUN {0}", livSweepStepCount));
                tcpSocket.SendScpiCommand("VOLT:READ:COUN 1");

                // Check for any errors with Digitizer initialization commands
                ReadAllEvents.LogAllEvents(tcpSocket);


                ////// LIV Sweep Operation

                // turn on SpikeSafe Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // start SpikeSafe Digitizer measurements. We want the digitizer waiting for triggers before starting the pulsed sweep
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // Wait until SpikeSafe Channel 1 is ready for a trigger command
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // Output pulsed sweep for Channel 1
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // prepare data objects for CAS4 measurement
                List<double> lightReadings = new List<double>();

                // obtain CAS4 measurements
                for (int measurementNumber = 0; measurementNumber < livSweepStepCount; measurementNumber++)
                {
                    // reset the CAS4 trigger signal in preparation for the next measurement 
                    CAS4DLL.casSetDeviceParameter(deviceId, CAS4DLL.dpidLine1FlipFlop, 0);

                    // perform the CAS4 measurement
                    CAS4DLL.casMeasure(deviceId);
                    CheckCASError(CAS4DLL.casColorMetric(deviceId));
                    CAS4DLL.casGetPhotInt(deviceId, out double lightReading, sb, sb.Capacity);
                    lightReadings.Add(lightReading);
                    CheckCASError(deviceId);

                    // Check for any SpikeSafe errors while outputting the Pulsed Sweep
                    ReadAllEvents.LogAllEvents(tcpSocket);
                }

                // wait for the Digitizer measurements to complete. We need to wait for the data acquisition to complete before fetching the data
                DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);

                // fetch the SpikeSafe Digitizer voltage readings
                List<DigitizerData> digitizerData = DigitizerDataFetch.FetchVoltageData(tcpSocket);

                // turn off SpikeSafe Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();    

                // disconnect from the CAS4
                CAS4DLL.casDoneDevice(deviceId);


                ////// Data Graphing

                // put the fetched data in a plottable data format
                List<double> voltageReadings = new List<double>();
                List<double> currentSteps = new List<double>();
                double stepSizeMilliamps = (livStopCurrentMilliamps - livStartCurrentMilliamps) / (livSweepStepCount - 1); // Step Size [in mA] = (StopCurrent - StartCurrent)/(StepCount - 1)
                foreach (DigitizerData dd in digitizerData)
                {
                    voltageReadings.Add(dd.VoltageReading);
                    currentSteps.Add(livStartCurrentMilliamps + stepSizeMilliamps * (dd.SampleNumber - 1));
                }

                // plot the pulse shape using the fetched voltage readings and the light measurement readings overlaid
                var plt = new ScottPlot.Plot();

                // configure the voltage data
                var voltageReadingsLine = plt.AddScatterLines(currentSteps.ToArray(), voltageReadings.ToArray(), Color.Red, 1);
                plt.XAxis.Label("Set Current (mA)");         
                plt.YAxis.Label("Voltage (V)", Color.Red);  
                
                // configure the light measurement data
                var voltageCalculatedReadingsLine = plt.AddScatterLines(currentSteps.ToArray(), lightReadings.ToArray(), Color.Blue, 1);
                plt.YAxis2.Label("Photometric (lm)", Color.Blue);
                plt.YAxis2.Ticks(true);

                plt.Title(string.Format("'LIV Sweep ({}mA to {}mA)", livStartCurrentMilliamps, livStopCurrentMilliamps));
                plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"liv_sweep_graph_screenshot.png"));

                _log.Info("LIVSweepExample.Run() completed.\n");
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

        private int CheckCASError(int AError)
        {
			if (AError < CAS4DLL.ErrorNoError) {
				CAS4DLL.casGetErrorMessage(AError, sb, sb.Capacity);
				throw new Exception(string.Format("CAS DLL error ({0}): {1}", AError, sb.ToString()));
			}
			return AError;
		}
    }
}