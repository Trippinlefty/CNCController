﻿using System;
using System.Threading;
using System.Threading.Tasks;
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

            _serialCommService.DataReceived += OnDataReceived;
        }

        public async Task JogAsync(string direction, double distance, CancellationToken cancellationToken)
        {
            string jogCommand = $"G91 G0 {direction}{distance} F100";
            await SendCommandAsync(jogCommand, cancellationToken);
            UpdateStatus(CNCState.Idle, "Jogging completed.");
        }

        public async Task HomeAsync(CancellationToken cancellationToken)
        {
            UpdateStatus(CNCState.Running, "Homing");
            await SendCommandAsync("G28", cancellationToken);
            UpdateStatus(CNCState.Idle, "Homing completed.");
        }

        public async Task ChangeToolAsync(int toolNumber, CancellationToken cancellationToken)
        {
            UpdateStatus(CNCState.Running, $"Changing tool to T{toolNumber}");
            string toolChangeCommand = $"T{toolNumber} M6";
            await SendCommandAsync(toolChangeCommand, cancellationToken);
            UpdateStatus(CNCState.Idle, $"Tool changed to T{toolNumber}");
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
