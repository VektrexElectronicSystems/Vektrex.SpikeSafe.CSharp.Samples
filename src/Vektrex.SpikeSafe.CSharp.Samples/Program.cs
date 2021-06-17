﻿using System;
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

namespace Vektrex.SpikeSafe.CSharp.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            string spikeSafeIpAddress = "10.0.0.220";
            int spikeSafePortNumber = 8282;

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

            // Uncomment line below to run MakingIntegratedVoltageMeasurements/MeasureAllPulsedVoltages
            //new MeasureAllPulsedVoltages().Run(spikeSafeIpAddress, spikeSafePortNumber);
        }
    }
}
