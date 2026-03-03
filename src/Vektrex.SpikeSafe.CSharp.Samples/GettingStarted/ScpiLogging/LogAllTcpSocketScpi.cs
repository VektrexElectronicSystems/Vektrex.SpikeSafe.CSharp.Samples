using NLog;
using System;
using System.Collections.Generic;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ScpiLogging
{
    public class LogAllTcpSocketScpi
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            //// start of main program       

            try
            {
                _log.Info("LogAllTcpSocketScpi.Run() started.");

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();

                // set TcpSocket to log all SCPI
                tcpSocket.EnableLogging = true;

                // set TcpSocket to log all SCPI as info level messages
                tcpSocket.DefaultLogLevel = LogLevel.Info;

                // connect to SpikeSafe
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command
                tcpSocket.SendScpiCommand("*RST");

                // parse the SpikeSafe information
                SpikeSafeInfo spikeSafeInfo = SpikeSafeInfoParser.Parse(tcpSocket, enableLogging: null);

                // request SpikeSafe memory table
                tcpSocket.SendScpiCommand("MEM:TABL:READ");

                // read SpikeSafe memory table and print SpikeSafe response to the log file
                string data = tcpSocket.ReadData();
                _log.Info(data);

                // read all events in SpikeSafe event queue, store in list, and print them to the log file
                // here it's expected to receive 1 event: 102, External Pause Signal Ended
                List<EventData> eventDataList = ReadAllEvents.ReadAllEventData(tcpSocket);
                foreach (EventData eventData in eventDataList)
                    _log.Info(eventData.Event);

                // disconnect from SpikeSafe
                tcpSocket.Disconnect();

                _log.Info("LogAllTcpSocketScpi.Run() completed.");
            }
            catch (SpikeSafeException ssErr)
            {
                // print any SpikeSafe-specific error to the log file
                string errorMessage = $"SpikeSafe error: {ssErr}";
                _log.Error(errorMessage);
            }
            catch (Exception err)
            {
                // print any general exception to the log file
                string errorMessage = $"Program error: {err}";
                _log.Error(errorMessage);
            }
        }
    }
}