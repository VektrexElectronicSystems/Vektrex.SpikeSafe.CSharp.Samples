// Goal: 
// Connect to a SpikeSafe and run Multi Pulse mode on Channel 1 into an LED, Laser, or electrical component
//
// Expectation: 
// All channels will output a 100mA pulse with a pulse width of 1ms and a Bias Current of 10mA. This will happen 3 times
// After outputting one Multi-Pulse train at 100mA, change the Set Current to 200mA while the channel is enabled and trigger another Multi-Pulse train
// Expecting a low (<1V) forward voltage

using System;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunMultiPulse
{
    public class RunMultiPulseMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("RunMultiPulseMode.Run() started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's pulse mode to Multi Pulse
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP MULTIPULSE");

                // set Channel 1's current to 100 mA
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1"); 

                // set Channel 1's voltage to 20 V 
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");   

                // set Channel 1's Pulse On Time and Pulse Off Time to 1s each
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 1");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 1");

                // set Channel 1's Pulse Count to 3. Every trigger will output 3 pulses
                tcpSocket.SendScpiCommand("SOUR1:PULS:COUN 3");

                // set Channel 1's compensation settings to their default values
                // For higher power loads or shorter pulses, these settings may have to be adjusted to obtain ideal pulse shape
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");   

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1
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

                // check that the Multi Pulse output has ended
                string hasMultiPulseEnded = string.Empty;
                while (hasMultiPulseEnded != "TRUE")
                {
                    tcpSocket.SendScpiCommand("SOUR1:PULS:END?");
                    hasMultiPulseEnded =  tcpSocket.ReadData();
                    Threading.Wait(0.5);
                }

                // After the pulsing has ended, set Channel 1's current to 200 mA while the channel is enabled
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.2"); 

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

                // check that the Multi Pulse output has ended
                // check that the Multi Pulse output has ended
                hasMultiPulseEnded = string.Empty;
                while (hasMultiPulseEnded != "TRUE")
                {
                    tcpSocket.SendScpiCommand("SOUR1:PULS:END?");
                    hasMultiPulseEnded =  tcpSocket.ReadData();
                    Threading.Wait(0.5);
                }

                // turn off all Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();    

                _log.Info("RunMultiPulseMode.Run() completed.\n");
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