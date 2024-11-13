using CNCController.Core.Exceptions;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;
using CNCController.Core.Services.ErrorHandle;

namespace CNCController.Core.Services.CNCControl
{
    public class CncController : ICncController
    {
        private readonly ILogger<CncController> _logger;
        private readonly IErrorHandler _errorHandler;
        private readonly ISerialCommService _serialCommService;
        private readonly IConfigurationService _configurationService;
        private CncStatus _currentStatus;

        public event EventHandler<CncStatus>? StatusUpdated;
        public event EventHandler<string>? ErrorOccurred;

        public ISerialCommService SerialCommService => _serialCommService;
        public IConfigurationService ConfigurationService => _configurationService;

        public CncController(ISerialCommService serialCommService, IConfigurationService configurationService, ILogger<CncController> logger, IErrorHandler errorHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _serialCommService = serialCommService ?? throw new ArgumentNullException(nameof(serialCommService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            _currentStatus = new CncStatus();
            _serialCommService.DataReceived += OnDataReceived;
        }

        public async Task JogAsync(string direction, double distance, CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync($"G91 G0 {direction}{distance} F100", CncState.Idle, "Jogging completed.", cancellationToken);
        }

        public async Task HomeAsync(CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync("G28", CncState.Idle, "Homing completed.", cancellationToken, CncState.Running, "Homing");
        }

        public async Task ChangeToolAsync(int toolNumber, CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync($"T{toolNumber} M6", CncState.Idle, $"Tool changed to T{toolNumber}", cancellationToken, CncState.Running, $"Changing tool to T{toolNumber}");
        }

        public async Task EmergencyStopAsync(CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync("M112", CncState.Idle, "Emergency Stop Activated", cancellationToken, CncState.Running, "Activating Emergency Stop");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync("M3 S1000", CncState.Idle, "CNC started.", cancellationToken, CncState.Running, "Starting CNC...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync("M5", CncState.Idle, "CNC stopped.", cancellationToken, CncState.Running, "Stopping CNC...");
        }

        public async Task PauseAsync(CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync("M0", CncState.Paused, "CNC Paused.", cancellationToken, CncState.Paused, "Pausing CNC...");
        }

        public CncStatus GetCurrentStatus() => _currentStatus;

        private async Task ExecuteCommandAsync(string command, CncState finalState, string finalMessage, CancellationToken cancellationToken, CncState? intermediateState = null, string? intermediateMessage = null)
        {
            if (intermediateState.HasValue && intermediateMessage != null)
                UpdateStatus(intermediateState.Value, intermediateMessage);

            try
            {
                var success = await _serialCommService.SendCommandAsync(command, cancellationToken);
                if (!success)
                    throw new InvalidOperationException($"Failed to execute command: {command}");
                
                UpdateStatus(finalState, finalMessage);
            }
            catch (Exception ex)
            {
                HandleError(ex, $"Error executing command: {command}");
            }
        }

        private void OnDataReceived(object? sender, string data)
        {
            if (data.Contains("ok"))
            {
                UpdateStatus(CncState.Idle, "Idle");
            }
            else if (data.Contains("ALARM"))
            {
                UpdateStatus(CncState.Error, "Alarm");
                ErrorOccurred?.Invoke(this, "CNC Alarm Detected");
            }
            else if (data.StartsWith("Position:"))
            {
                _currentStatus.Position = data.Replace("Position:", "").Trim();
                UpdateStatus(CncState.Running, "Position Updated");
            }
        }

        private void UpdateStatus(CncState newState, string message)
        {
            _currentStatus.State = newState;
            _currentStatus.StateMessage = message;
            StatusUpdated?.Invoke(this, _currentStatus);
        }

        private void HandleError(Exception ex, string message)
        {
            _errorHandler.HandleException(ex, message);
            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(this, message);
        }
    }
}
