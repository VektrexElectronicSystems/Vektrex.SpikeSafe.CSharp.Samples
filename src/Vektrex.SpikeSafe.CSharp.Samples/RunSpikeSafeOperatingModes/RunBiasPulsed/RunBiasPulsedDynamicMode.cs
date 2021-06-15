// Goal: 
// Connect to a SpikeSafe and run Pulsed mode into a 10Ω resistor. Take voltage measurements from the pulsed output using the SpikeSafe PSMU's integrated Digitizer
// 
// Expectation: 
// Channel 1 will be driven with 100mA with a forward voltage of ~1V during this time

using System;
using System.Collections.Generic;
using System.Drawing;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunBiasPulsed
{
    public class RunBiasPulsedDynamicMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("RunBiasPulsedDynamicMode.Run() started.");

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // Synchronize rising edge of all channels
                tcpSocket.SendScpiCommand("SOUR1:PULS:STAG 0");   

                // set Channel 1's pulse mode to Pulsed Dynamic and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP BIASPULSEDDYNAMIC");

                // set Channel 1's current to 100 mA
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");   

                // set Channel 1's voltage to 10 V 
                tcpSocket.SendScpiCommand("SOUR1:VOLT 30"); 

                // set Channel 1's bias current to 20 mA and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR:BIAS 0.02");   

                // set Channel 1's Pulse On Time to 1ms
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.001");

                // set Channel 1's Pulse Off Time to 9ms
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.009");

                // set Channel 1's compensation settings to their default values
                // For higher power loads or shorter pulses, these settings may have to be adjusted to obtain ideal pulse shape
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");   

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until the channel is fully ramped
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // check for all events and measure readings on Channel 1 once per second for 10 seconds,
                // it is best practice to do this to ensure Channel 1 is on and does not have any errors
                DateTime timeEnd = DateTime.Now.AddSeconds(10);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }                     

                // set Channel 1's current to 200 mA while running
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.2");      

                // set Channel 1's Pulse On Time to 100µs dynamically while channel is operating. Check events and measure readings
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.0001");
                ReadAllEvents.LogAllEvents(tcpSocket);
                MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                Threading.Wait(1);

                // set Channel 1's Pulse Off Time to 100µs dynamically while channel is operating. Check events and measure readings
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.0001");

                // after dynamically applying all new settings, check for all events and measure readings on Channel 1 once per second for 5 seconds
                timeEnd = DateTime.Now.AddSeconds(5);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }             
                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                _log.Info("RunBiasPulsedDynamicMode.Run() completed.\n");   
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