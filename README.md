# Vektrex.SpikeSafe.CSharp.Samples

Use these code samples to start learning how to communicate with your SpikeSafe via TCP/IP using C#. Sequences can be run with the following Vektrex products:
 - [SpikeSafe PSMU](https://www.vektrex.com/products/spikesafe-source-measure-unit/)
 - [SpikeSafe Performance Series ("PRF")](https://www.vektrex.com/products/spikesafe-performance-series-precision-pulsed-current-sources/)

## Directory

- [Getting Started](/src/Vektrex.SpikeSafe.CSharp.Samples/GettingStarted) - These samples are primarily intended for first-time users of Vektrex products. They contain steps to perform the basic tasks that are necessary to run the samples within the RunSpikeSafeOperatingModes folder.
- [Run SpikeSafe Operating Modes](/src/Vektrex.SpikeSafe.CSharp.Samples/RunSpikeSafeOperatingModes) - These folders contain examples to run specific SpikeSafe modes designed to test LEDs, Lasers, and electrical equipment. Basic settings will be sent to the SpikeSafe, and then one or more channels will be enabled to demonstrate the operation of each mode.
- [Making Integrated Voltage Measurements](/src/Vektrex.SpikeSafe.CSharp.Samples/MakingIntegratedVoltageMeasurements) - These folders contain examples to measure voltage using the SpikeSafe PSMU's integrated voltage Digitizer. The SpikeSafe outputs current to an LED, Laser, or electrical equipment, and then voltage measurements are read and displayed onscreen.
- [Using the Force Sense Selector Switch](/src/Vektrex.SpikeSafe.CSharp.Samples/UsingForceSenseSelectorSwitch) - These folders contain examples to operate the optional integrated switch within the SpikeSafe PSMU. The SpikeSafe outputs to an LED, Laser, or electrical equipment as in the previous examples, and the switch is used to either disconnect the SpikeSafe from the test circuit or to operate an auxiliary source to power the DUT.
- [Application-Specific Examples](/src/Vektrex.SpikeSafe.CSharp.Samples/ApplicationSpecificExamples) - These folders consist of more advanced samples to address specific test scenarios, as well as some demonstrations to fine-tune your SpikeSafe current output. These samples explain how to make light measurements using a SpikeSafe and a spectrometer, how to make in-situ junction temperature measurements on LEDs, and how to take full advantage of all SpikeSafe features.

## Usage

### IDE
To run these samples an IDE such as [Visual Studio Code](https://code.visualstudio.com/) is required, see [Working With C#](https://code.visualstudio.com/docs/languages/csharp) to simply setup your IDE with C#. After your IDE is setup, continue to install the remaining .NET packages below.

### Installing .NET Framework 4.8 Developer Pack
The [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48) need to be installed.

### Installing Vektrex.SpikeSafe.CSharp.Lib Package
The [Vektrex.SpikeSafe.CSharp.Lib](https://www.nuget.org/packages/Vektrex.SpikeSafe.CSharp.Lib/) package will need to be installed using the command `Install-Package Vektrex.SpikeSafe.CSharp.Lib`. Vektrex recommends always having the latest version of Vektrex.SpikeSafe.CSharp.Lib when running these sequences; the current version is 1.1.1.

Once the Vektrex.SpikeSafe.CSharp.Lib package is installed, each sample can be run independently by commenting out its respectively 'Run()' line in Program.cs. When a sample is run verify the expected outputs are obtained, as specified by the file's markdown description.

### Installing NLog
All samples will log messages and may log data to a log text file and require [NLog](https://www.nuget.org/packages/NLog/) (version 4.7.10 or greater). Use the NuGet command `Install-Package NLog` to install the latest version of Nlog.

### Installing ScottPlot
Some samples involve graphing measurement results. To properly graph results, the [ScottPlot](https://swharden.com/scottplot/) library is required (version 4.1.6 or greater). Use the NuGet command `Install-Package ScottPlot` to install the latest version of ScottPlot. Once the ScottPlot library is installed, each sample that involves graphing can be run from Program.cs.

### General Usage
For most examples, you may need to modify the specified IP address within a Program.cs to match the IP address that is physically set on your SpikeSafe's DIP switch. In each sample, the default IP address of 10.0.0.220 is set in the line `string spikeSafeIpAddress = "10.0.0.220";`.

Each file can be modified to include additional settings and commands to fit individual needs. Refer to SpikeSafe documentation for more information on the SpikeSafe API.

Most examples will log messages to the Vektrex.SpikeSafe.CSharp.Samples.log file under your local \bin\Debug\net48\ directory. Please refer to this file to ensure your sample is running correctly.

## Downloading Files

On this page, press "Clone or download" to download all files in this repository. We recommend saving this repository to your working directory for all other GitHub repositories.

## Where Do I Start?

First start with [TCP Socket Sample](/src/Vektrex.SpikeSafe.CSharp.Samples/GettingStarted/TcpSocketSample) to learn how setup a simple socket to communicate with your SpikeSafe. Then check out the rest of the samples under [Getting Started](GettingStarted).

## Built With

* [Visual Studio Code](https://code.visualstudio.com/)
* [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/VektrexElectronicSystems/Vektrex.SpikeSafe.CSharp.Samples/tags). 

## Support/Feedback

If any further assistance is needed beyond the information provided within this repository, email support@vektrex.com.

Feature requests and bug reports can be submitted to [the Vektrex website's support page](https://www.vektrex.com/request-support/). Select "Other" as the Product/System and enter "GitHub Repository" as the Subject.

## Contributors

* **Andy Fung** - [andyfung](https://github.com/andyfung)
* **Eljay Gemoto** - [eljayg](https://github.com/eljayg)

## License

Vektrex.SpikeSafe.CSharp.Samples is licensed under the MIT license, which allows for non-commercial and commercial use.