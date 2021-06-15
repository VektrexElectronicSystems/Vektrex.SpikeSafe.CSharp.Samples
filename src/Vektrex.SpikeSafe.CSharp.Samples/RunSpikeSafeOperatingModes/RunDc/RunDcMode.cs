// Goal: Connect to a SpikeSafe and run DC mode into an LED, Laser, or electrical component for 10 seconds
// Expectation: Channel 1 will be driven with 100mA with a forward voltage of <1V during this time

using System;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunDc
{
    public class RunDcMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("RunDcMode.cs started.");
                
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's pulse mode to DC and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP DC");    
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's safety threshold for over current protection to 50% and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");    
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's current to 100 mA and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");        
                ReadAllEvents.LogAllEvents(tcpSocket);  

                // set Channel 1's voltage to 10 V and check for all events
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");         
                ReadAllEvents.LogAllEvents(tcpSocket); 

                // turn on Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 1");               
                ReadAllEvents.LogAllEvents(tcpSocket);                            

                // wait until the channel is fully ramped to 10mA
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // check for all events and measure readings on Channel 1 once per second for 15 seconds,
                // it is best practice to do this to ensure Channel 1 is on and does not have any errors
                DateTime timeEnd = DateTime.Now.AddSeconds(10);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }                         
                
                // turn off Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 0");               
                ReadAllEvents.LogAllEvents(tcpSocket);

                // check Channel 1 is off
                MemoryTableReadData.LogMemoryTableRead(tcpSocket);

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();                  

                _log.Info("RunDcMode.cs completed.\n");
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