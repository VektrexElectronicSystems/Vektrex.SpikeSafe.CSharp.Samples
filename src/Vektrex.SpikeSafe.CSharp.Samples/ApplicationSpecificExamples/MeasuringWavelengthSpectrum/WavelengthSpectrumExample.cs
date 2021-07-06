// Goal: 
// Connect to a SpikeSafe and run DC Dynamic mode on Channel 1 into an LED
// Measure the emitted light using a Spectrometer
//
// Expectation: 
// Channel 1 will output 100mA DC current. Expecting a low (<1V) forward voltage
// Using external CAS DLL, control the spectrometer to make light measurements and then graph the wavelength spectrum

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MeasuringWavelengthSpectrum
{
    public class WavelengthSpectrumExample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private StringBuilder sb = new StringBuilder(256);

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("WavelengthSpectrumExample.Run() started.");

                // SpikeSafe Single Pulse current settings
                double setCurrentAmps = 0.1;
                double complianceVoltageVolts = 20;

                // CAS4 measurement settings
                double CAS4IntegrationTimeMs = 20;
                double CAS4TriggerDelayMs = 5; // needs to be set to a non-zero value for the spectrometer to correctly output data

                // CAS4 interface mode
                int CAS4InterfaceMode = 3;
                // CAS4InterfaceMode: int
                // - 1: PCI
                // - 3: Demo (No hardware)
                // - 5: USB
                // - 10: PCIe
                // - 11: Ethernet
                
                
                ////// CAS4 Spectrometer Connection/Initialization

                // Implements the external CAS4x64.dll file provided by Instrument Systems to configure a spectrometer for LIV sweep operation

                // creates a CAS4 device context to be used for all following configuration
                int deviceId = CAS4DLL.casCreateDeviceEx(CAS4InterfaceMode, 0);

                // connect to the CAS4 interface
                if (CAS4InterfaceMode != 3)
                {
                    int deviceInterface = CAS4DLL.casGetDeviceTypeOption(CAS4InterfaceMode, deviceId);
                    deviceId = CAS4DLL.casCreateDeviceEx(CAS4InterfaceMode, deviceInterface);
                }

                // check for errors on the CAS4
                CheckCASError(deviceId);

                string measuringWaveLengthFolder = "ApplicationSpecificExamples\\MeasuringWavelengthSpectrum";

                // specify and configure the .INI configuration and .ISC calibration file to initialize the CAS4
                Console.WriteLine("Enter .INI configuration file (must be located in src\\ApplicationSpecificExamples\\MeasuringWavelengthSpectrum) to be used for CAS operation:");
                string iniFileString = Console.ReadLine();
                if (iniFileString.EndsWith(".ini") == false)
                    iniFileString += ".ini";
                string iniFilePath = Path.Join(Directory.GetCurrentDirectory(), measuringWaveLengthFolder, iniFileString);

                Console.WriteLine("Enter .ISC calibration file (must be located in src\\ApplicationSpecificExamples\\MeasuringWavelengthSpectrum) to be used for CAS operation:");
                string iscFileString = Console.ReadLine();
                if (iscFileString.EndsWith(".isc") == false)
                    iscFileString += ".isc";
                string iscFilePath = Path.Join(Directory.GetCurrentDirectory(), measuringWaveLengthFolder, iscFileString);

                CAS4DLL.casSetDeviceParameterString(deviceId, CAS4DLL.dpidConfigFileName, iniFilePath);
                CAS4DLL.casSetDeviceParameterString(deviceId, CAS4DLL.dpidCalibFileName, iscFilePath);

                // initialize the CAS4 using the configuration and calibration files specified by the user, and check if any errors occur
                CheckCASError(CAS4DLL.casInitialize(deviceId, CAS4DLL.InitForced));


                ////// CAS4 Configuration

                // turn off CAS4 Autoranging so we can define our own integration time
                CAS4DLL.casSetOptionsOnOff(deviceId, CAS4DLL.coAutorangeMeasurement, 0);
                
                // set the CAS4 measurement integration time to 10ms to match the Pulsed Sweep parameters
                CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidIntegrationTime, CAS4TriggerDelayMs);

                // set the CAS4 trigger mode to a hardware (i.e. flip-flop) trigger
                CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidTriggerSource, CAS4DLL.trgFlipFlop);

                // set the CAS4 trigger delay time to 5ms to match the Pulsed Sweep parameters
                CAS4DLL.casSetMeasurementParameter(deviceId, CAS4DLL.mpidTriggerDelayTime, CAS4IntegrationTimeMs);

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

                // prepare the CAS4 for measurement and verify that there are no resulting errorss
                CheckCASError(CAS4DLL.casPerformAction(deviceId, CAS4DLL.paPrepareMeasurement));

                // reset the CAS4 trigger signal in preparation for the measurement 
                CAS4DLL.casSetDeviceParameter(deviceId, CAS4DLL.dpidLine1FlipFlop, 0);

                ////// SpikeSafe Connection and Configuration (Start of typical sequence)

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset SpikeSafe to default state and check for all events    
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set SpikeSafe Channel 1's pulse mode to Single Pulse and set all relevant settings. For more information, see RunSpikeSafeOperatingModes/RunDc
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP SINGLEPULSE");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 1");    
                tcpSocket.SendScpiCommand(string.Format("SOUR1:CURR {0}", setCurrentAmps));        
                tcpSocket.SendScpiCommand(string.Format("SOUR1:VOLT {0}", complianceVoltageVolts));         
                ReadAllEvents.LogAllEvents(tcpSocket); 

                // turn on SpikeSafe Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 1");               
                ReadAllEvents.LogAllEvents(tcpSocket);                            

                // wait until the channel is fully ramped and output a single pulse
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"
                tcpSocket.SendScpiCommand("OUTP1:TRIG");   

                // take a CAS4 measurement
                CAS4DLL.casMeasure(deviceId);
                CheckCASError(deviceId);

                // determine the number of visible pixels to be measured by the CAS4
                int visiblePixels = (int)CAS4DLL.casGetDeviceParameter(deviceId, CAS4DLL.dpidVisiblePixels);
                CheckCASError(visiblePixels);

                // prepare data objects for CAS4 measurement
                List<double> spectrum = new List<double>();
                List<double> wavelengths = new List<double>();

                // determine the number of dead pixels to be ignored by the CAS4
                int deadPixels = (int)CAS4DLL.casGetDeviceParameter(deviceId, CAS4DLL.dpidDeadPixels);
                CheckCASError(deadPixels);

                // measure the spectrum and associate wavelengths using the CAS4. Ignore dead pixels
                for (int pixel = 0; pixel < visiblePixels; pixel++)
                {
                    spectrum.Add(CAS4DLL.casGetData(deviceId, pixel + deadPixels));
                    wavelengths.Add(CAS4DLL.casGetXArray(deviceId, pixel + deadPixels));
                }

                // turn off SpikeSafe Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 0");               
                ReadAllEvents.LogAllEvents(tcpSocket);

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                // disconnect from the CAS4
                CAS4DLL.casDoneDevice(deviceId);


                ////// Plot the wavelength spectrum
                List<double> spectralIntensity = new List<double>();
                double spectrumMax = spectrum.Max();

                if (spectrumMax == 0)
                    throw new Exception("Full spectrum was measured as 0.0 mW/nm");

                foreach (double point in spectrum)
                    spectralIntensity.Add((point/spectrumMax) * 100);

                var plt = new ScottPlot.Plot();
                plt.AddScatterLines(wavelengths.ToArray(), spectralIntensity.ToArray(), Color.Blue, 1);
                plt.YAxis.Label("Spectral Intensity (%)");
                plt.XAxis.Label("Wavelength (nm))");               
                plt.SetAxisLimits(wavelengths.Min(), wavelengths.Max(), spectralIntensity.Min(), 100);
                plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"spectrum_output.png"));

                _log.Info("WavelengthSpectrumExample.Run() completed.\n");
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