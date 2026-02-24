using System;
using System.Reflection;
using System.Text;
namespace Vektrex.SpikeSafe.CSharp.Samples
{
    class Program
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            string spikeSafeIpAddress = "10.0.0.220";
            int spikeSafePortNumber = 8282;

            _log.Info("Vektrex.SpikeSafe.CSharp.Samples - Version {0}", Assembly.GetEntryAssembly().GetName().Version.ToString(3));

            // Uncomment line below to run GettingStarted/TcpSocketSample
            //new GettingStarted.TcpSocketSample.TcpSample().Run(spikeSafeIpAddress, spikeSafePortNumber);
            
            // Uncomment line below to run GettingStarted/ReadIdn
            //new GettingStarted.ReadIdn.ReadIdnExpanded().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run GettingStarted/ReadSpikeSafeInfo
            //new GettingStarted.ReadIdn.ReadSpikeSafeInfo().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run GettingStarted/ReadAllEvents
            //new GettingStarted.ReadAllEventsSample.ReadAllEventsSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run GettingStarted/ReadMemoryTableData
            //new GettingStarted.ReadMemoryTableData.ReadMemoryTableData().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run Gettingstarted/DischargeChannel
            //new GettingStarted.DischargeChannel.DischargeChannel().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunBias
            //new RunSpikeSafeOperatingModes.RunBias.RunBias().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunBiasPulsed/RunBiasPulsedDynamicMode
            //new RunSpikeSafeOperatingModes.RunBiasPulsed.RunBiasPulsedDynamicMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunBiasPulsed/RunBiasPulsedMode
            //new RunSpikeSafeOperatingModes.RunBiasPulsed.RunBiasPulsedMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunDc/RunDcDynamicMode
            //new RunSpikeSafeOperatingModes.RunDc.RunDcDynamicMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunDc/RunDcMode
            //new RunSpikeSafeOperatingModes.RunDc.RunDcMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunModulatedDc
            //new RunSpikeSafeOperatingModes.RunModulatedDc.RunModulatedMode().Run(spikeSafeIpAddress, spikeSafePortNumber);
            
            // Uncomment line below to run RunSpikeSafeOperatingModes/RunMultiPulse
            //new RunSpikeSafeOperatingModes.RunMultiPulse.RunMultiPulseMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsed/RunPulsedDynamicMode
            //new RunSpikeSafeOperatingModes.RunPulsed.RunPulsedDynamicMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsed/RunPulsedMode
            //new RunSpikeSafeOperatingModes.RunPulsed.RunPulsedMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsedSweep/RunBiasPulsedSweepMode
            //new RunSpikeSafeOperatingModes.RunPulsedSweep.RunBiasPulsedSweepMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsedSweep/RunPulsedSweepMode
            //new RunSpikeSafeOperatingModes.RunPulsedSweep.RunPulsedSweepMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunSinglePulse/RunBiasSinglePulseMode
            //new RunSpikeSafeOperatingModes.RunSinglePulse.RunBiasSinglePulseMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunSinglePulse/RunSinglePulseMode
            //new RunSpikeSafeOperatingModes.RunSinglePulse.RunSinglePulseMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunStaircaseSweep/RunStaircaseSweepMode.cs
            //new RunSpikeSafeOperatingModes.RunStaircaseSweep.RunStaircaseSweepMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasureAllPulsedVoltages
            //new MakingIntegratedVoltageMeasurements.MeasureAllPulsedVoltages.MeasureAllPulsedVoltages().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasurePulsedSweepVoltage
            //new MakingIntegratedVoltageMeasurements.MeasurePulsedSweepVoltage.MeasurePulsedSweepVoltage().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasureStaircaseSweepVoltage
            //new MakingIntegratedVoltageMeasurements.MeasureStaircaseSweepVoltage.MeasureStaircaseSweepVoltage().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasureVoltageAcrossPulse
            //new MakingIntegratedVoltageMeasurements.MeasureVoltageAcrossPulse.MeasureVoltageAcrossPulse().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run UsingForceSenseSelectorSwitch/ABForceSenseSwitching
            //new UsingForceSenseSelectorSwitch.ABForceSenseSwitching.ForceSenseSwitchSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run UsingForceSenseSelectorSwitch/ConnectDisconnectSwitching
            //new UsingForceSenseSelectorSwitch.ConnectDisconnectSwitching.ConnectDisconnectSwitchSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/ControllingThermalPlatformTemperature            
            //new ApplicationSpecificExamples.ControllingThermalPlatformTemperature.ControllingThermalPlatformTemperature().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/FixedPulseCountUsingSoftwareTiming            
            //new ApplicationSpecificExamples.FixedPulseCountUsingSoftwareTiming.FixedPulseCountUsingSoftwareTimingExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MakingTjMeasurements            
            //new ApplicationSpecificExamples.MakingTjMeasurements.TjMeasurement().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MakingTransientDualInterfaceMeasurement
            //new ApplicationSpecificExamples.MakingTransientDualInterfaceMeasurement.MakingTransientDualInterfaceMeasurementExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MeasuringDcStaircaseVoltages            
            //new ApplicationSpecificExamples.MeasuringDcStaircaseVoltages.MeasuringDcStaircaseVoltages().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MeasuringWavelengthSpectrum            
            //new ApplicationSpecificExamples.MeasuringWavelengthSpectrum.WavelengthSpectrumExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/PulseTuning            
            //new ApplicationSpecificExamples.PulseTuning.PulseTuningExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/RunningLivSweeps            
            //new ApplicationSpecificExamples.RunningLivSweeps.LIVSweepExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/PulseTuning            
            //new ApplicationSpecificExamples.UsingDigitizerOutputTrigger.DigitizerOutputTriggerSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/UsingPulseHolds            
            //new ApplicationSpecificExamples.UsingPulseHolds.UsingPulseHolds().Run(spikeSafeIpAddress, spikeSafePortNumber);
        }
    }
}
