# Vektrex.SpikeSafe.CSharp.Lib Releases

## v1.7.8
6/17/26
- Added
    - Compensation.GetCustomCompensation()
        - New function that returns the custom compensation for a given set current, device type, and custom compensation table, and optionally a given pulse on time. The custom compensation table may be defined using a list of CustomCompensationEntry objects.
    - SpikeSafeEvents
        - Added new events:
            - INVALID_CURRENT_RAMP_RATE_SETTINGS = 604
            - BIAS_CURRENT_SETTING_IS_ONLY_ALLOWED_IN_BIAS_MODE_ON_CHANNEL_1 = 605
- Fixed
    - SpikeSafeInfoParser.Parse()
        - Works when there are no Digitizers available
- Improved
    - TcpSocket.Connect()
        - Disabled Nagle's algorithm to improve SCPI round trip timing

## v1.6.2
4/8/26

- Updated
    - Discharge.WaitForSpikeSafeChannelDischarge()
        - Function assesses Discharge Complete query response before checking if elapsed time has surpassed the model's maximum expected discharge time.
        - Error message when the elapsed time has surpassed the model's maximum expected discharge time now includes the expected times for both the compliance voltage and model's maximum compliance voltage.

## v1.5.32
3/2/26

- Breaking Changes
    - PulseWidthCorrection.GetOptimumPulseWidthCorrection()
        - Added required parameters:
            - SpikeSafeInfo spikeSafeInfo. An object containing the SpikeSafe information.
        - Removed required parameters:
            - float spikeSafeModelMaxCurrentAmps. Maximum current of the SpikeSafe model.
- Added
    - DigitizerData.TimeSinceStartSeconds
    - DigitizerDataFetch.FetchVoltageDataSamplingModeLinear()
    - DigitizerDataFetch.FetchVoltageDataSamplingModeLogarithmic()
    - DigitizerDataFetch.FetchVoltageDataSamplingModeCustom()
    - DigitizerEnums class
    - DigitizerVfCustomSequence class
    - DigitizerVfCustomSequenceStep class
    - Discharge.WaitForSpikeSafeChannelDischarge()
        - Added optional parameter bool? enableLogging. Overrides spikeSafeSocket.EnableLogging attribute (default to null will use spikeSafeSocket.EnableLogging value)
    - ReadAllEvents.ReadUntilEvent()
        - Added parameter code as type SpikeSafeEvent enum
        - Added optional parameter float timeout. Maximum time in seconds to wait for the desired event before raising an exception (default to None will wait indefinitely)
    - ScpiFormatter class
    - SpikeSafeEvents
        - Added new events:
            - CURRENT_RAMP_RATE_RESTORED = 126
            - STAIRCASE_SWEEP_IS_COMPLETED = 127
            - STAIRCASE_SWEEP_SHUTDOWN_DUE_TO_ERROR = 128
            - INVALID_STAIRCASE_SWEEP_ON_TIME = 602
            - INVALID_STAIRCASE_SWEEP_STEP_COUNT = 603
    - SpikeSafeInfoParser.CompareRevVersion()
    - SpikeSafeInfoParser.Parse()
        - optional parameter bool? enableLogging. Overrides spikeSafeSocket.EnableLogging attribute (default to null will use spikeSafeSocket.EnableLogging value)
- Fixed
    - DigitizerDataFetch.WaitForNewVoltageData()
        - Calculation now returns the correct time, which before returned a time larger by a factor of 10.
- Dependency Updates:
    - NLog 6.0.7

## v1.5.11
1/26/26

- Added
    - ChannelData.CurrentReadingAmpsFormattedFloat()
    - ChannelData.CurrentReadingAmpsFormattedString()
    - ChannelData.VoltageReadingVoltsFormattedFloat()
    - ChannelData.VoltageReadingVoltsFormattedString()
    - DigitizerData.VoltageReadingVoltsFormattedFloat()
    - DigitizerData.VoltageReadingVoltsFormattedString()
    - DigitizerDataFetch.FetchVoltageData(
            TcpSocket spikeSafeSocket,
            bool? enableLogging = null,
            int? digitizerNumber = null)
        - Added optional parameters:
            - digitizerNumber: The Digitizer number to fetch from. If null, fetches from Digitizer 1 ("VOLT").
    - DigitizerDataFetch.GetNewVoltageDataEstimatedCompleteTime(
            int apertureMicroseconds,
            int readingCount,
            int? hardwareTriggerCount = null,
            int? hardwareTriggerDelayMicroseconds = null)
    - DigitizerDataFetch.WaitForNewVoltageData(
            TcpSocket spikeSafeSocket,
            double waitTime = 0.01,
            bool? enableLogging = null,
            double? timeout = null,
            int? digitizerNumber = null)
        - Added optional parameters:
            - waitTime: Wait time in seconds between each VOLT:NDAT? query.
            - timeout: Timeout in seconds for waiting for new data. When null, waits indefinitely, otherwise a TimeoutException is thrown when the timeout is reached.
            - digitizerNumber: The Digitizer number to query. When null, queries Digitizer 1 (prefix "VOLT").
        - Now returns when Digitizer partial data is ready as well as full data
    - Discharge.WaitForSpikeSafeChannelDischarge(TcpSocket spikeSafeSocket, SpikeSafeInfo spikeSafeInfo, double complianceVoltage, int channelNumber = 1)
- Updated
    - PulseWidthCorrection.GetOptimumPulseWidthCorrection(double spikeSafeModelMaxCurrentAmps, double setCurrentAmps, LoadImpedance loadImpedance, RiseTime riseTime)
        - Upper limit reduced from 50us to 9us
- Obsoleted
    - DigitizerDataFetch.FetchVoltageData(TcpSocket tcpSocket)
        - Replaced by DigitizerDataFetch.FetchVoltageData(
            TcpSocket spikeSafeSocket,
            bool? enableLogging = null,
            int? digitizerNumber = null)
    - DigitizerDataFetch.FetchVoltageData(TcpSocket tcpSocket, bool enableLogging)
        - Replaced by DigitizerDataFetch.FetchVoltageData(
            TcpSocket spikeSafeSocket,
            bool? enableLogging = null,
            int? digitizerNumber = null)
    - DigitizerDataFetch.WaitForNewVoltageData(TcpSocket spikeSafeSocket, double waitTime)
        - Replaced by DigitizerDataFetch.WaitForNewVoltageData(
            TcpSocket spikeSafeSocket,
            double waitTime = 0.01,
            bool? enableLogging = null,
            double? timeout = null,
            int? digitizerNumber = null)
    - DigitizerDataFetch.WaitForNewVoltageData(TcpSocket spikeSafeSocket, double waitTime, bool enableLogging)
        - Replaced by DigitizerDataFetch.WaitForNewVoltageData(
            TcpSocket spikeSafeSocket,
            double waitTime = 0.01,
            bool? enableLogging = null,
            double? timeout = null,
            int? digitizerNumber = null)
