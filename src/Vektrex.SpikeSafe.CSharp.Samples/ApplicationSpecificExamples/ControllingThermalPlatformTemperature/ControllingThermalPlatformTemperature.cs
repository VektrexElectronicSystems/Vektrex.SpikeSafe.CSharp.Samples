// Goal: 
// Use TEC to ramp up to 50°C, stabilize, and run for 10 minutes; then ramp down to 25°C, stabilize, and run for 10 minutes.

using System;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.ControllingThermalPlatformTemperature
{
    public class ControllingThermalPlatformTemperature
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(string ipAddress, int portNumber)
        {
            // start of main program
            try
            {
                _log.Info("ControllingThermalPlatformTemperature.Run() started.");;

                double setTemperatureOne = 50;
                double setTemperatureOneStabilityMinutes = 10;
                double setTemperatureTwo = 25;
                double setTemperatureTwoStabilityMinutes = 10;

                SerialInterfaceDll tecController = new SerialInterfaceDll();

                // Set the beep enable sound
                tecController.WriteCommand("BEEP 1");

                // Set the mount type to 284 TEC High Power LaserMount.
                // Will default sensor type, sensor coefficients, gain, fan mode, and current limit
                tecController.WriteCommand("TEC:MOUNT 284");

                // Set temperature control mode
                tecController.WriteCommand("TEC:MODE:T");

                // Set the heat/cool mode to both
                tecController.WriteCommand("TEC:HEATCOOL BOTH");

                // Set the low set temperature limit to 10°C
                tecController.WriteCommand("TEC:LIMit:TLO 10");

                // Set the high set temperature limit to 85°C
                tecController.WriteCommand("TEC:LIMit:THI 85");

                // Set platform tolerance to be within 0.5°C of set point for 30 seconds
                tecController.WriteCommand("TEC:TOLerance 0.5, 30");

                ////////// Target temperature 1

                // Set set temperature to setTemperatureOne
                tecController.WriteCommand(string.Format("TEC:T {0}", setTemperatureOne));

                // Set controller output to on
                tecController.WriteCommand("TEC:OUT 1");

                // Monitor until TEC is in tolerance
                while (true)
                {
                    tecController.WriteCommand("TEC:COND?");
                    byte tecOutOfTolerance = Byte.Parse(tecController.ReadData());
                    if (isKthBitSet(tecOutOfTolerance, 9) == true)
                        break;
                }

                // Let TEC temperature stabilize while in tolerance
                DateTime tecStabilityStartTime = DateTime.Now;
                while (true)
                {
                    tecController.WriteCommand("TEC:COND?");
                    byte tecOutOfTolerance = Byte.Parse(tecController.ReadData());
                    if ((isKthBitSet(tecOutOfTolerance, 9) == true) && 
                        ((DateTime.Now - tecStabilityStartTime - TimeSpan.FromSeconds(30)).TotalSeconds >= setTemperatureOneStabilityMinutes * 60))
                            break;
                    else
                        break;
                }

                // Set set temperature to setTemperatureTwo
                tecController.WriteCommand(string.Format("TEC:T {0}", setTemperatureTwo));

                ////////// Target temperature 2

                // Monitor until TEC is in tolerance
                while (true)
                {
                    tecController.WriteCommand("TEC:COND?");
                    byte tecOutOfTolerance = Byte.Parse(tecController.ReadData());
                    if (isKthBitSet(tecOutOfTolerance, 9) == true)
                        break;
                }

                // Let TEC temperature stabilize while in tolerance
                tecStabilityStartTime = DateTime.Now;
                while (true)
                {
                    tecController.WriteCommand("TEC:COND?");
                    byte tecOutOfTolerance = Byte.Parse(tecController.ReadData());
                    if ((isKthBitSet(tecOutOfTolerance, 9) == true) && 
                        ((DateTime.Now - tecStabilityStartTime - TimeSpan.FromSeconds(30)).TotalSeconds >= setTemperatureTwoStabilityMinutes * 60))
                            break;
                    else
                        break;
                }

                ////////// Disconnect
                tecController.Disconnect();

                _log.Info("ControllingThermalPlatformTemperature.Run() completed.\n");
            }
            catch(Exception e)
            {
                // print any general exception to both the terminal and the log file, then exit the application
                string errorMessage = string.Format("Program error: {0}\n", e.Message);
                _log.Error(errorMessage);
                Console.WriteLine(errorMessage);
            }
        }

        private bool isKthBitSet(byte n, int k)
        { 
            if ((n & (1 << (k - 1))) != 0) 
                return true;
            else
                return false; 
        }
    }
}