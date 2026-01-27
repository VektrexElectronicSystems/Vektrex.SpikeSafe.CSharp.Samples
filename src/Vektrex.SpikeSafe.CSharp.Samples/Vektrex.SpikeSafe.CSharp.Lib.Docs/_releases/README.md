# Vektrex.SpikeSafe.CSharp.Lib Releases

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