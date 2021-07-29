// Goal: 
// Demonstrate the connect/disconnect switch functionality of the SpikeSafe PSMU while operating in Multi-Pulse mode
// 
// Expectation: 
// Channel 1 will run in Multi-Pulse mode with the switch set to Primary
// While the channel is enabled but not outputting, the switch be set to Auxiliary mode to isolate the source from the DUT
// Once any modifications to the DUTs have completed in Auxiliary mode, the switch will be set to Primary in which the SpikeSafe will output another Multi-Pulse train

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.UsingForceSenseSelectorSwitch.ConnectDisconnectSwitching
{
    public class ConnectDisconnectSwitchSample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("ForceSenseSwitchSample.Run() started.");

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state    
                tcpSocket.SendScpiCommand("*RST");                  

                // check that the Force Sense Selector Switch is available for this SpikeSafe. We need the switch to run this sequence
                // If switch related SCPI is sent and there is no switch configured, it will result in error "386, Output Switch is not installed"
                tcpSocket.SendScpiCommand("OUTP1:CONN:AVAIL?");
                string isSwitchAvailable = tcpSocket.ReadData();
                if (isSwitchAvailable != "Ch:1")
                    throw new Exception("Force Sense Selector Switch is not available, and is necessary to run this sequence.");

                // set the Force Sense Selector Switch state to Primary (A) so that the SpikeSafe can output to the DUT
                // the default switch state can be manually adjusted using SCPI, so it is best to send this command even after sending a *RST
                tcpSocket.SendScpiCommand("OUTP1:CONN PRI");

                // set Channel 1's settings to operate in Multi-Pulse mode
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP MULTIPULSE");
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1"); 
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");   
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 1");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 1");
                tcpSocket.SendScpiCommand("SOUR1:PULS:COUN 3");
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

                // set the Force Sense Selector Switch state to Auxiliary to disconnect the SpikeSafe output from the DUT
                // this action can be performed as long as no pulses are actively being outputted from the SpikeSafe. The channel may be enabled
                tcpSocket.SendScpiCommand("OUTP1:CONN AUX");

                // show a message so any tasks using the Auxiliary source may be performed before adjusting the switch back to Primary
                // the SpikeSafe is not electrically connected to the DUT at this time
                Console.WriteLine("Force Sense Selector Switch is in Auxiliary mode, so SpikeSafe is isolated from the DUT. Once DUT modifications are complete, press Enter adjust the switch back to Primary mode and re-connect the SpikeSafe.");
                Console.ReadLine();

                // set the Force Sense Selector Switch state to Primary (A) so that the SpikeSafe can output to the DUT
                tcpSocket.SendScpiCommand("OUTP1:CONN PRI");

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

                _log.Info("ConnectDisconnectSwitchSample.Run() completed.\n");
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