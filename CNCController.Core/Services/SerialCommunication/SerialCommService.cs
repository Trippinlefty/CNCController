using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CNCController.Core.Services.SerialCommunication
{
    public class SerialCommService : ISerialCommService
    {
        public event EventHandler<string>? DataReceived;
        public event EventHandler? ConnectionOpened;
        public event EventHandler? ConnectionClosed;
        public event EventHandler<string>? ErrorOccurred;

        private SerialPort? _serialPort;
        private StringBuilder _receiveBuffer;
        private readonly object _lock = new(); // To synchronize access to the port

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialCommService()
        {
            _receiveBuffer = new StringBuilder();
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken)
        {
            try
            {
                _serialPort = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500,
                    DtrEnable = true,
                    RtsEnable = true
                };

                _serialPort.DataReceived += OnDataReceived;
                _serialPort.Open();

                ConnectionOpened?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Connection error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken)
        {
            if (!IsConnected) return false;

            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(command + "\r\n");
                
                await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);

                // Wait for acknowledgment or response
                var response = await WaitForResponseAsync(cancellationToken);
                return !string.IsNullOrEmpty(response);
            }
            catch (OperationCanceledException)
            {
                ErrorOccurred?.Invoke(this, "Command sending canceled.");
                return false;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Send error: {ex.Message}");
                return false;
            }
        }

        private async Task<string> WaitForResponseAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_receiveBuffer.Length > 0)
                    {
                        string response = _receiveBuffer.ToString();
                        _receiveBuffer.Clear();
                        return response;
                    }
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                ErrorOccurred?.Invoke(this, "Response waiting canceled.");
            }
            return string.Empty;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;

                lock (_lock)
                {
                    string data = _serialPort.ReadExisting();
                    _receiveBuffer.Append(data);
                    DataReceived?.Invoke(this, data);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Read error: {ex.Message}");
            }
        }
        
        public Task DisconnectAsync()
        {
            if (_serialPort?.IsOpen ?? false)
            {
                _serialPort.Close();
                ConnectionClosed?.Invoke(this, EventArgs.Empty);
            }
            return Task.CompletedTask;
        }
    }
}
