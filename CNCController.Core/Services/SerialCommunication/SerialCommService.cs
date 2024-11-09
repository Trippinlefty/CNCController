using System.IO.Ports;
using System.Text;
using CNCController.Core.Services.ErrorHandle;
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
        private readonly IErrorHandler _errorHandler;
        private readonly ILogger<SerialCommService> _logger;
        private readonly object _lock = new();

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialCommService(ILogger<SerialCommService> logger, IErrorHandler globalErrorHandler)
        {
            _receiveBuffer = new StringBuilder();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorHandler = globalErrorHandler ?? throw new ArgumentNullException(nameof(globalErrorHandler));
            _logger.LogInformation("SerialCommService initialized.");
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken)
        {
            try
            {
                // Code for establishing the connection
                return true;
            }
            catch (IOException ex)
            {
                _errorHandler.HandleException(ex);
                _logger.LogError(ex, "Failed to connect to the serial port.");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                _errorHandler.HandleException(ex);
                _logger.LogError(ex, "Access denied to the serial port.");
                return false;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(ex);
                _logger.LogError(ex, "Unexpected error occurred while connecting to the serial port.");
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
            try
            {
                // Code to disconnect
            }
            catch (IOException ex)
            {
                _errorHandler.HandleException(ex);
                _logger.LogError(ex, "Failed to disconnect from the serial port.");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(ex);
                _logger.LogError(ex, "Unexpected error occurred during disconnection.");
            }
        }
    }
}
