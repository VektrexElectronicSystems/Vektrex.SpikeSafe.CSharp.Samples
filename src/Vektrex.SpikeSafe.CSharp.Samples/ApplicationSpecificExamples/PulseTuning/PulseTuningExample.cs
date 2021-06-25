// Goal:
// Tune the pulse shape of a SpikeSafe PSMU or PRF by varying the compensation settings (Load Impedance and Rise Time)
//
// Expectation:
// A single 100µs pulse will be outputted sixteen separate times, demonstrating each combination of pulse tuning settings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.PulseTuning
{
    public class PulseTuningExample
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("PulseTuningExample.Run() started.");
                    
                // instantiate new TcpSocket to connect to SpikeSafe
                TcpSocket tcpSocket = new TcpSocket();
                tcpSocket.Connect(ipAddress, portNumber);

                // reset to default state and check for all events,
                // it is best practice to check for errors after sending each command      
                tcpSocket.SendScpiCommand("*RST");                  
                ReadAllEvents.LogAllEvents(tcpSocket);

                // set channel 1's pulse mode to Single Pulse
                tcpSocket.SendScpiCommand("SOUR1:FUNC:SHAP SINGLEPULSE");

                // set channel 1's current to 100 mA
                tcpSocket.SendScpiCommand("SOUR1:CURR 0.1");     

                // set channel 1's voltage to 20 V 
                tcpSocket.SendScpiCommand("SOUR1:VOLT 20");   

                // set channel 1's pulse width to 100µs. Of the pulse time settings, only Pulse On Time and Pulse Width [+Offset] are relevant in Single Pulse mode
                tcpSocket.SendScpiCommand("SOUR1:PULS:TON 0.0001");

                // set channel 1's output ramp to fast so that tests can be run in succession
                tcpSocket.SendScpiCommand("OUTP1:RAMP FAST");

                // Check for any errors with initializing commands
                ReadAllEvents.LogAllEvents(tcpSocket);

                // run each combination of Pulse Tuning settings to determine the settings that output the best pulse shape
                // per Vektrex recommendation, Load Impedance is tuned prior to Rise Time
                // once a pattern has been established, it may be useful to comment out ineffective or redundant test cases
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.VERY_LOW, RiseTime.VERY_SLOW);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.LOW, RiseTime.VERY_SLOW);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.MEDIUM, RiseTime.VERY_SLOW);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.HIGH, RiseTime.VERY_SLOW);    

                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.VERY_LOW, RiseTime.SLOW);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.LOW, RiseTime.SLOW);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.MEDIUM, RiseTime.SLOW);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.HIGH, RiseTime.SLOW);   

                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.VERY_LOW, RiseTime.MEDIUM);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.LOW, RiseTime.MEDIUM);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.MEDIUM, RiseTime.MEDIUM);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.HIGH, RiseTime.MEDIUM);    

                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.VERY_LOW, RiseTime.FAST);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.LOW, RiseTime.FAST);   
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.MEDIUM, RiseTime.FAST);    
                RunSinglePulseTuningTest(tcpSocket, LoadImpedance.HIGH, RiseTime.FAST);    

                // disconnect from SpikeSafe                      
                tcpSocket.Disconnect();    

                _log.Info("PulseTuningExample.Run() completed.\n");
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
         
        ////// defining the action to take per test session
        private void RunSinglePulseTuningTest(TcpSocket tcpSocket, LoadImpedance loadImpedance, RiseTime riseTime)
        {
            _log.Info("Running single pulse tuning test iteration with {0} and {1}", loadImpedance, riseTime);

            // set the load impedance and rise time according to the input parameters
            tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:CCOM {0}", (int)loadImpedance));
            tcpSocket.SendScpiCommand(string.Format("SOUR1:PULS:RCOM {0}", (int)riseTime)); 

            // Check for any errors with initializing commands
            ReadAllEvents.LogAllEvents(tcpSocket);

            // turn on all channels
            tcpSocket.SendScpiCommand("OUTP1 1");

            // Wait until channels are ready for a trigger command
            ReadAllEvents.ReadUntilEvent(tcpSocket, 100); // event 100 is "Channel Ready"

            // Output 1ms pulse for all channels
            tcpSocket.SendScpiCommand("OUTP1:TRIG");

            string is_pulse_complete = string.Empty;                
            while (is_pulse_complete != "TRUE")
            {                       
                tcpSocket.SendScpiCommand("SOUR1:PULS:END?");
                is_pulse_complete = tcpSocket.ReadData();
                ReadAllEvents.LogAllEvents(tcpSocket);
            }

            Console.WriteLine("Observe the current pulse shape using an oscilloscope or DMM, and note the current compensation settings.\n\nPress \"Enter\" to move to the next combination of Pulse Tuning settings.\n\nLoad Impedance: {0}\nRise Time: {1}", loadImpedance, riseTime);
            Console.ReadLine();

            tcpSocket.SendScpiCommand("OUTP1 0");

            // wait one second to account for any electrical transients before starting the next session
            Threading.Wait(1);

            _log.Info("Single pulse tuning test iteration completed successfully.");
        }

        ////// classes to express the compensation settings being tested
        private enum LoadImpedance
        {
            HIGH = 1,
            MEDIUM = 2,
            LOW = 3,
            VERY_LOW = 4
        }

        private enum RiseTime
        {
            FAST = 1,
            MEDIUM = 2,
            SLOW = 3,
            VERY_SLOW = 4
        }
    }
}