using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController.Core.Exceptions;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;

namespace CNCController.ViewModels;

public class CNCViewModel : INotifyPropertyChanged
{
    private readonly ILogger<CNCViewModel> _logger;
    private readonly GlobalErrorHandler _globalErrorHandler;
    private readonly ICNCController? _cncController;
    private readonly ISerialCommService _serialCommService;
    private readonly IConfigurationService _configService;
    private CNCStatus _currentStatus;

    public CNCViewModel(ICNCController? cncController, ISerialCommService serialCommService,
        ILogger<CNCViewModel> logger, IConfigurationService configService)
    {
        _cncController = cncController ?? throw new ArgumentNullException(nameof(cncController));
        _serialCommService = serialCommService ?? throw new ArgumentNullException(nameof(serialCommService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _globalErrorHandler = new GlobalErrorHandler(logger);

        _currentStatus = new CNCStatus { StateMessage = "Idle" } ?? throw new ArgumentNullException(nameof(_currentStatus)); 

        _serialCommService.ErrorOccurred += (sender, message) =>
        {
            ErrorMessage = message;
            _logger.LogWarning("Error occurred: {Message}", message);
        };

        _logger.LogInformation("CNCViewModel initialized.");
        RefreshAvailablePorts();

        // Initialize commands
        JogCommand = new RelayCommand(
            execute: ExecuteJogCommand,
            canExecute: CanExecuteJogCommand
        );
        HomeCommand = new RelayCommand(
            execute: _ => ExecuteHomeCommand(),
            canExecute: _ => CanExecuteHomeCommand()
        );
        
        ConnectCommand = new AsyncRelayCommand(async () => await ExecuteConnectCommand(), CanExecuteConnectCommand);
        DisconnectCommand = new AsyncRelayCommand(async () => await ExecuteDisconnectCommand(), CanExecuteDisconnectCommand);
        SaveSettingsCommand = new RelayCommand(async _ => await SaveSettingsAsync());
        StartCommand = new AsyncRelayCommand(async () => await ExecuteStartCommand(), CanExecuteStartCommand);
        PauseCommand = new AsyncRelayCommand(async () => await ExecutePauseCommand(), CanExecutePauseCommand);
        StopCommand = new AsyncRelayCommand(async () => await ExecuteStopCommand(), CanExecuteStopCommand);

        InitializeSettingsAsync();
    }

    private string _portName;

    public string PortName
    {
        get => _portName;
        set => SetProperty(ref _portName, value, nameof(PortName));
    }

    private int _baudRate;
    private bool _isConnecting = false;

    public int BaudRate
    {
        get => _baudRate;
        set => SetProperty(ref _baudRate, value, nameof(BaudRate));
    }

    public CNCStatus CurrentStatus
    {
        get => _currentStatus;
        set => SetProperty(ref _currentStatus, value, nameof(CurrentStatus));
    }

    private string _errorMessage;

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value, nameof(ErrorMessage));
    }
    
    private int _pollingInterval;
    public int PollingInterval
    {
        get => _pollingInterval;
        set => SetProperty(ref _pollingInterval, value, nameof(PollingInterval));
    }
    
    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
    }

    public ObservableCollection<string> AvailablePorts { get; private set; }

    // Commands for UI actions
    public ICommand JogCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;
    private void ApplyConfiguration(AppConfig config)
    {
        PortName = config.PortName;
        BaudRate = config.BaudRate;
        PollingInterval = config.PollingInterval;
    }
    
    private async Task InitializeSettingsAsync()
    {
        var config = await _configService.LoadConfigAsync();
        ApplyConfiguration(config);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private bool SetProperty<T>(ref T backingField, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value)) return false;

        backingField = value;
        OnPropertyChanged(propertyName);
        _logger.LogInformation($"{propertyName} set to: {value}");
        return true;
    }

    // Connection command execution
    private async Task ExecuteConnectCommand()
    {
        try
        {
            UpdateStatus("Connecting...");
            bool connected = await _serialCommService.ConnectAsync(PortName, BaudRate, CancellationToken.None);
            UpdateStatus(connected ? "Connected" : "Failed to connect");
        }
        catch (Exception ex)
        {
            HandleError("Connection failed", ex);
        }
    }

    private async Task ExecuteDisconnectCommand()
    {
        _logger.LogInformation("Disconnecting from CNC...");

        try
        {
            await _serialCommService.DisconnectAsync();
            UpdateStatus("Disconnected");
            _logger.LogInformation("Disconnected successfully.");
        }
        catch (Exception ex)
        {
            HandleError(ErrorMessage, ex);
        }
        finally
        {
            (ConnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (DisconnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    private bool CanExecuteConnectCommand() => !_serialCommService.IsConnected;

    private bool CanExecuteDisconnectCommand() => _serialCommService.IsConnected;

    private void ExecuteJogCommand(object param)
    {
        try
        {
            _cncController?.JogAsync("X", 10, new CancellationTokenSource().Token).Wait();
            UpdateStatus("Jogging...");
            _logger.LogInformation("Jogging...");
        }
        catch (Exception ex)
        {
            HandleError(ErrorMessage, ex);
        }
    }

    private void ExecuteHomeCommand()
    {
        try
        {
            _cncController?.HomeAsync(new CancellationTokenSource().Token).Wait();
            UpdateStatus("Homing...");
            _logger.LogInformation("Homing...");
        }
        catch (Exception ex)
        {
            HandleError(ErrorMessage, ex);
        }
    }

    private static bool CanExecuteJogCommand(object param) => true;

    private static bool CanExecuteHomeCommand() => true;
    private bool CanExecuteStartCommand() => _currentStatus.StateMessage != "Running";
    private bool CanExecutePauseCommand() => _currentStatus.StateMessage == "Running";
    private bool CanExecuteStopCommand() => _currentStatus.StateMessage != "Idle";

    private void UpdateStatus(string message)
    {
        CurrentStatus = new CNCStatus { StateMessage = message };
        _logger.LogWarning("Status Updated.");
    }

    public void RefreshAvailablePorts()
    {
        var ports = SerialPort.GetPortNames();
        AvailablePorts = new ObservableCollection<string>(ports);
        OnPropertyChanged(nameof(AvailablePorts));
        _logger.LogWarning("Available Ports Refreshed.");
    }
    
    private void HandleError(string context, Exception ex)
    {
        // Customize error messages based on the type of exception.
        string specificMessage = ex switch
        {
            IOException => "Serial port error. Please check CNC connection and port.",
            InvalidOperationException => "Invalid operation attempted. Verify CNC state.",
            FormatException => "Invalid command format. Check G-code syntax.",
            TimeoutException => "Command timed out. Verify CNC is responding.",
            CNCOperationException cncEx => cncEx.Message, // Directly use the custom message in CNCOperationException
            _ => "An unexpected error occurred. Contact support."  // Generic fallback
        };

        // Prefix with "Error:" as required by the test cases.
        ErrorMessage = $"Error: {context} - {specificMessage}";

        // Log the exception and message centrally through the error handler
        _globalErrorHandler.HandleException(ex);
        _logger.LogError(ex, ErrorMessage);

        // Update the status for the UI to show the error state
        UpdateStatus(ErrorMessage);
    }

    private async Task SaveSettingsAsync()
    {
        var success = await _configService.UpdateConfigAsync("PortName", PortName) &&
                      await _configService.UpdateConfigAsync("BaudRate", BaudRate.ToString()) &&
                      await _configService.UpdateConfigAsync("PollingInterval", PollingInterval.ToString());
        Console.WriteLine($"PollingInterval: {_pollingInterval}");
        if (success)
        {
            StatusMessage = "Settings saved successfully.";
        }
        else
        {
            StatusMessage = "Failed to save settings.";
        }
    }

    private async Task ExecutePauseCommand()=>
        await ExecuteCommandWithStatusUpdate(
            () => _cncController.StartAsync(CancellationToken.None), 
            "Paused", 
            "Pause", 
            StartCommand, PauseCommand, StopCommand);

    public async Task ExecuteStartCommand()
    {
        try
        {
            UpdateStatus("Starting...");
            await _cncController?.StartAsync(CancellationToken.None);
            UpdateStatus("Running");
            _logger.LogInformation("CNC started.");
        }
        catch (Exception ex)
        {
            HandleError("Failed to start the CNC.", ex);
        }
        finally
        {
            (StartCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (PauseCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (StopCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    private async Task ExecuteStopCommand() =>
        await ExecuteCommandWithStatusUpdate(
            () => _cncController.StopAsync(CancellationToken.None), 
            "Stopped", 
            "Stop", 
            StartCommand, PauseCommand, StopCommand);
    
    private async Task ExecuteCommandWithStatusUpdate(Func<Task> commandAction, string statusOnSuccess, string statusOnError, params ICommand[] commandsToUpdate)
    {
        try
        {
            UpdateStatus($"{statusOnSuccess}...");
            await commandAction();
            UpdateStatus(statusOnSuccess);
            _logger.LogInformation($"{statusOnSuccess} completed.");
        }
        catch (Exception ex)
        {
            HandleError(ErrorMessage, ex);
        }
        finally
        {
            foreach (var command in commandsToUpdate)
            {
                (command as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
}