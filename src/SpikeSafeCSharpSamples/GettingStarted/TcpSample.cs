using System;
using SpikeSafeCSharpLib;

namespace SpikeSafeCSharpSamples.GettingStarted
{
    public class TcpSample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run()
        {
            new TcpSocket().Connect();
            Console.WriteLine("TcpSample run()");
            _log.Info("TcpSample run()");
        }
    }
}