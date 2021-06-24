// Goal: 
// Make 10,000 pulses using software based timing and then shutoff pulsing

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.FixedPulseCountUsingSoftwareTiming
{
    public class FixedPulseCountUsingSoftwareTimingExample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("FixedPulseCountUsingSoftwareTimingExample.Run() started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's mode to DC Dynamic and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP PULSEDDYNAMIC");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Trigger Output to Positive and check for all events
                tcpSocket.SendScpiCommand("OUTP1:TRIG:SLOP POS");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Trigger Output Always and check for all events
                tcpSocket.SendScpiCommand("SOUR0:PULS:TRIG ALWAYS");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Pulse On Time to 1us and check for all events
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.000001");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Pulse Off Time 9us and check for all events
                tcpSocket.SendScpiCommand("SOUR1:PULS:TOFF 0.000009");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Pulse Width adjustment to disabled and check for all events
                tcpSocket.SendScpiCommand("SOUR1:PULS:AADJ 0");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's current to 100mA and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's voltage to 20V and check for all events
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Auto Range to On and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR:RANG:AUTO 1");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Load Impedance to High and check for all events
                tcpSocket.SendScpiCommand("SOUR1:PULS:CCOM 1");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Rise Time to Fast and check for all events
                tcpSocket.SendScpiCommand("SOUR1:PULS:RCOM 1");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Ramp mode to Fast and check for all events
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // Start the channel
                tcpSocket.SendScpiCommand("OUTP1 ON");

                // wait until Channel 1 is ready
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

                // pulsing starts before before getting Channel Ready message
                // wait 30ms for getting ~10000 pulses
                Threading.Wait(0.030);
                
                // disable Channel
                tcpSocket.SendScpiCommand("OUTP1 OFF");
                    
                // disconnect from SpikeSafe    
                tcpSocket.Disconnect();      

                _log.Info("FixedPulseCountUsingSoftwareTimingExample.Run() completed.\n");
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