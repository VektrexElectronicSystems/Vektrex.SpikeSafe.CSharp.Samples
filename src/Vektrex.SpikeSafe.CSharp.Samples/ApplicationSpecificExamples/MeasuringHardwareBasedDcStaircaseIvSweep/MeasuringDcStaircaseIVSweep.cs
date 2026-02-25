using ScottPlot;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vektrex.SpikeSafe.CSharp.Lib;
using static ScottPlot.Generate;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MeasuringHardwareBasedDcStaircaseIvSweep
{
    public class MeasuringDcStaircaseIVSweep
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber, string dmmIpAddress = "10.0.0.240", int dmmPortNumber = 5025)
        {
            // start of main program
            try
            {
                double startCurrentAmps = 0.02;
                double stopCurrentAmps = 0.2;
                int currentStepCount = 10;
                double complianceVoltageVolts = 20;
                int stepOnTimeMilliseconds = 2;

                double digitizerVoltageRangeVolts = 10;
                int digitizerApertureMicroseconds = 500;
                int digitizerHardwareTriggerDelayMicroseconds = 1000;
                int digitizerHardwareTriggerCount = currentStepCount;
                int digitizerReadingCount = 1;

                // DMM trigger timeout in seconds with added 10% margin to expected DMM trigger time. This is the maximum time to wait for the DMM to complete its triggered measurements
                double dmmTriggerTimeoutSeconds = (stepOnTimeMilliseconds * currentStepCount) + stepOnTimeMilliseconds * currentStepCount * 0.1;

                // DMM current range setting based on the largest test current, matching SpikeSafe's Stop Current
                double dmmCurrentRange = 1;
                if (stopCurrentAmps < 0.00001)
                    dmmCurrentRange = 0.00001;
                else if (stopCurrentAmps < 0.0001)
                    dmmCurrentRange = 0.0001;
                else if (stopCurrentAmps < 0.001)
                    dmmCurrentRange = 0.001;
                else if (stopCurrentAmps < 0.01)
                    dmmCurrentRange = 0.01;
                else if (stopCurrentAmps < 0.1)
                    dmmCurrentRange = 0.1;
                else
                    dmmCurrentRange = 1;

                // DMM aperture in seconds
                double dmmApertureSeconds = digitizerApertureMicroseconds / 1_000_000.0;

                // DMM hardware trigger delay in seconds, matching Digitizer's hardware trigger delay
                double dmmHardwareTriggerDelaySeconds = digitizerHardwareTriggerDelayMicroseconds / 1_000_000.0;

                // DMM trigger count, matching Digitizer's Trigger Count
                int dmmHardwareTriggerCount = digitizerHardwareTriggerCount;

                // DMM reading count, matching Digitizer's Reading Count
                int dmmReadingCount = digitizerReadingCount;

                _log.Info("MeasuringDcStaircaseIVSweep.Run() started.");

                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events, this will automatically abort digitizer in order get it into a known state
                // This is good practice when connecting to a SpikeSafe PSMU, and is best practice to check for errors after sending each command        
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

                // set Digitizer voltage range to 10V since we expect to measure voltages significantly less than 10V
                tcpSocket.SendScpiCommand($"VOLT:RANG {digitizerVoltageRangeVolts}");

                // set Digitizer aperture. Aperture specifies the measurement time, and we want to measure a majority of the pulse's constant current output
                tcpSocket.SendScpiCommand($"VOLT:APER {Precision.GetPreciseTimeMicrosecondsCommandArgument(digitizerApertureMicroseconds)}");

                // set Digitizer trigger delay. We want to give sufficient delay to omit any overshoot the current pulse may have
                tcpSocket.SendScpiCommand($"VOLT:TRIG:DEL {Precision.GetPreciseTimeMicrosecondsCommandArgument(digitizerHardwareTriggerDelayMicroseconds)}");

                // set Digitizer trigger source to hardware. When set to a hardware trigger, the digitizer waits for a trigger signal from the SpikeSafe to start a measurement
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR HARDWARE");

                // set Digitizer trigger edge to rising. The Digitizer will start a measurement after the SpikeSafe's rising pulse edge occurs
                tcpSocket.SendScpiCommand("VOLT:TRIG:EDGE RISING");

                // set Digitizer trigger count. We want to take one voltage reading for every step in the pulsed sweep
                tcpSocket.SendScpiCommand($"VOLT:TRIG:COUN {digitizerHardwareTriggerCount}");

                // set Digitizer reading count. This is the amount of readings that will be taken when the Digitizer receives its specified trigger signal
                tcpSocket.SendScpiCommand($"VOLT:READ:COUN {digitizerReadingCount}");

                // check all SpikeSafe event since all settings have been sent
                ReadAllEvents.LogAllEvents(tcpSocket);

                // instantiate new TcpSocket to connect to DMM
                TcpSocket dmmTcpSocket = new TcpSocket();
                dmmTcpSocket.Connect(dmmIpAddress, dmmPortNumber);

                // DMM may require a few seconds to process commands, extend the timeout to avoid unintentional connection errors
                dmmTcpSocket.TimeoutMilliSeconds = 10000;

                // Log DMM information
                dmmTcpSocket.SendScpiCommand("*IDN?");
                string dmmIdn = dmmTcpSocket.ReadData();
                _log.Info($"DMM *IDN?: {dmmIdn}");
                dmmTcpSocket.SendScpiCommand("*LANG SCPI");

                // Set time on DMM to match computer's
                System.DateTime dateTime = System.DateTime.Now;
                dmmTcpSocket.SendScpiCommand($"SYST:TIME {dateTime.Hour}, {dateTime.Minute}, {dateTime.Second}");
                dmmTcpSocket.SendScpiCommand("*RST");
                dmmTcpSocket.SendScpiCommand("*CLS");

                // Set sense function
                dmmTcpSocket.SendScpiCommand("FUNC 'CURR:DC'");

                // Set aperture
                dmmTcpSocket.SendScpiCommand($"CURR:APER {dmmApertureSeconds}");

                // Set auto-zero
                dmmTcpSocket.SendScpiCommand("CURR:AZER OFF");

                // Set auto-delay
                dmmTcpSocket.SendScpiCommand("CURR:DEL:AUTO OFF");

                // Set range (either SENS:CURR:RANG:AUTO ON, or CURR:RANG <0.00001, 0.0001,.001, .01, .1, 1, 3, or 10>)
                dmmTcpSocket.SendScpiCommand($"CURR:RANG {dmmCurrentRange}");

                // Set relative offset state (controls relative offset value to the measurement, if ON send CURR:REL: <value in amps>)
                dmmTcpSocket.SendScpiCommand("CURR:REL:STAT OFF");

                // Clear existing trigger model
                dmmTcpSocket.SendScpiCommand("TRIG:LOAD \"Empty\"");

                // Set Trigger Block 1, clears defbuffer1 at beginning of execution
                dmmTcpSocket.SendScpiCommand("TRIG:BLOC:BUFF:CLE 1");

                // Set Trigger Block 2, wait for EXT trigger event, and clear previously detected trigger events when entering wait block (ENT)
                dmmTcpSocket.SendScpiCommand("TRIG:BLOC:WAIT 2, EXT, ENT");

                // Set EXT trigger to rising edge (or Trigger Slope)
                dmmTcpSocket.SendScpiCommand("TRIG:EXT:IN:EDGE RIS");

                // Set Trigger Block 3, delay for n seconds (or Trigger Delay)
                dmmTcpSocket.SendScpiCommand($"TRIG:BLOC:DEL:CONS 3, {dmmHardwareTriggerDelaySeconds}");

                // Set Trigger Block 4, use buffer defbuffer1 to store readings, and take n readings (or Reading count)
                dmmTcpSocket.SendScpiCommand($"TRIG:BLOC:MEAS 4, \"defbuffer1\", {dmmReadingCount}");

                // Set Trigger Block 5, loop n more time (or number of Triggers) back to block 2
                dmmTcpSocket.SendScpiCommand($"TRIG:BLOC:BRAN:COUN 5, {dmmHardwareTriggerCount}, 2");

                // check for DMM errors
                // initialize flag to check if DMM event queue is empty 
                bool isDmmEventQueueEmpty = false;
                // run as long as there is an event in the DMM queue
                while (isDmmEventQueueEmpty == false)
                {
                    // request DMM events and read data 
                    dmmTcpSocket.SendScpiCommand("SYST:ERR?");
                    string eventResponse = dmmTcpSocket.ReadData();

                    // event queue is empty
                    if (eventResponse.StartsWith("0"))
                        isDmmEventQueueEmpty = true;
                    else
                        // add event to event queue
                        throw new Exception($"DMM exception: {eventResponse}");
                }

                // start Digitizer measurements. We want the digitizer waiting for triggers before starting the pulsed sweep
                tcpSocket.SendScpiCommand("VOLT:INIT");

                // start DMM measurements. We want the DMM waiting for triggers before starting the Staircase Sweep
                dmmTcpSocket.SendScpiCommand("INIT");

                // turn on Channel 1 
                tcpSocket.SendScpiCommand("OUTP1 1");

                // wait until Channel 1 is fully ramped so we can send a trigger command for a pulsed sweep
                ReadAllEvents.ReadUntilEvent(tcpSocket, SpikeSafeEvents.CHANNEL_READY); // event 100 is "Channel Ready"

                // trigger Channel 1 to start the pulsed sweep output
                tcpSocket.SendScpiCommand("OUTP1:TRIG");

                // Get estimated completion time for Digitizer measurements to occur. Estimating completion time minimizes digitizer polling during DigitizerDataFetch.
                double estimatedCompleteTimeSeconds = DigitizerDataFetch.GetNewVoltageDataEstimatedCompleteTime(
                    apertureMicroseconds: digitizerApertureMicroseconds,
                    readingCount: digitizerReadingCount,
                    hardwareTriggerCount: digitizerHardwareTriggerCount,
                    hardwareTriggerDelayMicroseconds: digitizerHardwareTriggerDelayMicroseconds);

                // fetch voltage data from Digitizer containing sample number and voltage reading
                List<DigitizerData> digitizerData = null;

                try
                {
                    // wait for the Digitizer measurements to complete. We need to wait for the data acquisition to complete before fetching the data
                    DigitizerDataFetch.WaitForNewVoltageData(
                        spikeSafeSocket: tcpSocket,
                        waitTime: estimatedCompleteTimeSeconds,
                        enableLogging: null,
                        timeout: 10,
                        digitizerNumber: null);

                    // fetch complete data
                    digitizerData = DigitizerDataFetch.FetchVoltageData(
                        spikeSafeSocket: tcpSocket,
                        enableLogging: null,
                        digitizerNumber: null);

                    _log.Info("Complete VOLT:FETC? Response returned with {0} readings", digitizerData.Count);
                }
                catch (Exception err)
                {
                    _log.Error("Complete VOLT:FETC? Response error: {0}", err.Message);

                    try
                    {
                        // on timeout or error, abort the digitizer and fetch any partial readings

                        // attempt to abort partial digitizer readings
                        tcpSocket.SendScpiCommand("VOLT:ABOR:PART", tcpSocket.EnableLogging);

                        // wait for the Digitizer partial measurements to complete. It's expected that the wait time here will be small since we are fetching partial data after an abort.
                        DigitizerDataFetch.WaitForNewVoltageData(
                            spikeSafeSocket: tcpSocket,
                            waitTime: 0.01,
                            enableLogging: null,
                            timeout: 10,
                            digitizerNumber: null);

                        // fetch whatever data is available
                        digitizerData = DigitizerDataFetch.FetchVoltageData(
                            spikeSafeSocket: tcpSocket,
                            enableLogging: null,
                            digitizerNumber: null);

                        _log.Info("Partial VOLT:FETC? Response after error returned with {0} readings", digitizerData.Count);

                        // check if partial measurements were a result of a SpikeSafe error
                        ReadAllEvents.LogAllEvents(tcpSocket);
                    }
                    catch (TimeoutException e)
                    {
                        // Timeout error will occur if no partial measurements were taken
                        _log.Error("Partial VOLT:FETC? Response error: {0}", e.Message);

                        // check if no partial measurements were a result of a SpikeSafe error
                        ReadAllEvents.ReadAllEventData(tcpSocket);
                    }
                    catch (Exception)
                    {
                        // All other errors, exit the script
                        throw;
                    }
                }

                // initialize flag to check if DMM trigger state is idle
                bool isDmmIdleState = false;

                System.DateTime startTime = System.DateTime.Now;

                List<double> dmmReadings = new List<double>();

                // monitor DMM trigger state machine and check for errors until all readings are taken
                while (isDmmIdleState == false)
                {
                    // Calculate the elapsed time
                    System.TimeSpan elapsed_time = System.DateTime.Now - startTime;

                    // Check if the elapsed time exceeds the DMM trigger timeout
                    if (elapsed_time.TotalSeconds > dmmTriggerTimeoutSeconds)
                        throw new Exception($"DMM exception: DMM triggered measurement has not completed in {dmmTriggerTimeoutSeconds} seconds. Check if DMM trigger cable is functioning properly");

                    // request DMM trigger state and read data 
                    dmmTcpSocket.SendScpiCommand("TRIG:STAT?");
                    string triggerStateResponse = dmmTcpSocket.ReadData();

                    // DMM trigger state is idle
                    if (triggerStateResponse.StartsWith("IDLE"))
                        isDmmIdleState = true;

                    // initialize flag to check if DMM event queue is empty 
                    isDmmEventQueueEmpty = false;

                    // run as long as there is an event in the DMM queue
                    while (isDmmEventQueueEmpty == false)
                    {
                        // request DMM events and read data 
                        dmmTcpSocket.SendScpiCommand("SYST:ERR?");
                        string event_response = dmmTcpSocket.ReadData();

                        // event queue is empty
                        if (event_response.StartsWith("0"))
                            isDmmEventQueueEmpty = true;
                        else
                            // add event to event queue
                            throw new Exception($"DMM exception: {event_response}");
                    }

                    // Read DMM data
                    _log.Info("Reading DMM measurement...");

                    // return the number of readings in buffer
                    dmmTcpSocket.SendScpiCommand("TRAC:ACT?");
                    string dmmDataPointTotalCount = dmmTcpSocket.ReadData();

                    // return the starting index
                    dmmTcpSocket.SendScpiCommand("TRAC:ACT:STAR?");
                    string dmmDataPointStartIndex = dmmTcpSocket.ReadData();

                    // return the end index
                    dmmTcpSocket.SendScpiCommand("TRAC:ACT:END?");
                    string dmmDataPointEndIndex = dmmTcpSocket.ReadData();

                    // return the array of data
                    dmmTcpSocket.SendScpiCommand($"TRAC:DATA? {dmmDataPointStartIndex}, {dmmDataPointEndIndex}, \"defbuffer1\", READ");
                    string dmmDataPoints = dmmTcpSocket.ReadData();

                    // split array and separate with commas
                    string[] dmmDataPointsSplit = dmmDataPoints.Split(',');
                    foreach (string dmmDataPoint in dmmDataPointsSplit)
                    {
                        if (double.TryParse(dmmDataPoint, out double dmmCurrentReading))
                        {
                            dmmReadings.Add(dmmCurrentReading);
                        }
                    }

                    // Wait for the Staircase Sweep to be complete
                    ReadAllEvents.ReadUntilEvent(tcpSocket, SpikeSafeEvents.STAIRCASE_SWEEP_IS_COMPLETED); // event 127 is "Staircase Sweep is completed"
                }

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

                // put the fetched data in a plottable data format
                var plt = new ScottPlot.Plot();
                List<double> voltageReadings = new List<double>();
                List<double> currentSteps = new List<double>();
                double sweepStepSizeAmps = (stopCurrentAmps - startCurrentAmps) / (currentStepCount - 1);
                List<double> currentReadings = dmmReadings;
                foreach (DigitizerData dd in digitizerData)
                {
                    voltageReadings.Add(dd.VoltageReading);
                    currentSteps.Add(startCurrentAmps + sweepStepSizeAmps * (dd.SampleNumber - 1));
                }

                // convert to arrays for plotting
                double[] xs = currentSteps.ToArray();
                double[] ysCurrent = currentReadings.ToArray();
                double[] ysVoltage = voltageReadings.ToArray();

                // primary (left) Y axis for DMM current
                var leftAxis = plt.Axes.Left;
                var rightAxis = plt.Axes.Right; // secondary (right) Y axis for voltage

                var currentLine = plt.Add.ScatterLine(xs, ysCurrent);
                currentLine.Color = Colors.Blue;
                currentLine.LineWidth = 1;
                currentLine.Axes.YAxis = leftAxis;

                // voltage on right Y axis
                var voltageLine = plt.Add.ScatterLine(xs, ysVoltage);
                voltageLine.Color = Colors.Red;
                voltageLine.LineWidth = 1;
                voltageLine.Axes.YAxis = rightAxis;

                // axis labels and title
                plt.XLabel("Set Current (A)");
                leftAxis.Label.Text = "DMM Current (A)";
                rightAxis.Label.Text = "Digitizer Voltage (V)";
                plt.Title($"2 ms Staircase L-I-V ({startCurrentAmps}A to {stopCurrentAmps}A)");

                // save figure
                plt.SavePng(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "staircase_sweep_liv.png"), 800, 600);

                _log.Info("MeasuringDcStaircaseIVSweep.Run() completed.\n");
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
