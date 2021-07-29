// Goal: 
// Demonstrate use of the alternate commands to adjust SpikeSafe Pulse On Time and Off Time
// Commands for Pulse Width, Duty Cycle, Pulse Period, and Pulse Hold will be demonstrated. Settings will be adjusted while running "dynamically"
//
// Expectation: 
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.UsingPulseHolds
{
    public class UsingPulseHolds
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("UsingPulseHolds.Run() started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and configure settings to run in Continuous Dynamic mode
                tcpSocket.SendScpiCommand("*RST");                  
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP PULSEDDYNAMIC");
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");   
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");   
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 4");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 4");   

                // initially setting the On and Off Time to their default values using the standard commands 
                // Although not recommended, it is possible to use On Time, Off Time, Pulse Width, Period, and Duty Cycle commands in the same test session
                // If On or Off Time is specified using these standard commands, the Pulse Hold will be ignored
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.001");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.009");

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until the channel is fully ramped
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // check for all events and measure readings on Channel 1 once per second for 5 seconds,
                // it is best practice to do this to ensure Channel 1 is on and does not have any errors
                DateTime timeEnd = DateTime.Now.AddSeconds(5);
                while (DateTime.Now <= timeEnd)
                {                       
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }

                // set Channel 1's Pulse Hold to Period. Setting any pulse-related setting will not re-calculate Pulse Period
                tcpSocket.SendScpiCommand("SOUR1:PULS:HOLD PER");
                logAndPrint("Held Pulse Period");

                // set Channel 1's Pulse Width to 8ms. Since Period is being held, the Period will remain at 10ms
                double pulseWidthSeconds = 0.008;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:WIDT {0}", pulseWidthSeconds));
                logAndPrint(string.Format("Set Pulse Width to {0}s", pulseWidthSeconds));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // set Channel 1's Duty Cycle to 50%. Since Period is being held, the Period will remain at 10ms
                double dutyCycle = 50;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:DCYC {0}", dutyCycle));
                logAndPrint(string.Format("Set Duty Cycle to {0}%", dutyCycle));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // set Channel 1's Duty Cycle to 0%. Using this alternate command set, the Duty Cycle is able to be set to 0% and 100%
                // Duty Cycle of 0% corresponds to an always-off output, similar to a disabled channel
                dutyCycle = 0;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:DCYC {0}", dutyCycle));
                logAndPrint(string.Format("Set Duty Cycle to {0}%", dutyCycle));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // set Channel 1's Duty Cycle to 100%. Using this alternate command set, the Duty Cycle is able to be set to 0% and 100%
                // Duty Cycle of 100% corresponds to an always-on output, similar to a DC mode
                dutyCycle = 100;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:DCYC {0}", dutyCycle));
                logAndPrint(string.Format("Set Duty Cycle to {0}%", dutyCycle));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // set Channel 1's Pulse Hold to Pulse Width. Setting any pulse-related setting will not re-calculate Pulse Width
                tcpSocket.SendScpiCommand("SOUR1:PULS:HOLD WIDT");
                logAndPrint("Held Pulse Width");

                // set Channel 1's Pulse Period to 20ms. Since Pulse Width is being held, the Pulse Width will remain at 10ms
                double pulsePeriodSeconds = 0.02;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:PER {0}", pulsePeriodSeconds));
                logAndPrint(string.Format("Set Pulse Period to {0}s", pulsePeriodSeconds));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // set Channel 1's Duty Cycle to 25%. Since Pulse Width is being held, the Pulse Width will remain at 10ms
                dutyCycle = 25;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:DCYC {0}", dutyCycle));
                logAndPrint(string.Format("Set Duty Cycle to {0}%", dutyCycle));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // set Channel 1's Pulse Hold to Duty Cycle. Setting any pulse-related setting will not re-calculate Duty Cycle
                tcpSocket.SendScpiCommand("SOUR1:PULS:HOLD DCYC");
                logAndPrint("Held Duty Cycle");

                // set Channel 1's Pulse Period to 200ms. Since Duty Cycle is being held, the Duty Cycle will remain at 25%
                pulsePeriodSeconds = 0.2;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:PER {0}", pulsePeriodSeconds));
                logAndPrint(string.Format("Set Pulse Period to {0}s", pulsePeriodSeconds));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // set Channel 1's Pulse Width to 1ms. Since Duty Cycle is being held, the Duty Cycle will remain at 25%
                pulseWidthSeconds = 0.001;
                tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:WIDT {0}", pulseWidthSeconds));
                logAndPrint(string.Format("Set Pulse Width to {0}s", pulseWidthSeconds));

                // verify that the expected updates are made to the pulse settings
                verifyCurrentPulseSettings(tcpSocket);
                
                // wait two seconds while running with the newly updated settings
                Threading.Wait(2);

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();   

                _log.Info("UsingPulseHolds.Run() completed.\n");
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

        private void logAndPrint(string messageString)
        {
            _log.Info(messageString);
            Console.WriteLine(messageString);
        }

        private void verifyCurrentPulseSettings(TcpSocket spikeSafeSocket)
        {
            spikeSafeSocket.SendScpiCommand("SOUR1:PULS:WIDT?");
            string pulseWidth = spikeSafeSocket.ReadData();
            logAndPrint(string.Format("Updated Pulse Width: {0}s", pulseWidth));

            spikeSafeSocket.SendScpiCommand("SOUR1:PULS:DCYC?");
            string dutyCycle = spikeSafeSocket.ReadData();
            logAndPrint(string.Format("Updated Duty Cycle: {0}%", dutyCycle));

            spikeSafeSocket.SendScpiCommand("SOUR1:PULS:PER?");
            string pulsePeriod = spikeSafeSocket.ReadData();
            logAndPrint(string.Format("Updated Pulse Period: {0}s", pulsePeriod));

            ReadAllEvents.LogAllEvents(spikeSafeSocket);

            // space out the log and terminal output for clarity
            logAndPrint(string.Empty);
        }
    }
}

