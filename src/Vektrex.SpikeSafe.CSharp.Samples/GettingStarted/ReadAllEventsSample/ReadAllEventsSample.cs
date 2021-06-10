using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadAllEventsSample
{
    public class ReadAllEventsSample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        
        public void Run(string ipAddress, int portNumber)
        {
            //// start of main program                   

            try
            {
                _log.Info("ReadAllEvents.Run() started.");
                
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();

                // connect to SpikeSafe
                tcpSocket.Connect(ipAddress, portNumber);
                
                // read all events in SpikeSafe event queue, store in list, and print them to the log file
                List<EventData> eventDataList = ReadAllEvents.ReadAllEventData(tcpSocket);
                foreach (EventData eventData in eventDataList)                        
                    _log.Info(eventData.Event);

                // disconnect from SpikeSafe
                tcpSocket.Disconnect();  

                _log.Info("ReadAllEvents.Run() completed.\n"); 
            }
            catch(SocketException e)
            {
                _log.Error(e.Message);
            }
            catch(TimeoutException e)
            {
                _log.Error(e.Message);
            }
            catch(Exception e)
            {
                _log.Error(e.Message);
            }
        }
    }
}