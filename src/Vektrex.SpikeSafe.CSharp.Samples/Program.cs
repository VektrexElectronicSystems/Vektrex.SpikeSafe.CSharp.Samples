using System;
using System.Text;
using Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadIdn;
using Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.TcpSocketSample;

namespace Vektrex.SpikeSafe.CSharp.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            string spikeSafeIpAddress = "10.0.0.220";
            int spikeSafePortNumber = 8282;

            // Uncomment line below to run TcpSocketSample
            new TcpSample().Run(spikeSafeIpAddress, spikeSafePortNumber);
            
            // Uncomment line below to run ReadIdn
            new ReadIdnExpanded().Run(spikeSafeIpAddress, spikeSafePortNumber);
        }
    }
}
