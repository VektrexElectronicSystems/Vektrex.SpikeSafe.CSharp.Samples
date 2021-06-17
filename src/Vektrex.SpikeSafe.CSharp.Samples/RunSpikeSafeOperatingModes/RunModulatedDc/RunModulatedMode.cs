// Goal: 
// Connect to a SpikeSafe and run Modulated DC mode into an LED, Laser, or electrical component with a custom pulse sequence
//
// Expectation: 
// Channel 1 will be driven with varying current levels up to 150mA, specified within SCPI commands

using System;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunModulatedDc
{
    public class RunModulatedMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("RunModulatedMode.Run() started.");
                
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's pulse mode to Modulated DC
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP MODULATED");    

                // set Channel 1's current to 200 mA. This will be the output current when a sequence step specifies "@100"
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.2");        

                // set Channel 1's voltage to 20 V
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20"); 

                // set Channel 1's modulated sequence to a DC staircase with 5 steps
                // There are 5 current steps that each last for 1 second: 40mA, 80mA, 120mA, 160mA, and 200mA
                tcpSocket.SendScpiCommand("SOUR1:SEQ 1(1@20,1@40,1@60,1@80,1@100)"); 

                // Log all events since all settings are sent
                ReadAllEvents.LogAllEvents(tcpSocket); 

                // turn on Channel 1
                tcpSocket.SendScpiCommand("OUTP1 1");                                         

                // Wait until channel is ready for a trigger command
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // Output modulated sequence
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Wait until channel has completed it modulated sequence
                ReadAllEvents.ReadUntilEvent(tcpSocket, 105); // event 105 is "Modulated SEQ completed"

                // turn off Channel 1
                tcpSocket.SendScpiCommand("OUTP1 0");      

                // set Channel 1's modulated sequence to an infinite pulsing pattern. This pulsing pattern will repeatedly perform 3 steps:
                //       1.) it will pulse Off for 250ms, then On for 250ms at 120mA. This will happen twice
                //       2.) it will pulse Off for 500ms, then On for 500ms at 60mA. This will also happen twice 
                //       3.) for one second, 180mA will be outputted
                tcpSocket.SendScpiCommand("SOUR1:SEQ *(2(.25@0,.25@60),2(.5@0,.5@30),1@90)");          

                // turn on Channel 1
                tcpSocket.SendScpiCommand("OUTP1 1");                                         

                // Wait until channel is ready for a trigger command
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // Output modulated sequence
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // check for all events and measure readings on Channel 1 once per second for 10 seconds,
                // it is best practice to do this to ensure Channel 1 is on and does not have any errors
                DateTime timeEnd = DateTime.Now.AddSeconds(10);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }                                   
                
                // turn off Channel 1. Since the sequence runs indefinitely, we do not wait for a "Modulated SEQ completed" message
                tcpSocket.SendScpiCommand("OUTP1 0");      

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();   

                _log.Info("RunModulatedMode.Run() completed.\n");
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