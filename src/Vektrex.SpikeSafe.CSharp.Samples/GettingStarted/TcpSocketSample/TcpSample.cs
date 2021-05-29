using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.TcpSocketSample
{
    public class TcpSample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            //// start of main program

            // parse IP Address and store for use in error messages
            IPAddress ipAddressObj = IPAddress.Parse(ipAddress);

            // create network end point
            IPEndPoint remoteEP = new IPEndPoint(ipAddressObj, portNumber);

            // create socket object
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // set default socket parameters
            socket.Blocking = true;
            socket.ReceiveTimeout = 2000;
            socket.SendTimeout = 2000;                           

            try
            {
                _log.Info("TcpSample.py started.");
                
                // connect to SpikeSafe
                socket.Connect(remoteEP);

                // define SpikeSafe SCPI command with line feed \n as an argument to send from socket   
                string argStr = "*IDN?\n";

                // convert argument to type byte, which is the format required by the socket                             
                byte[] msg = Encoding.ASCII.GetBytes(argStr);

                // send SpikeSafe SCPI command                     
                socket.Send(msg);

                // read SpikeSafe response and print it to the log file
                byte[] bytes = new byte[2048];                 
                int bytesRec = socket.Receive(bytes);
                string response = Encoding.ASCII.GetString(bytes, 0, bytesRec);        
                _log.Info(response);
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
            finally
            {
                // disconnect from SpikeSafe
                socket.Close();

                _log.Info("TcpSample.py completed.\n");
            }
        }
    }
}