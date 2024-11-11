using CNCController.Core.Exceptions;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;
using CNCController.Core.Services.ErrorHandle;

namespace CNCController.Core.Services.CNCControl;

public class CncController : ICncController
{
    private readonly ILogger<CncController> _logger;
    private readonly IErrorHandler _errorHandler;
    private readonly ISerialCommService _serialCommService;
    private readonly IConfigurationService _configurationService;
    private CncStatus _currentStatus;

    public event EventHandler<CncStatus>? StatusUpdated;
    public event EventHandler<string>? ErrorOccurred;

    public CncController(ISerialCommService serialCommService, IConfigurationService configurationService, ILogger<CncController> logger, IErrorHandler globalErrorHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorHandler = globalErrorHandler ?? throw new ArgumentNullException(nameof(globalErrorHandler));

        _serialCommService = serialCommService;
        _configurationService = configurationService;
        _currentStatus = new CncStatus();
        _configurationService.LoadConfigAsync();

        _serialCommService.DataReceived += OnDataReceived;
    }

    public async Task JogAsync(string direction, double distance, CancellationToken cancellationToken)
    {
        try
        {
            var jogCommand = $"G91 G0 {direction}{distance} F100";
            await SendCommandAsync(jogCommand, cancellationToken);
            UpdateStatus(CncState.Idle, "Jogging completed.");
        }
        catch (InvalidOperationException ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Jog command failed due to an invalid operation.");
            throw new CncOperationException("Jog command failed.", ex);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Unexpected error during jogging operation.");
            throw;
        }
    }

    public async Task HomeAsync(CancellationToken cancellationToken)
    {
        try
        {
            UpdateStatus(CncState.Running, "Homing");
            await SendCommandAsync("G28", cancellationToken);
            UpdateStatus(CncState.Idle, "Homing completed.");
        }
        catch (InvalidOperationException ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Home command failed due to an invalid operation.");
            throw new CncOperationException("Home command failed.", ex);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Unexpected error during homing operation.");
            throw;
        }
    }

    public async Task ChangeToolAsync(int toolNumber, CancellationToken cancellationToken)
    {
        try
        {
            UpdateStatus(CncState.Running, $"Changing tool to T{toolNumber}");
            var toolChangeCommand = $"T{toolNumber} M6";
            await SendCommandAsync(toolChangeCommand, cancellationToken);
            UpdateStatus(CncState.Idle, $"Tool changed to T{toolNumber}");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Error during tool change.");
            throw;
        }
    }

    public async Task EmergencyStopAsync(CancellationToken cancellationToken)
    {
        UpdateStatus(CncState.Running, "Activating Emergency Stop");
        var stopCommand = "M112";
        await SendCommandAsync(stopCommand, cancellationToken);
        UpdateStatus(CncState.Idle, "Emergency Stop Activated");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            UpdateStatus(CncState.Running, "Starting CNC...");
            await SendCommandAsync("M3 S1000", cancellationToken); // Example start command, replace with actual G-Code
            UpdateStatus(CncState.Idle, "CNC started.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Failed to start CNC.");
            throw new CncOperationException("Start operation failed.", ex);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            UpdateStatus(CncState.Running, "Stopping CNC...");
            await SendCommandAsync("M5", cancellationToken); // Example stop command, replace as needed
            UpdateStatus(CncState.Idle, "CNC stopped.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Failed to stop CNC.");
            throw new CncOperationException("Stop operation failed.", ex);
        }
    }

    public async Task PauseAsync(CancellationToken cancellationToken)
    {
        try
        {
            UpdateStatus(CncState.Paused, "Pausing CNC...");
            await SendCommandAsync("M0", cancellationToken); // Example pause command
            UpdateStatus(CncState.Paused, "CNC Paused.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Failed to pause CNC.");
            throw new CncOperationException("Pause operation failed.", ex);
        }
    }

    public CncStatus GetCurrentStatus()
    {
        return _currentStatus;
    }

    private async Task SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _serialCommService.SendCommandAsync(command, cancellationToken);
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
}