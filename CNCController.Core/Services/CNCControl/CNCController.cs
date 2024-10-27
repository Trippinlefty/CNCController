using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;

namespace CNCController.Core.Services.CNCControl
{
    public class CNCController : ICNCController
    {
        private readonly ISerialCommService _serialCommService;
        private readonly IConfigurationService _configurationService;
        private CNCStatus _currentStatus;

        public event EventHandler<CNCStatus>? StatusUpdated;
        public event EventHandler<string>? ErrorOccurred;

        public CNCController(ISerialCommService serialCommService, IConfigurationService configurationService)
        {
            _serialCommService = serialCommService;
            _configurationService = configurationService;
            _currentStatus = new CNCStatus();
            _configurationService.LoadConfigAsync();

            // Subscribe to serial data received for status updates
            _serialCommService.DataReceived += OnDataReceived;
        }

        public async Task JogAsync(string direction, double distance)
        {
            string jogCommand = $"G91 G0 {direction}{distance} F100"; // Incremental jog with feed rate
            await SendCommandAsync(jogCommand);
            UpdateStatus($"Jogging {direction}{distance}");
        }

        public async Task HomeAsync()
        {
            string homeCommand = "G28"; // Home command
            await SendCommandAsync(homeCommand);
            UpdateStatus("Homing");
        }

        public async Task ChangeToolAsync(int toolNumber)
        {
            string toolChangeCommand = $"T{toolNumber} M6"; // Tool change command
            await SendCommandAsync(toolChangeCommand);
            UpdateStatus($"Changing tool to T{toolNumber}");
        }

        public async Task EmergencyStopAsync()
        {
            string stopCommand = "M112"; // Emergency stop (GRBL specific)
            await SendCommandAsync(stopCommand);
            UpdateStatus("Emergency Stop Activated");
        }

        public CNCStatus GetCurrentStatus()
        {
            return _currentStatus;
        }

        private async Task SendCommandAsync(string command)
        {
            try
            {
                bool success = await _serialCommService.SendCommandAsync(command);
                if (!success)
                {
                    ErrorOccurred?.Invoke(this, "Failed to send command: " + command);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error in command execution: {ex.Message}");
            }
        }

        private void OnDataReceived(object? sender, string data)
        {
            // Basic handling of CNC status data
            if (data.Contains("ok"))
            {
                _currentStatus.IsRunning = false;
                UpdateStatus("Idle");
            }
            else if (data.Contains("ALARM"))
            {
                _currentStatus.StateMessage = "Alarm";
                ErrorOccurred?.Invoke(this, "CNC Alarm Detected");
            }
            else if (data.StartsWith("Position:"))
            {
                _currentStatus.Position = data.Replace("Position:", "").Trim();
                UpdateStatus("Position Updated");
            }
        }

        private void UpdateStatus(string message)
        {
            _currentStatus.StateMessage = message;
            StatusUpdated?.Invoke(this, _currentStatus);
        }
    }
}