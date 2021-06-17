// Goal: 
// Connect to a SpikeSafe and run Bias Single Pulse mode on Channel 1 into an LED, Laser, or electrical component
// 
// Expectation: 
// All channels will output a 110mA pulse with a pulse width of 1ms and a Bias Current of 10mA. This will happen 3 times. 
// Expecting a low (<1V) forward voltage

using System;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunSinglePulse
{
    public class RunBiasSinglePulseMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("RunBiasSinglePulseMode.Run() started.");
                
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's pulse mode to Bias Single Pulse
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP BIASSINGLEPULSE");

                // set Channel 1's current to 100 mA
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");

                // set Channel 1's bias current to 10 mA and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR:BIAS 0.01");     

                // set Channel 1's voltage to 20 V 
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");   

                // set Channel 1's pulse width to 1ms. Of the pulse time settings, only Pulse On Time and Pulse Width [+Offset] are relevant in Single Pulse mode
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.001");

                // set Channel 1's compensation settings to their default values
                // For higher power loads or shorter pulses, these settings may have to be adjusted to obtain ideal pulse shape
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");   

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1. Bias current will be outputted as long as the channel is on
                tcpSocket.SendScpiCommand("OUTP1 1");

                // Wait until channel is ready for a trigger command
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // Output 1ms pulse for Channel 1
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // check for all events and measure readings on the channel once per second for 2 seconds,
                // it is best practice to do this to ensure the channel is on and does not have any errors
                DateTime timeEnd = DateTime.Now.AddSeconds(2);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }         

                // Output 1ms pulse for Channel 1. Multiple pulses can be outputted while the channel is enabled
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // check for all events and measure readings after the second pulse output
                timeEnd = DateTime.Now.AddSeconds(2);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }     

                // Output 1ms pulse for Channel 1
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // check for all events and measure readings after the last pulse output
                timeEnd = DateTime.Now.AddSeconds(2);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }     

                // turn off all Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();  

                _log.Info("RunBiasSinglePulseMode.Run() completed.\n");
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
