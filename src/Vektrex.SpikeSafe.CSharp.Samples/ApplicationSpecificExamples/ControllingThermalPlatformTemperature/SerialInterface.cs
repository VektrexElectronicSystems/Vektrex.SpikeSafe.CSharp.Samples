// Goal: 
// Connect to a SpikeSafe and run DC Dynamic mode on Channel 1 into an LED
// Measure the emitted light using a Spectrometer
//
// Expectation: 
// Channel 1 will output 100mA DC current. Expecting a low (<1V) forward voltage
// Using external CAS DLL, control the spectrometer to make light measurements and then graph the wavelength spectrum

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using NLog;
using Vektrex.SpikeSafe.CSharp.Lib;

namespace Vektrex.SpikeSafe.CSharp.Samples.ApplicationSpecificExamples.ControllingThermalPlatformTemperature
{
    public class SerialInterfaceDll
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        private SerialPort _serialPort = null;

        private double _commandDelay = 0;

        private string _port = "Not set";
        public string Port
        {
            get { return _port; }
        }

        public void Connect(string port, int baudRate = 38400, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int dataBits = 8, int timeOut = 0, int writeTimeout = 0, double commandDelay = 0.1)
        {
            try
            {
                _commandDelay = commandDelay;
                _port = port;

                _serialPort = new SerialPort();
                _serialPort.PortName = port;
                _serialPort.BaudRate = baudRate;
                _serialPort.Parity = parity;
                _serialPort.StopBits = stopBits;
                _serialPort.DataBits = dataBits;
                _serialPort.ReadTimeout = timeOut;
                _serialPort.WriteTimeout = writeTimeout;

                _serialPort.Open();
            }
            catch (Exception exception)
            {
                _log.Error("Error connecting to Serial Port at {0}, {1}", Port, exception.Message);
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_serialPort != null)
                {
                    _serialPort.Close();
                    Threading.Wait(_commandDelay);
                }
            }
            catch (Exception exception)
            {
                _log.Error("Error disconnecting from Serial Port at {0}, {1}", Port, exception.Message);
                throw;
            }
        }

        public void WriteCommand(string command)
        {
            try
            {
                _serialPort.WriteLine(command);
            }
            catch (Exception exception)
            {
                _log.Error("Error writing to Serial Port at {0}, {1}", Port, exception.Message);
                throw;
            }
        }
        
        public string ReadData()
        {
            try
            {
                string data = _serialPort.ReadLine();
                return data;
            }
            catch (Exception exception)
            {
                _log.Error("Error reading from Serial Port at {0}, {1}", Port, exception.Message);
                throw;
            }
        }
    }
}