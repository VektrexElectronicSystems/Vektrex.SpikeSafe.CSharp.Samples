// Goal: 
// Demonstrate the A/B switch functionality of the SpikeSafe PSMU while operating in DC mode
// 
// Expectation: 
// Channel 1 will run in DC mode with the switch set to Primary. 
// Afterward the Switch be set to Auxiliary mode, in which another source may operate connected to the SpikeSafe
// After the Auxiliary source has completed operation, the switch will be set to Primary to operate the SpikeSafe in DC mode again

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.UsingForceSenseSelectorSwitch.ABForceSenseSwitching
{
    public class ForceSenseSwitchSample
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
                ReadAllEvents.LogAllEvents(tcpSocket);

                // check that the Force Sense Selector Switch is available for this SpikeSafe. We need the switch to run this sequence
                // If switch related SCPI is sent and there is no switch configured, it will result in error "386, Output Switch is not installed"
                tcpSocket.SendScpiCommand("OUTP1:CONN:AVAIL?");
                string isSwitchAvailable = tcpSocket.ReadData();
                if (isSwitchAvailable != "Ch:1")
                    throw new Exception("Force Sense Selector Switch is not available, and is necessary to run this sequence.");

                // set the Force Sense Selector Switch state to Primary (A) so that the SpikeSafe can output to the DUT
                // the default switch state can be manually adjusted using SCPI, so it is best to send this command even after sending a *RST
                tcpSocket.SendScpiCommand("OUTP1:CONN PRI");

                // set Channel 1 settings to operate in DC mode
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP DC");    
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");    
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");        
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");       

                // log all SpikeSafe event after settings are adjusted  
                ReadAllEvents.LogAllEvents(tcpSocket); 

                // turn on Channel 1
                tcpSocket.SendScpiCommand("OUTP1 1");                                        

                // check for all events and measure readings on Channel 1 once per second for 10 seconds
                DateTime timeEnd = DateTime.Now.AddSeconds(10);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }                            
                
                // turn off Channel 1 and check for all events
                // When operating in DC mode, the channel must be turned off before adjusting the switch state
                tcpSocket.SendScpiCommand("OUTP1 0");               
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set the Force Sense Selector Switch state to Auxiliary (B) so that the Auxiliary Source will be routed to the DUT and the SpikeSafe will be disconnected
                tcpSocket.SendScpiCommand("OUTP1:CONN AUX");

                // Show a messageso any tasks using the Auxiliary source may be performed before adjusting the switch back to Primary
                // The SpikeSafe is not electrically connected to the DUT at this time
                Console.WriteLine("Force Sense Selector Switch is in Auxiliary (B) mode. Perform any tests using the auxiliary source, then press Enter adjust the switch back to Primary (A) mode.");
                Console.ReadLine();

                // set the Force Sense Selector Switch state to Primary (A) so that the SpikeSafe can output to the DUT
                tcpSocket.SendScpiCommand("OUTP1:CONN PRI");

                // turn on Channel 1
                tcpSocket.SendScpiCommand("OUTP1 1");                                        

                // check for all events and measure readings on Channel 1 once per second for 10 seconds
                timeEnd = DateTime.Now.AddSeconds(10);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }                   
                
                // turn off Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 0");               
                ReadAllEvents.LogAllEvents(tcpSocket);

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();                 

                _log.Info("ForceSenseSwitchSample.Run() completed.\n");
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