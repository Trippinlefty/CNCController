using System;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.SerialCommunication;

namespace CNCController.ViewModels;

public class StatusViewModel : ViewModelBase
{
    private readonly ICncController _cncController;
    private readonly ISerialCommService _serialCommService;
    private string _position;
    private bool _isLimitSwitchTripped;
    private string _spindleStatus;
    private string _alarmState;
    private bool _isConnected;
    private string _currentOperation = "Idle";
    private string _errorMessage;


    public string Position
    {
        get => _position;
        private set => SetProperty(ref _position, value);
    }

    public bool IsLimitSwitchTripped
    {
        get => _isLimitSwitchTripped;
        private set => SetProperty(ref _isLimitSwitchTripped, value);
    }

    public string SpindleStatus
    {
        get => _spindleStatus;
        private set => SetProperty(ref _spindleStatus, value);
    }

    public string AlarmState
    {
        get => _alarmState;
        private set => SetProperty(ref _alarmState, value);
    }

    public event EventHandler LimitSwitchTriggered;
    public event EventHandler AlarmStateChanged;

    public StatusViewModel(ICncController cncController, ISerialCommService serialCommService)
    {
        _cncController = cncController;
        _serialCommService = serialCommService;
        _serialCommService.DataReceived += OnDataReceived;
    }

    private void OnDataReceived(object sender, string data)
    {
        // Parse data and update properties based on CNC feedback
        if (data.Contains("Position:"))
        {
            Position = ParsePosition(data);
        }

        if (data.Contains("LIMIT"))
        {
            IsLimitSwitchTripped = true;
            LimitSwitchTriggered?.Invoke(this, EventArgs.Empty);
        }
        
        if (data.Contains("ALARM"))
        {
            AlarmState = "Alarm Triggered";
            AlarmStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private string ParsePosition(string data)
    {
        // Logic to extract and format position information from data string
        return data.Replace("Position:", "").Trim();
    }
    
    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public string CurrentOperation
    {
        get => _currentOperation;
        set => SetProperty(ref _currentOperation, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    // Method to update the status when the connection status changes
    public void UpdateConnectionStatus(bool connected)
    {
        IsConnected = connected;
        CurrentOperation = connected ? "Connected" : "Disconnected";
    }

    // Method to set error messages from CNCViewModel
    public void SetError(string message)
    {
        ErrorMessage = message;
        CurrentOperation = "Error";
    }
}