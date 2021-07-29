using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadIdn
{
    public class ReadIdnExpanded
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            //// start of main program                   

            try
            {
                _log.Info("ReadIdnExpanded.Run() started.");
                
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();

                // connect to SpikeSafe
                tcpSocket.Connect(ipAddress, portNumber);
                
                // request SpikeSafe information
                tcpSocket.SendScpiCommand("*IDN?");             
                
                // read SpikeSafe information
                string data = tcpSocket.ReadData();                    
                _log.Info("SpikeSafe *IDN? Response: {0}", data);

                // request if Digitizer is available (This is only available on PSMU and PSMU HC depending on model)
                tcpSocket.SendScpiCommand("VOLT:DIGI:AVAIL?");         

                // read Digitizer information
                data = tcpSocket.ReadData();
                _log.Info("SpikeSafe VOLT:DIGI:AVAIL? Response: {0}", data);
                if (data == "TRUE")
                {        
                    tcpSocket.SendScpiCommand("VOLT:VER?");             
                    string digitizerVersion = tcpSocket.ReadData();
                    tcpSocket.SendScpiCommand("VOLT:DATA:HWRE?");             
                    string digitizerHardwareRev = tcpSocket.ReadData();    
                    tcpSocket.SendScpiCommand("VOLT:DATA:SNUM?");             
                    string digitizerSerialNumber = tcpSocket.ReadData();        
                    tcpSocket.SendScpiCommand("VOLT:DATA:CDAT?");             
                    string digitizerCalibrationDate = tcpSocket.ReadData();                        

                    _log.Info("Digitizer Information Response: version={0}, HW Rev={1}, SN={2}, Cal Date={3}", digitizerVersion, digitizerHardwareRev, digitizerSerialNumber, digitizerCalibrationDate);
                }

                // request if Force Sense Selector Switch is available (This is only available on PSMU and PSMU HC depending on model)
                tcpSocket.SendScpiCommand("OUTP1:CONN:AVAIL?");

                // read Force Sense Selector Switch information
                data = tcpSocket.ReadData();
                _log.Info("SpikeSafe OUTP1:CONN:AVAIL? Response: {0}", data);

                // disconnect from SpikeSafe
                tcpSocket.Disconnect();  

                _log.Info("ReadIdnExpanded.Run() completed.\n"); 
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