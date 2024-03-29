﻿using System;
using System.Reflection;
using System.Text;
using Vektrex.SpikeSafe.CSharp.Samples.MakingIntegratedVoltageMeasurements.MeasureAllPulsedVoltages;
using Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadAllEventsSample;
using Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadIdn;
using Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.ReadMemoryTableData;
using Vektrex.SpikeSafe.CSharp.Samples.GettingStarted.TcpSocketSample;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunBias;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunBiasPulsed;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunDc;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunModulatedDc;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunMultiPulse;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunPulsed;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunPulsedSweep;
using Vektrex.SpikeSafe.CSharp.Samples.RunSpikeSafeOperatingModes.RunSinglePulse;
using Vektrex.SpikeSafe.CSharp.Samples.MakingIntegratedVoltageMeasurements.MeasurePulsedSweepVoltage;
using Vektrex.SpikeSafe.CSharp.Samples.MakingIntegratedVoltageMeasurements.MeasureVoltageAcrossPulse;
using Vektrex.SpikeSafe.CSharp.Samples.UsingForceSenseSelectorSwitch.ABForceSenseSwitching;
using Vektrex.SpikeSafe.CSharp.Samples.UsingForceSenseSelectorSwitch.ConnectDisconnectSwitching;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.FixedPulseCountUsingSoftwareTiming;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MakingTjMeasurements;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MakingTransientDualInterfaceMeasurement;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MeasuringDcStaircaseVoltages;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MeasuringWavelengthSpectrum;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.PulseTuning;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.RunningLivSweeps;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.UsingDigitizerOutputTrigger;
using Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.UsingPulseHolds;

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
            //new TcpSample().Run(spikeSafeIpAddress, spikeSafePortNumber);
            
            // Uncomment line below to run GettingStarted/ReadIdn
            //new ReadIdnExpanded().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run GettingStarted/ReadAllEvents
            //new ReadAllEventsSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run GettingStarted/ReadMemoryTableData
            //new ReadMemoryTableData().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunBias
            //new RunBias().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunBiasPulsed/RunBiasPulsedDynamicMode
            //new RunBiasPulsedDynamicMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunBiasPulsed/RunBiasPulsedMode
            //new RunBiasPulsedMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunDc/RunDcDynamicMode
            //new RunDcDynamicMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunDc/RunDcMode
            //new RunDcMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunModulatedDc
            //new RunModulatedMode().Run(spikeSafeIpAddress, spikeSafePortNumber);
            
            // Uncomment line below to run RunSpikeSafeOperatingModes/RunMultiPulse
            //new RunMultiPulseMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsed/RunPulsedDynamicMode
            //new RunPulsedDynamicMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsed/RunPulsedMode
            //new RunPulsedMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsedSweep/RunBiasPulsedSweepMode
            //new RunBiasPulsedSweepMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunPulsedSweep/RunPulsedSweepMode
            //new RunPulsedSweepMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunSinglePulse/RunBiasSinglePulseMode
            //new RunBiasSinglePulseMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run RunSpikeSafeOperatingModes/RunSinglePulse/RunSinglePulseMode
            //new RunSinglePulseMode().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasureAllPulsedVoltages
            //new MeasureAllPulsedVoltages().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasurePulsedSweepVoltage
            //new MeasurePulsedSweepVoltage().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasureVoltageAcrossPulse
            //new MeasureVoltageAcrossPulse().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run UsingForceSenseSelectorSwitch/ABForceSenseSwitching
            //new ForceSenseSwitchSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run UsingForceSenseSelectorSwitch/ConnectDisconnectSwitching
            //new ConnectDisconnectSwitchSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/ControllingThermalPlatformTemperature            
            //new ControllingThermalPlatformTemperature().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/FixedPulseCountUsingSoftwareTiming            
            //new FixedPulseCountUsingSoftwareTimingExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MakingTjMeasurements            
            //new TjMeasurement().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MakingTransientDualInterfaceMeasurement
            //new MakingTransientDualInterfaceMeasurementExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MeasuringDcStaircaseVoltages            
            //new MeasuringDcStaircaseVoltages().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/MeasuringWavelengthSpectrum            
            //new WavelengthSpectrumExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/PulseTuning            
            //new PulseTuningExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/RunningLivSweeps            
            //new LIVSweepExample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/PulseTuning            
            //new DigitizerOutputTriggerSample().Run(spikeSafeIpAddress, spikeSafePortNumber);

            // Uncomment line below to run ApplicationSpecificExamples/UsingPulseHolds            
            //new UsingPulseHolds().Run(spikeSafeIpAddress, spikeSafePortNumber);
        }
    }
}
