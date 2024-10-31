using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;

namespace CNCController.ViewModels;

public class CNCViewModel : INotifyPropertyChanged
{
    private readonly ILogger<CNCViewModel> _logger;
    private readonly ICNCController? _cncController;
    private readonly ISerialCommService _serialCommService;
    private readonly IConfigurationService _configService;
    private CNCStatus _currentStatus;

    public CNCViewModel(ICNCController? cncController, ISerialCommService serialCommService,
        ILogger<CNCViewModel> logger, IConfigurationService configService)
    {
        _configService = configService;
        _cncController = cncController;
        _serialCommService = serialCommService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentStatus = new CNCStatus { StateMessage = "Idle" }; // Default state

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

        InitializeSettingsAsync();
    }

    private string _portName;

    public string PortName
    {
        get => _portName;
        set
        {
            if (_portName == value) return;
            _portName = value;
            Console.WriteLine($"PortName set to: {_portName}");
            OnPropertyChanged(nameof(PortName));
        }
    }

    private int _baudRate;
    private bool _isConnecting = false;

    public int BaudRate
    {
        get => _baudRate;
        set
        {
            if (_baudRate == value) return;
            _baudRate = value;
            Console.WriteLine($"BaudRate set to: {_baudRate}");
            OnPropertyChanged(nameof(BaudRate));
        }
    }

    public CNCStatus CurrentStatus
    {
        get => _currentStatus;
        set
        {
            _currentStatus = value;
            OnPropertyChanged(nameof(CurrentStatus));
        }
    }

    private string _errorMessage;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
    
    private int _pollingInterval;
    public int PollingInterval
    {
        get => _pollingInterval;
        set
        {
            _pollingInterval = value;
            OnPropertyChanged(nameof(PollingInterval));
        }
    }
    
    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    public ObservableCollection<string> AvailablePorts { get; private set; }

    // Commands for UI actions
    public ICommand JogCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SaveSettingsCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;
    
    private async Task InitializeSettingsAsync()
    {
        var config = await _configService.LoadConfigAsync();
        PortName = config.PortName;
        BaudRate = config.BaudRate;
        PollingInterval = config.PollingInterval;
        // load other machine settings if necessary
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Connection command execution
    private async Task ExecuteConnectCommand()
    {
        _logger.LogInformation("Attempting to connect...");
        if (_isConnecting) return;
        _isConnecting = true;

        try
        {
            if (await _serialCommService.ConnectAsync(PortName, BaudRate, CancellationToken.None))
            {
                UpdateStatus("Connected");
                _logger.LogInformation("Connected to CNC.");
            }
            else
            {
                UpdateStatus("Failed to connect");
                _logger.LogWarning("Connection failed.");
            }
        }
        catch (Exception ex)
        {
            HandleError("Connection error", ex);
        }
        finally
        {
            _isConnecting = false;
            (ConnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (DisconnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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
            HandleError("Disconnection error", ex);
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
            HandleError("Jogging error", ex);
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
            HandleError("Homing error", ex);
        }
    }

    private static bool CanExecuteJogCommand(object param) => true;

    private static bool CanExecuteHomeCommand() => true;

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
    
    private void HandleError(string message, Exception ex)
    {
        ErrorMessage = $"Error: {ex.Message}";
        _logger.LogError(ex, message);
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
}