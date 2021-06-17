// Goal: 
// Connect to a SpikeSafe and run Pulsed Sweep mode on Channel 1 into an LED, Laser, or electrical component and run two complete Pulsed Sweeps
//
// Expectation: 
// Channel 1 will run a sweep from 20mA to 200mA, which will take 100ms. Expecting a low (<1V) forward voltage

using System;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunPulsedSweep
{
    public class RunPulsedSweepMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("RunPulsedSweepMode.Run() started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's pulse mode to Pulsed Sweep and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP PULSEDSWEEP");

                // set Channel 1's Pulsed Sweep parameters to match the test expectation
                tcpSocket.SendScpiCommand("SOUR1:CURR:STAR 0.02");
                tcpSocket.SendScpiCommand("SOUR1:CURR:STOP 0.2");   
                tcpSocket.SendScpiCommand("SOUR1:CURR:STEP 100");   

                // set Channel 1 to output one pulse per step
                tcpSocket.SendScpiCommand("SOUR1:PULS:COUN 1");

                // set Channel 1's voltage to 20 V 
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");   

                // set Channel 1's pulse settings for a 1% duty cycle and 1ms Period using the Pulse On Time and Pulse Off Time commands
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.0001");
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.0099");

                // set Channel 1's compensation settings to High/Fast
                // For higher power loads or shorter pulses, these settings may have to be adjusted to obtain ideal pulse shape
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 1");
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 1");   

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // Wait until Channel 1 is ready for a trigger command
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // Output pulsed sweep for Channel 1
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Wait for the Pulsed Sweep to be complete
                ReadAllEvents.ReadUntilEvent(tcpSocket, 109); // event 109 is "Pulsed Sweep Complete"

                // Output pulsed sweep for Channel 1. Multiple sweeps can be run while the channel is enabled
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Wait for the Pulsed Sweep to be complete
                ReadAllEvents.ReadUntilEvent(tcpSocket, 109); // event 109 is "Pulsed Sweep Complete"

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();    

                _log.Info("RunPulsedSweepMode.Run() completed.\n");
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