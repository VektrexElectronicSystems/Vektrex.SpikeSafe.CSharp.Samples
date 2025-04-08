// Goal: 
// Use PSMU to supply current to Channel on into a resistor load in DC staircase pattern and measure voltage at each step
// Load the results of sample number, current, voltage, and calculated voltage to a table
// Graph the results of current (I), voltage (V), and calculated voltage (V) measurements. Voltage and calculated voltage can be compared

using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.MeasuringDcStaircaseVoltages
{
    public class MeasuringDcStaircaseVoltages
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("MeasuringDcStaircaseVoltages.Run() started.");

                // stair case parameters
                int stepCount = 10;
                double startCurrentAmps = 0.010;
                double stopCurrentAmps = 0.100;
                double stepSizeAmps = (stopCurrentAmps - startCurrentAmps) / (stepCount - 1);
                double loadOhmValue = 1;
                    
                // instantiate new TcpSocket to connect to PSMU
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's mode to DC Dynamic mode and check for all events
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP DCDYNAMIC");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's voltage to 10 and check for all events
                tcpSocket.SendScpiCommand("SOUR1:VOLT 10");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Auto Range to On and check for all events
                tcpSocket.SendScpiCommand("SOUR1:CURR:RANG:AUTO 1");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's current to start current and check for all events
                tcpSocket.SendScpiCommand(string.Format("SOUR1:CURR {0}", startCurrentAmps));
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Channel 1's Ramp mode to Fast and check for all events
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // start the Channel 1
                tcpSocket.SendScpiCommand("OUTP1 ON");

                // wait until Channel 1 is ready
                ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"
                
                // set Digitizer to abort any measurements
                tcpSocket.SendScpiCommand("VOLT:ABOR");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Digitizer Aperture to 10us and check for all events
                tcpSocket.SendScpiCommand("VOLT:APER 10");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Digitizer Trigger Count to step count and check for all events
                tcpSocket.SendScpiCommand(string.Format("VOLT:TRIG:COUN {0}", stepCount));
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Digitizer Read Count to 1 and check for all events
                tcpSocket.SendScpiCommand("VOLT:READ:COUN 1");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Digitizer Range to 10V and check for all events
                tcpSocket.SendScpiCommand("VOLT:RANG 10");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set Digitizer SW Trigger and check for all events
                tcpSocket.SendScpiCommand("VOLT:TRIG:SOUR SOFTWARE");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // start Digitizer software triggered measurements
                tcpSocket.SendScpiCommand("VOLT:INIT:SENDMSG");
                ReadAllEvents.LogAllEvents(tcpSocket);

                // start DC staircase current supply and voltage measurement per step
                double setCurrent = startCurrentAmps;
                double currentIncrementDouble = 0.0;
                for(int step = 0; step < stepCount; step++)
                {
                    // step up Channel 1 current to next step
                    setCurrent = Math.Round(setCurrent + currentIncrementDouble, 3);
                    string cmdStr = "SOUR1:TRIG " + setCurrent.ToString();
                    // send Set Current command for next step
                    tcpSocket.SendScpiCommand(cmdStr);
                    currentIncrementDouble = stepSizeAmps;
                }
                    
                // check for all events
                ReadAllEvents.LogAllEvents(tcpSocket);
                
                // wait for the Digitizer measurements to complete. We need to wait for the data acquisition to complete before fetching the data
                DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);

                // Fetch Data and check for all events
                List<DigitizerData> digitizerData = DigitizerDataFetch.FetchVoltageData(tcpSocket);
                ReadAllEvents.LogAllEvents(tcpSocket);

                // disconnect from PSMU    
                tcpSocket.Disconnect();  

                // put the fetched data in a plottable data format
                var plt = new ScottPlot.Plot();
                List<double> voltageReadings = new List<double>();
                List<double> currentSteps = new List<double>();
                List<double> voltageCalculatedReadings = new List<double>();
                _log.Info("Sample Number  |   Current     |       Vf   |    Vf Calculated");
                _log.Info("-------------  | ------------- | ---------- | ----------------");
                foreach (DigitizerData dd in digitizerData)
                {
                    voltageReadings.Add(dd.VoltageReading);
                    currentSteps.Add(startCurrentAmps + stepSizeAmps * (dd.SampleNumber - 1));
                    voltageCalculatedReadings.Add((startCurrentAmps + stepSizeAmps * (dd.SampleNumber - 1))*loadOhmValue);
                    double current = startCurrentAmps + stepSizeAmps * (dd.SampleNumber - 1);
                    double voltageCalculated = (startCurrentAmps + stepSizeAmps * (dd.SampleNumber - 1))*loadOhmValue;
                    _log.Info("      {0}      |      {1}    |    {2}   |   {3}", dd.SampleNumber, current.ToString("0.000"), dd.VoltageReading.ToString("0.0000000000"), String.Format("{0:0.000}", voltageCalculated));
                }

                // configure the voltage data
                var voltageReadingsLine = plt.Add.ScatterLine(currentSteps.ToArray(), voltageReadings.ToArray());
                voltageReadingsLine.Color = Colors.Red;
                voltageReadingsLine.LineWidth = 1;
                voltageReadingsLine.Axes.YAxis = plt.Axes.Left;
                plt.XLabel("Set Current (mA)");
                plt.Axes.Left.Label.Text = "Voltage (V)";
                plt.Axes.Left.Label.ForeColor = Colors.Red;

                // configure the calculated voltage data
                var voltageCalculatedReadingsLine = plt.Add.ScatterLine(currentSteps.ToArray(), voltageCalculatedReadings.ToArray());
                voltageCalculatedReadingsLine.Color = Colors.Blue;
                voltageCalculatedReadingsLine.LineWidth = 1;
                voltageCalculatedReadingsLine.Axes.YAxis = plt.Axes.Right;
                plt.Axes.Right.Label.Text = "Calculated Voltage (V)";
                plt.Axes.Right.Label.ForeColor = Colors.Blue;
                plt.Axes.Right.IsVisible = true; // ensure right Y axis is shown

                plt.Title(string.Format("Sweep ({0}A to {1}A)", startCurrentAmps, stopCurrentAmps));
                plt.SavePng(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dc_staircase_graph.png"), 800, 600);

                _log.Info("MeasuringDcStaircaseVoltages.Run() completed.\n");
            }
            catch(SpikeSafeException e)
            {
                // print any SpikeSafe-specific error to both the terminal and the log file, then exit the application
                string errorMessage = string.Format("SpikeSafe error: {0}\n", e.Message);
                _log.Error(errorMessage);
                Console.WriteLine(errorMessage);
            }
            catch(Exception e)
            {
                // print any general exception to both the terminal and the log file, then exit the application
                string errorMessage = string.Format("Program error: {0}\n", e.Message);
                _log.Error(errorMessage);
                Console.WriteLine(errorMessage);
            }
        }
    }
}