// Goal: 
// Use PSMU to supply current to Channel on into a resistor load in DC staircase pattern and measure voltage at each step
// Load the results of sample number, current, voltage, and calculated voltage to a table
// Graph the results of current (I), voltage (V), and calculated voltage (V) measurements. Voltage and calculated voltage can be compared

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
                _log.Info("MeasuringDcStaircaseVoltages.py started.");

                // stair case parameters
                int step_count = 10;
                double start_current_A = 0.010;
                double stop_current_A = 0.100;
                double step_size_A = (stop_current_A - start_current_A) / (step_count - 1);
                double load_ohm_value = 1;
                    
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
                tcpSocket.SendScpiCommand(string.Format("SOUR1:CURR {0}", start_current_A));
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
                tcpSocket.SendScpiCommand(string.Format("VOLT:TRIG:COUN {0}", step_count));
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
                double set_current = start_current_A;
                double currentIncrementDouble = 0.0;
                for(int step = 0; step < step_count; step++)
                {
                    // step up Channel 1 current to next step
                    set_current = Math.Round(set_current + currentIncrementDouble, 3);
                    string cmdStr = "SOUR1:TRIG " + set_current.ToString();
                    // send Set Current command for next step
                    tcpSocket.SendScpiCommand(cmdStr);
                    currentIncrementDouble = step_size_A;
                }
                    
                // check for all events
                ReadAllEvents.LogAllEvents(tcpSocket);
                
                // wait for the Digitizer measurements to complete. We need to wait for the data acquisition to complete before fetching the data
                DigitizerDataFetch.WaitForNewVoltageData(tcpSocket, 0.5);

                // Fetch Data and check for all events
                List<DigitizerData> digitizer_data = DigitizerDataFetch.FetchVoltageData(tcpSocket);
                ReadAllEvents.LogAllEvents(tcpSocket);

                // disconnect from PSMU    
                tcpSocket.Disconnect();  

                // put the fetched data in a plottable data format
                var plt = new ScottPlot.Plot();
                List<double> voltage_readings = new List<double>();
                List<double> current_steps = new List<double>();
                List<double> voltage_calculated_readings = new List<double>();
                List<double> voltage_data_log_array = new List<double>();
                _log.Info("Sample Number  |   Current     |       Vf   |    Vf Calculated");
                _log.Info("-------------  | ------------- | ---------- | ----------------");
                foreach (DigitizerData dd in digitizer_data)
                {
                    voltage_readings.Add(dd.VoltageReading);
                    current_steps.Add(start_current_A + step_size_A * (dd.SampleNumber - 1));
                    voltage_data_log_array.Add(dd.VoltageReading);
                    voltage_calculated_readings.Add((start_current_A + step_size_A * (dd.SampleNumber - 1))*load_ohm_value);
                    double current = start_current_A + step_size_A * (dd.SampleNumber - 1);
                    double voltage_calculated = (start_current_A + step_size_A * (dd.SampleNumber - 1))*load_ohm_value;
                    _log.Info("      %s      |      %.3f    |    %.10f   |   %.3f", dd.SampleNumber, current, dd.VoltageReading, voltage_calculated);
                }

                // plot the pulse shape using the fetched voltage readings and the light measurement readings overlaid
                // graph, voltage_axis = plt.subplots();

                // configure the voltage data
                var voltage_readings_line = plt.AddScatterLines(current_steps.ToArray(), voltage_readings.ToArray(), Color.Red, 1);
                voltage_readings_line.YAxisIndex = 0;
                plt.YAxis.Label("Voltage (V)", Color.Red);
                plt.YAxis.Color(voltage_readings_line.Color); 
                plt.XAxis.Label("Set Current (A)");               
                
                // configure the calculated voltage data
                var voltage_calculated_readings_line = plt.AddScatterLines(current_steps.ToArray(), voltage_calculated_readings.ToArray(), Color.Blue, 1);
                voltage_calculated_readings_line.YAxisIndex = 1;
                plt.YAxis2.Label("Calculated Voltage (V)", Color.Blue);
                plt.YAxis2.Color(voltage_calculated_readings_line.Color);
                plt.YAxis2.Ticks(true);

                plt.Title(string.Format("Sweep ({0}A to {1}A)", start_current_A, stop_current_A));
                plt.SaveFig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"dc_staircase_graph.png"));

                _log.Info("MeasuringDcStaircaseVoltages.py completed.\n");
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