# Getting Started

These samples are primarily intended for first-time users of Vektrex products. They contain steps to perform the basic tasks that are necessary to run the samples within Vektrex.SpikeSafe.CSharp.Samples/[Run SpikeSafe Operating Modes](../RunSpikeSafeOperatingModes).

## Directory
For first-time users, Vektrex recommends running the samples in the order shown below:

1. [TCP Sample](TcpSocketSample) - a more in depth example that connects to the SpikeSafe using a TCP socket. An *IDN? query is sent with more verbose C# commands
2. [Read *IDN?](ReadIdn) - Uses the SCPI Standard "*IDN?" query and following information queries to obtain the model of your SpikeSafe
3. [Read All Events](ReadAllEvents) - Reads all events from the SpikeSafe event queue 
4. [Read Memory Table Data](ReadMemoryTableData) - Reads the SpikeSafe status and obtains current operational information from the SpikeSafe

## Usage
To run these samples, an IDE such as [Visual Studio Code](https://code.visualstudio.com/) is required. The [Vektrex.SpikeSafe.CSharp.Lib](https://www.nuget.org/packages/Vektrex.SpikeSafe.CSharp.Lib/) package will need to be installed using the command `Install-Package Vektrex.SpikeSafe.CSharp.Lib`. Vektrex recommends always having the latest version of Vektrex.SpikeSafe.CSharp.Lib when running these sequences; the current version is 1.1.1.

Simply change the line `string spikeSafeIpAddress = "10.0.0.220";` in Program.cs to match the IP address of you connected SpikeSafe and run the sequence (generally by pressing F5). Observe the outputs that appear in both the terminal window and Vektrex.SpikeSafe.CSharp.Samples.log, which will output wherever you have saved the local Vektrex.SpikeSafe.CSharp.Samples\bin\Debug\net48\ directory.