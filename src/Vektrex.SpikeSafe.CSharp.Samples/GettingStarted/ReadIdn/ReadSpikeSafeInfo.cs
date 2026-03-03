using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadIdn
{
    public class ReadSpikeSafeInfo
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            try
            {
                _log.Info("ReadSpikeSafeInfo.Run() started.");

                // import spikesafe library and connect
                var tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // "*RST" - reset to a known state (does not affect "VOLT" commands)
                tcpSocket.SendScpiCommand("*RST");

                // Parse the SpikeSafe information and print it to the log file
                SpikeSafeInfo spikeSafeInfo = SpikeSafeInfoParser.Parse(tcpSocket, enableLogging: null);

                // log the SpikeSafe information. To access an attribute, use the dot operator (e.g., spikeSafeInfo.Idn)
                _log.Info(spikeSafeInfo.Idn);

                // log the information for each digitizer. To access an attribute, use the dot operator (e.g., digitizer.version)
                foreach (DigitizerInfo digitizerInfo in spikeSafeInfo.DigitizerInfos)
                    _log.Info(digitizerInfo.Version);

                // Disconnect
                tcpSocket.Disconnect();

                _log.Info("ReadSpikeSafeInfo.Run() completed.");
            }

            catch (SocketException e)
            {
                _log.Error(e.Message);
            }
            catch (TimeoutException e)
            {
                _log.Error(e.Message);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }
    }
}
