using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunStaircaseSweep
{
    public class RunStaircaseSweepMode
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                double startCurrentAmps = 0.02;
                double stopCurrentAmps = 0.2;
                int currentStepCount = 10;
                double complianceVoltageVolts = 20;
                int stepOnTimeMilliseconds = 1;

                _log.Info("RunStaircaseSweep.Run() started.");

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // Parse SpikeSafe information for later use
                SpikeSafeInfo spikeSafeInfo = SpikeSafeInfoParser.Parse(tcpSocket, enableLogging: null);

                // set Channel 1's pulse mode to Staircase Sweep and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP STAIRCASESWEEP");

                // set Channel 1's Staircase Sweep parameters to match the test expectation
                tcpSocket.SendScpiCommand($"SOUR1:CURR:STA:SWE:STAR {Precision.GetPreciseCurrentCommandArgument(startCurrentAmps)}");
                tcpSocket.SendScpiCommand($"SOUR1:CURR:STA:SWE:STOP {Precision.GetPreciseCurrentCommandArgument(stopCurrentAmps)}");
                tcpSocket.SendScpiCommand($"SOUR1:CURR:STA:SWE:STEP {currentStepCount}");
                tcpSocket.SendScpiCommand($"SOUR1:PULS:STA:SWE:TON {stepOnTimeMilliseconds}");

                // set Channel 1's voltage
                tcpSocket.SendScpiCommand($"SOUR1:VOLT {Precision.GetPreciseComplianceVoltageCommandArgument(complianceVoltageVolts)}");

                // set Channel 1's compensation settings to High/Fast
                // For higher power loads or shorter pulses, these settings may have to be adjusted to obtain ideal pulse shape
                (SpikeSafeEnums.LoadImpedance loadImpedance, SpikeSafeEnums.RiseTime riseTime) = Compensation.GetOptimumCompensation(
                    spikesafeModelMaxCurrentAmps: spikeSafeInfo.MaximumSetCurrent,
                    setCurrentAmps: stopCurrentAmps,
                    pulseOnTimeSeconds: stepOnTimeMilliseconds / 1000.0);
                tcpSocket.SendScpiCommand($"SOUR1:PULS:CCOM {(int)loadImpedance}");
                tcpSocket.SendScpiCommand($"SOUR1:PULS:RCOM {(int)riseTime}");

                // set Channel 1's Ramp mode to Fast
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");

                // set Channel 1's External Source Trigger Out to Always
                tcpSocket.SendScpiCommand("SOUR1:PULS:TRIG ALWAYS");

                // set Channel 1's External Source Trigger Out to Positive

                tcpSocket.SendScpiCommand("OUTP1:TRIG:SLOP POS");

                // set Channel 1's trigger source to Internal (so that the SpikeSafe triggers the Staircase Sweep when the OUTP:TRIG command is sent)
                tcpSocket.SendScpiCommand("OUTP1:TRIG:SOUR INT");

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // Wait until Channel 1 is ready for a trigger command
                ReadAllEvents.ReadUntilEvent(tcpSocket, SpikeSafeEvents.CHANNEL_READY); // event 100 is "Channel Ready"

                // Output pulsed sweep for Channel 1
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Wait for the Staircase Sweep to be complete
                ReadAllEvents.ReadUntilEvent(tcpSocket, SpikeSafeEvents.STAIRCASE_SWEEP_IS_COMPLETED); // event 127 is "Staircase Sweep is completed"

                // Output pulsed sweep for Channel 1. Multiple sweeps can be run while the channel is enabled
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Wait for the Staircase Sweep to be complete
                ReadAllEvents.ReadUntilEvent(tcpSocket, SpikeSafeEvents.STAIRCASE_SWEEP_IS_COMPLETED); // event 127 is "Staircase Sweep is completed"

                // turn off Channel 1 after routine is complete
                tcpSocket.SendScpiCommand("OUTP1 0");

                // wait for Channel 1 to fully discharge to ensure safe conditions before re-starting channel or disconnecting the load
                Discharge.WaitForSpikeSafeChannelDischarge(
                    spikeSafeSocket: tcpSocket,
                    spikeSafeInfo: spikeSafeInfo,
                    complianceVoltage: complianceVoltageVolts,
                    channelNumber: 1);

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();

                _log.Info("RunStaircaseSweep.Run() completed.\n");
            }
            catch (SpikeSafeException e)
            {
                // print any SpikeSafe-specific error to both the terminal and the log file, then exit the application
                string errorMessage = string.Format("SpikeSafe error: {0}\n", e.Message);
                _log.Error(errorMessage);
                Console.WriteLine(errorMessage);
            }
            catch (Exception e)
            {
                // print any general exception to both the terminal and the log file, then exit the application
                string errorMessage = string.Format("Program error: {0}\n", e.Message);
                _log.Error(errorMessage);
                Console.WriteLine(errorMessage);
            }
        }
    }
}
