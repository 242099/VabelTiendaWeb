using System.Diagnostics;
using System.IO.Ports;
using System.Windows;

namespace VabelMitienditaEsc.Services
{
    public class ArduinoService
    {
        private readonly SerialPort _serialPort;

        public event Action<string> DataReceived;

        public ArduinoService(string portName, int baudRate = 9600)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Connect()
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    MessageBox.Show("Arduino conectado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                MessageBox.Show("Arduino desconectado.");
            }
        }

        public void SendData(string data)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.WriteLine(data);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadLine();
                DataReceived?.Invoke(data.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer datos: {ex.Message}");
            }
        }
    }
}