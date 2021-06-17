// Goal: 
// Connect to a SpikeSafe and run Single Pulse mode on all channels into an LED, Laser, or electrical component and output to all channels
// 
// Expectation: 
// All channels will output a 100mA pulse with a pulse width of 1ms. This will happen 3 times. Expecting a low (<1V) forward voltage

using System;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunPulsedSweep
{
    public class RunSinglePulseMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("RunSinglePulseMode.py started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set each channel's pulse mode to Single Pulse
                tcpSocket.SendScpiCommand("SOUR0:FUNC:SHAP SINGLEPULSE");

                // set each channel's current to 100 mA
                tcpSocket.SendScpiCommand("SOUR0:CURR 0.1");     

                // set each channel's voltage to 20 V 
                tcpSocket.SendScpiCommand("SOUR0:VOLT 20");   

                // set each channel's pulse width to 1ms. Of the pulse time settings, only Pulse On Time and Pulse Width [+Offset] are relevant in Single Pulse mode
                tcpSocket.SendScpiCommand("SOUR0:PULS:TON 0.001");

                // set each channel's compensation settings to their default values
                // For higher power loads or shorter pulses, these settings may have to be adjusted to obtain ideal pulse shape
                tcpSocket.SendScpiCommand("SOUR0:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR0:PULS:RCOM 4");   

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on all channels
                tcpSocket.SendScpiCommand("OUTP0 1");

                // Wait until channels are ready for a trigger command
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // Output 1ms pulse for all channels
                tcpSocket.SendScpiCommand("OUTP0:TRIG");

                // check for all events and measure readings on each channel once per second for 2 seconds,
                // it is best practice to do this to ensure each channel is on and does not have any errors
                DateTime timeEnd = DateTime.Now.AddSeconds(2);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }         

                // Output 1ms pulse for all channels. Multiple pulses can be outputted while the channel is enabled
                tcpSocket.SendScpiCommand("OUTP0:TRIG");

                // check for all events and measure readings after the second pulse output
                timeEnd = DateTime.Now.AddSeconds(2);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }     
                // After the pulse is complete, set each channel's current to 200 mA while the channels are enabled
                tcpSocket.SendScpiCommand("SOUR0:CURR 0.2");  

                // Output 1ms pulse for all channels
                tcpSocket.SendScpiCommand("OUTP0:TRIG");

                // check for all events and measure readings after the last pulse output
                timeEnd = DateTime.Now.AddSeconds(2);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }     

                // turn off all channels after routine is complete
                tcpSocket.SendScpiCommand("OUTP0 0");

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();   

                _log.Info("RunSinglePulseMode.Run() completed.\n");
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
