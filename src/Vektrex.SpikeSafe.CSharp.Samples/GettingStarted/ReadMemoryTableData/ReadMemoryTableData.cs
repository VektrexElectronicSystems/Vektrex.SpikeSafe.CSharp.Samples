using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadMemoryTableData
{
    public class ReadMemoryTableData
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            //// start of main program                   

            try
            {
                _log.Info("ReadMemoryTableData.Run() started.");
    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();                    
                tcpSocket.Connect(ipAddress, portNumber);
                
                // request SpikeSafe memory table
                tcpSocket.SendScpiCommand("MEM:TABL:READ");

                // read SpikeSafe memory table and print SpikeSafe response to the log file
                string data = tcpSocket.ReadData();                                            
                _log.Info(data);

                // parse SpikeSafe memory table
                MemoryTableReadData memory_table_read = MemoryTableReadData.Parse(data);
                
                // disconnect from SpikeSafe
                tcpSocket.Disconnect();   

                _log.Info("ReadAllEvents.Run() completed.\n");
            }
            catch(SpikeSafeException e)
            {
                string errorMessage = string.Format("SpikeSafe error: {0}", e.Message);
                _log.Error(errorMessage);
            }
            catch(Exception e)
            {
                string errorMessage = string.Format("Program error: {0}", e.Message);
                _log.Error(errorMessage);
            }
        }
    }
}