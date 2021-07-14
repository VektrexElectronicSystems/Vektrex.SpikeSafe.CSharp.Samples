# Making Integrated Voltage Measurements

These folders contain examples to make voltage measurements using the SpikeSafe PSMU's integrated Digitizer. Relevant settings will be sent to both the SpikeSafe and the Digitizer, and then voltage will be measured across the DUT and displayed onscreen. Each folder contains information on the relevant settings and expected output from the given mode(s). These examples are only applicable to the SpikeSafe PSMU and assume some basic knowledge of [SpikeSafe Operating Modes](../RunSpikeSafeOperatingModes).

## Directory
- [Measure All Pulsed Voltages](MeasureAllPulsedVoltages)
- [Measure Pulsed Sweep Voltage](MeasurePulsedSweepVoltage)
- [Measure Voltage Across Pulse](MeasureVoltageAcrossPulse)

## Usage

These sequences involve graphing measurement results, and require the [ScottPlot](https://swharden.com/scottplot/) library. See instructions on installing this library under the "Usage" section in the [Vektrex.SpikeSafe.CSharp.Samples markdown file](/README.md#installing-scottplot).

Connect the SpikeSafe PSMU's Force and Sense leads to the LED, Laser, or electrical equipment to be tested. Using the descriptions and screenshots provided, determine which mode fits your test scenario. Run the sequences provided and observe the outputted voltage measurement data onscreen. The code provided sends all the necessary commands to output current and measure voltage for a given test purpose, but sequences may be modified as necessary to fit your specific application.
