# Application-Specific Examples

These sequences address specific scenarios in which SpikeSafe functionality is used in an integrated test system, or scenarios where more advanced SpikeSafe settings need to be tuned to meet specific criteria. These sequences are for users that are comfortable with the basic functionality of the SpikeSafe PRF or PSMU. See individual folders' descriptions for more information on each sequence.

## Directory
- [Controlling Thermal Platform Temperature](ControllingThermalPlatformTemperature)
- [Fixed Pulse Count Using Software Timing](FixedPulseCountUsingSoftwareTiming)
- [Making Tj Measurements](MakingTjMeasurements)
- [Measuring DC Staircase Voltages](MeasuringDcStaircaseVoltages)
- [Measuring Wavelength Spectrum](MeasuringWavelengthSpectrum)
- [Pulse Tuning](PulseTuning)
- [Running LIV Sweeps](RunningLivSweeps)
- [Using Digitizer Output Trigger](UsingDigitizerOutputTrigger)
- [Using Pulse Holds](UsingPulseHolds)

## Usage

Vektrex recommends getting acquainted with basic SpikeSafe features described in any of the other folders within [Vektrex.SpikeSafe.CSharp.Samples](/../../../) before running these sequences. If any additional explanation or insight is necessary, contact support@vektrex.com. 

Some of these sequences involve graphing measurement results, and require the [ScottPlot](https://swharden.com/scottplot/) library. See instructions on installing this library under the "Usage" section in the [Vektrex.SpikeSafe.CSharp.Samples markdown file](/../../../README.md#installing-scottplot).

For some of these sequences, some external instrumentation such as a spectrometer or a temperature-control device may be necessary. Read the individual sequence's description to see which settings and instrumentation need modification, and adjust settings based on your given test setup. For some of these sequences, some repetition is necessary as trial and error is the best method to achieve the desired outcome.

