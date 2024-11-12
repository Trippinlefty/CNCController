using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CNCController.Core.Services.ErrorHandle;
using Microsoft.Extensions.Logging;

namespace CNCController.Core.Services.SerialCommunication
{
    public class SerialCommService : ISerialCommService
    {
        public Task<IEnumerable<string>> GetAvailablePortsAsync()
        {
            throw new NotImplementedException();
        }

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

        public IEnumerable<string> GetAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available ports");
                ErrorOccurred?.Invoke(this, "Failed to retrieve serial ports.");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken)
        {
            try
            {
                // Add actual connection logic here
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
                await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

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
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(this, message);
        }

        private void HandleCommandException(Exception ex, string message)
        {
            ErrorOccurred?.Invoke(this, message);
            _logger.LogError(ex, message);
        }
    }
}
