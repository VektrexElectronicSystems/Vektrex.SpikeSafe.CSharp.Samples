using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.DischargeChannel
{
    public class DischargeChannel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            try
            {
                Log.Info("DischargeChannel.cs started.");
                Log.Info("CLR version: {0}", Environment.Version);

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events
                tcpSocket.SendScpiCommand("*RST");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // parse the SpikeSafe information
                SpikeSafeInfo spikeSafeInfo = SpikeSafeInfoParser.Parse(tcpSocket, enableLogging: null);

                // set Channel 1's pulse mode to DC and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP DC");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's safety threshold for over current protection to 50% and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR:PROT 50");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's current to 100 mA and check for all events
                tcpSocket.SendScpiCommand($"SOUR1:CURR {Precision.GetPreciseCurrentCommandArgument(0.1)}");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's voltage to 20 V and check for all events
                double complianceVoltage = 20.0;
                tcpSocket.SendScpiCommand($"SOUR1:VOLT {Precision.GetPreciseComplianceVoltageCommandArgument(complianceVoltage)}");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // start test #1 by turning on Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 1");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // wait until the channel is fully ramped to target current
                ReadAllEvents.ReadUntilEvent(tcpSocket, (int)SpikeSafeEvents.CHANNEL_READY); // event 100 is "Channel Ready"

                // check for all events and measure readings on Channel 1 once per second for 5 seconds
                DateTime timeEnd = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < timeEnd)
                {
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }

                // turn off Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 0", enableLogging: true);
                ReadAllEvents.LogAllEvents(tcpSocket);

                // wait until the channel is fully discharged before starting test #2
                Log.Info("Waiting for Channel 1 to fully discharge after test #1...");
                Discharge.WaitForSpikeSafeChannelDischarge(
                    spikeSafeSocket: tcpSocket,
                    spikeSafeInfo: spikeSafeInfo,
                    complianceVoltage: complianceVoltage,
                    channelNumber: 1,
                    enableLogging: true);

                // start test #2 by turning on Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 1", enableLogging: true);
                ReadAllEvents.LogAllEvents(tcpSocket);

                // wait until the channel is fully ramped to target current
                ReadAllEvents.ReadUntilEvent(tcpSocket, (int)SpikeSafeEvents.CHANNEL_READY); // event 100 is "Channel Ready"

                // check for all events and measure readings on Channel 1 once per second for 5 seconds
                timeEnd = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < timeEnd)
                {
                    ReadAllEvents.LogAllEvents(tcpSocket);
                    MemoryTableReadData.LogMemoryTableRead(tcpSocket);
                    Threading.Wait(1);
                }

                // turn off Channel 1 and check for all events
                tcpSocket.SendScpiCommand("OUTP1 0", enableLogging: true);
                ReadAllEvents.LogAllEvents(tcpSocket);

                // wait until the channel is fully discharged before disconnecting the load
                Log.Info("Waiting for Channel 1 to fully discharge after test #2...");
                Discharge.WaitForSpikeSafeChannelDischarge(
                    spikeSafeSocket: tcpSocket,
                    spikeSafeInfo: spikeSafeInfo,
                    complianceVoltage: complianceVoltage,
                    channelNumber: 1,
                    enableLogging: true);

                // disconnect from SpikeSafe
                tcpSocket.Disconnect();

                Log.Info("DischargeChannel.cs completed.");
            }
            catch (SpikeSafeException ssErr)
            {
                string errorMessage = $"SpikeSafe error: {ssErr}\n";
                Log.Error(errorMessage);
                Console.Error.WriteLine(errorMessage);
                Environment.Exit(1);
            }
            catch (Exception err)
            {
                string errorMessage = $"Program error: {err}\n";
                Log.Error(errorMessage);
                Console.Error.WriteLine(errorMessage);
                Environment.Exit(1);
            }
        }
    }
}
