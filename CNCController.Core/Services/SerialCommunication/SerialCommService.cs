using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<SerialCommService> _logger;
        private readonly object _lock = new();

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialCommService(ILogger<SerialCommService> logger)
        {
            _receiveBuffer = new StringBuilder();
            _logger = logger;
            _logger.LogInformation("SerialCommService initialized.");
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken)
        {
            const int maxRetryAttempts = 3;
            const int initialDelay = 500; // in milliseconds
            int retryAttempt = 0;

            while (retryAttempt < maxRetryAttempts)
            {
                try
                {
                    _logger.LogInformation("Attempting to connect to {PortName} at {BaudRate}. Retry {RetryAttempt}", portName, baudRate, retryAttempt + 1);
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
                    _logger.LogInformation("Successfully connected to {PortName}.", portName);

                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    ErrorOccurred?.Invoke(this, "Access to the port is denied. Please check permissions.");
                    _logger.LogError(ex, "Access error while connecting to port {PortName}", portName);
                    break;
                }
                catch (IOException ex)
                {
                    ErrorOccurred?.Invoke(this, "I/O error occurred. Check port connection or cable.");
                    _logger.LogError(ex, "I/O error while connecting to port {PortName}", portName);
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, $"Unexpected error: {ex.Message}");
                    _logger.LogError(ex, "Unexpected error while connecting to port {PortName}", portName);
                }

                await Task.Delay(initialDelay * (int)Math.Pow(2, retryAttempt), cancellationToken);
                retryAttempt++;
            }

            _logger.LogWarning("Connection failed after {RetryAttempt} attempts.", retryAttempt);
            return false;
        }

        public async Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken)
        {
            if (!IsConnected) return false;

            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(command + "\r\n");
                
                await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);

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
        
        public async Task DisconnectAsync()
        {
            if (_serialPort?.IsOpen ?? false)
            {
                try
                {
                    _logger.LogInformation("Attempting to disconnect from {PortName}.", _serialPort.PortName);
                    _serialPort.Close();
                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                    _logger.LogInformation("Successfully disconnected.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during disconnection.");
                    ErrorOccurred?.Invoke(this, "Failed to disconnect.");
                }
            }
        }
    }
}
