using System.IO.Ports;
using System.Text;

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

        public async Task<bool> ConnectAsync(string portName, int baudRate)
        {
            try
            {
                _serialPort = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 500, // Set read timeout to prevent blocking
                    WriteTimeout = 500, // Set write timeout
                    DtrEnable = true, // Enable Data Terminal Ready signal
                    RtsEnable = true // Enable Request to Send signal
                };

                _serialPort.DataReceived += OnDataReceived;
                _serialPort.Open();

                ConnectionOpened?.Invoke(this, EventArgs.Empty); // Fire connection opened event
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Connection error: {ex.Message}");
                return false;
            }
        }

        public Task DisconnectAsync()
        {
            if (_serialPort?.IsOpen ?? false)
            {
                _serialPort.Close();
                ConnectionClosed?.Invoke(this, EventArgs.Empty); // Fire connection closed event
            }

            return Task.CompletedTask;
        }

        public async Task<bool> SendCommandAsync(string command, int timeout = 2000)
        {
            if (!IsConnected) return false;

            try
            {
                lock (_lock)
                {
                    _serialPort?.WriteLine(command); // Send the command
                }

                // Wait for an acknowledgment or response
                var response = await WaitForResponseAsync(timeout);
                return !string.IsNullOrEmpty(response);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Send error: {ex.Message}");
                return false;
            }
        }

        private async Task<string> WaitForResponseAsync(int timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (_receiveBuffer.Length > 0)
                    {
                        var response = _receiveBuffer.ToString();
                        _receiveBuffer.Clear();
                        return response;
                    }

                    await Task.Delay(100, cts.Token); // Wait a bit before checking again
                }
            }
            catch (OperationCanceledException)
            {
                ErrorOccurred?.Invoke(this, "Response timeout");
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
                    string data = _serialPort.ReadExisting(); // Read all available data
                    _receiveBuffer.Append(data); // Buffer the incoming data
                    DataReceived?.Invoke(this, data); // Fire data received event
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Read error: {ex.Message}");
            }
        }
    }
}