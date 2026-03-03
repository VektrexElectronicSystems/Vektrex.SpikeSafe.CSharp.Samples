// Goal: 
// Connect to a SpikeSafe and read all events

using System;
using System.Collections.Generic;
using NLog;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ScpiLogging
{
    public class LogSpecificTcpSocketScpi
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            //// start of main program

            try
            {
                _log.Info("LogSpecificTcpSocketScpi.Run() started.");

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();

                // set TcpSocket to log no SCPI
                tcpSocket.EnableLogging = false;

                // set TcpSocket to log all SCPI as info level messages
                tcpSocket.DefaultLogLevel = LogLevel.Info;

                // connect to SpikeSafe
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");

                // parse the SpikeSafe information
                SpikeSafeInfo spikeSafeInfo = SpikeSafeInfoParser.Parse(tcpSocket, enableLogging: null);

                // request SpikeSafe memory table but do not print SCPI to file
                tcpSocket.SendScpiCommand("MEM:TABL:READ", enableLogging: false);

                // read SpikeSafe memory table and print SpikeSafe response to the log file
                string data = tcpSocket.ReadData(enableLogging: true);
                _log.Info(data);

                // read all events in SpikeSafe event queue, store in list, and print them to the log file
                // here it's expected to receive 1 event: 102, External Pause Signal Ended
                List<EventData> eventData = ReadAllEvents.ReadAllEventData(tcpSocket, enableLogging: true);
                foreach (EventData ev in eventData)
                    _log.Info(ev.Event);

                // disconnect from SpikeSafe
                tcpSocket.Disconnect();

                _log.Info("LogSpecificTcpSocketScpi.Run() completed.");
            }
            catch (SpikeSafeException ssErr)
            {
                // print any SpikeSafe-specific error to both the terminal and the log file, then exit the application
                string error_message = $"SpikeSafe error: {ssErr}";
                _log.Error(error_message);
                Console.WriteLine(error_message);
            }
            catch (Exception err)
            {
                // print any general exception to both the terminal and the log file, then exit the application
                string error_message = $"Program error: {err}";
                _log.Error(error_message);
                Console.WriteLine(error_message);
            }
        }
    }
}