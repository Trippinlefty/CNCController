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
        private readonly StringBuilder _receiveBuffer;
        private readonly IErrorHandler _errorHandler;
        private readonly ILogger<SerialCommService> _logger;
        private readonly object _lock = new();

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialCommService(ILogger<SerialCommService> logger, IErrorHandler errorHandler)
        {
            _receiveBuffer = new StringBuilder();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _logger.LogInformation("SerialCommService initialized.");
        }

        public async Task<IEnumerable<string>> GetAvailablePortsAsync()
        {
            return await Task.Run(() => SerialPort.GetPortNames());  // Asynchronous port retrieval
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken)
        {
            try
            {
                _serialPort = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                _serialPort.DataReceived += OnDataReceived;
                _serialPort.Open();
                
                ConnectionOpened?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                HandleConnectionException(ex, "Failed to connect to the serial port.");
                return false;
            }
        }

        public async Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken)
        {
            if (!IsConnected) return false;

            try
            {
                var buffer = Encoding.ASCII.GetBytes(command + "\r\n");
                await _serialPort!.BaseStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                var response = await WaitForResponseAsync(cancellationToken).ConfigureAwait(false);
                return !string.IsNullOrEmpty(response);
            }
            catch (Exception ex)
            {
                HandleCommandException(ex, "Error sending command.");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _serialPort?.Close();
                ConnectionClosed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                HandleConnectionException(ex, "Failed to disconnect from the serial port.");
            }
        }

        public void Dispose()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= OnDataReceived;
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort.Dispose();
            }
            _logger.LogInformation("SerialCommService disposed.");
        }

        private async Task<string> WaitForResponseAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_receiveBuffer.Length > 0)
                    {
                        var response = _receiveBuffer.ToString();
                        _receiveBuffer.Clear();
                        return response;
                    }
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                HandleCommandException(ex, "Error waiting for response.");
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
                    var data = _serialPort.ReadExisting();
                    _receiveBuffer.Append(data);
                    DataReceived?.Invoke(this, data);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Read error: {ex.Message}");
            }
        }

        private void HandleConnectionException(Exception ex, string message)
        {
            _errorHandler.HandleException(ex, message);
            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(this, message);
        }

        private void HandleCommandException(Exception ex, string message)
        {
            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(this, message);
        }
    }
}
