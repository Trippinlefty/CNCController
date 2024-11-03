using CNCController.Core.Exceptions;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using CNCController.Core.Services.ErrorHandle;

namespace CNCController.Core.Services.CNCControl
{
    public class CNCController : ICNCController
    {
        private readonly ILogger<CNCController> _logger;
        private readonly IErrorHandler _globalErrorHandler;
        private readonly ISerialCommService _serialCommService;
        private readonly IConfigurationService _configurationService;
        private CNCStatus _currentStatus;

        public event EventHandler<CNCStatus>? StatusUpdated;
        public event EventHandler<string>? ErrorOccurred;

        public CNCController(ISerialCommService serialCommService, IConfigurationService configurationService, ILogger<CNCController> logger, IErrorHandler globalErrorHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _globalErrorHandler = globalErrorHandler ?? throw new ArgumentNullException(nameof(globalErrorHandler));

            _serialCommService = serialCommService;
            _configurationService = configurationService;
            _currentStatus = new CNCStatus();
            _configurationService.LoadConfigAsync();

            _serialCommService.DataReceived += OnDataReceived;
        }

        public async Task JogAsync(string direction, double distance, CancellationToken cancellationToken)
        {
            try
            {
                string jogCommand = $"G91 G0 {direction}{distance} F100";
                await SendCommandAsync(jogCommand, cancellationToken);
                UpdateStatus(CNCState.Idle, "Jogging completed.");
            }
            catch (InvalidOperationException ex)
            {
                _globalErrorHandler.HandleException(ex);
                _logger.LogError(ex, "Jog command failed due to an invalid operation.");
                throw new CNCOperationException("Jog command failed.", ex);
            }
            catch (Exception ex)
            {
                _globalErrorHandler.HandleException(ex);
                _logger.LogError(ex, "Unexpected error during jogging operation.");
                throw;
            }
        }

        public async Task HomeAsync(CancellationToken cancellationToken)
        {
            try
            {
                UpdateStatus(CNCState.Running, "Homing");
                await SendCommandAsync("G28", cancellationToken);
                UpdateStatus(CNCState.Idle, "Homing completed.");
            }
            catch (InvalidOperationException ex)
            {
                _globalErrorHandler.HandleException(ex);
                _logger.LogError(ex, "Home command failed due to an invalid operation.");
                throw new CNCOperationException("Home command failed.", ex);
            }
            catch (Exception ex)
            {
                _globalErrorHandler.HandleException(ex);
                _logger.LogError(ex, "Unexpected error during homing operation.");
                throw;
            }
        }

        public async Task ChangeToolAsync(int toolNumber, CancellationToken cancellationToken)
        {
            try
            {
                UpdateStatus(CNCState.Running, $"Changing tool to T{toolNumber}");
                string toolChangeCommand = $"T{toolNumber} M6";
                await SendCommandAsync(toolChangeCommand, cancellationToken);
                UpdateStatus(CNCState.Idle, $"Tool changed to T{toolNumber}");
            }
            catch (Exception ex)
            {
                _globalErrorHandler.HandleException(ex);
                _logger.LogError(ex, "Error during tool change.");
                throw;
            }
        }

        public async Task EmergencyStopAsync(CancellationToken cancellationToken)
        {
            UpdateStatus(CNCState.Running, "Activating Emergency Stop");
            string stopCommand = "M112";
            await SendCommandAsync(stopCommand, cancellationToken);
            UpdateStatus(CNCState.Idle, "Emergency Stop Activated");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task PauseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public CNCStatus GetCurrentStatus()
        {
            return _currentStatus;
        }

        private async Task SendCommandAsync(string command, CancellationToken cancellationToken)
        {
            try
            {
                bool success = await _serialCommService.SendCommandAsync(command, cancellationToken);
                if (!success)
                {
                    ErrorOccurred?.Invoke(this, "Failed to send command: " + command);
                }
            }
            catch (OperationCanceledException)
            {
                ErrorOccurred?.Invoke(this, $"Command '{command}' was canceled.");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error in command execution: {ex.Message}");
                throw;
            }
        }

        private void OnDataReceived(object? sender, string data)
        {
            // Basic handling of CNC status data
            if (data.Contains("ok"))
            {
                UpdateStatus(CNCState.Idle, "Idle");
            }
            else if (data.Contains("ALARM"))
            {
                UpdateStatus(CNCState.Error, "Alarm");
                ErrorOccurred?.Invoke(this, "CNC Alarm Detected");
            }
            else if (data.StartsWith("Position:"))
            {
                _currentStatus.Position = data.Replace("Position:", "").Trim();
                UpdateStatus(CNCState.Running, "Position Updated");
            }
        }

        private void UpdateStatus(CNCState state, string message)
        {
            _currentStatus.State = state;
            _currentStatus.StateMessage = message;
            StatusUpdated?.Invoke(this, _currentStatus);
        }
    }
}
