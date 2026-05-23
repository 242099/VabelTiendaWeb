using System.Diagnostics;
using System.IO.Ports;

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
                    Debug.WriteLine("Arduino conectado.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al conectar: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                Debug.WriteLine("Arduino desconectado.");
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
                Debug.WriteLine($"Error al leer datos: {ex.Message}");
            }
        }
    }
}