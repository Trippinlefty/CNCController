using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;

public class CNCViewModel : INotifyPropertyChanged
{
    private readonly ICNCController _cncController;
    private readonly ISerialCommService _serialCommService;
    private CNCStatus _currentStatus;

    public CNCViewModel(ICNCController cncController, ISerialCommService serialCommService)
    {
        _cncController = cncController;
        _serialCommService = serialCommService;
        
        RefreshAvailablePorts();

        // Initialize commands
        JogCommand = new RelayCommand(
            execute: param => ExecuteJogCommand(param),
            canExecute: param => CanExecuteJogCommand(param)
        );
        HomeCommand = new RelayCommand(
            execute: _ => ExecuteHomeCommand(),
            canExecute: _ => CanExecuteHomeCommand()
        );
        ConnectCommand = new RelayCommand(
            execute: async param => await ExecuteConnectCommand(),
            canExecute: param => CanExecuteConnectCommand()
        );
        DisconnectCommand = new RelayCommand(
            execute: async param => await ExecuteDisconnectCommand(),
            canExecute: param => CanExecuteDisconnectCommand()
        );
    }

    private string _portName;
    public string PortName
    {
        get => _portName;
        set
        {
            if (_portName != value)
            {
                _portName = value;
                Console.WriteLine($"PortName set to: {_portName}");
                //OnPropertyChanged(nameof(PortName));
            }
        }
    }

    private int _baudRate;
    private bool _isConnecting = false;
    public int BaudRate
    {
        get => _baudRate;
        set
        {
            if (_baudRate != value)
            {
                _baudRate = value;
                Console.WriteLine($"BaudRate set to: {_baudRate}");
                //OnPropertyChanged(nameof(BaudRate));
            }
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
    
    public ObservableCollection<string> AvailablePorts { get; private set; }

    // Commands for UI actions
    public ICommand JogCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Connection command execution
    private async Task ExecuteConnectCommand()
    {
        if (_isConnecting) return; // Prevent re-entry
        _isConnecting = true;
        
        try
        {
            if (await _serialCommService.ConnectAsync(PortName, BaudRate, CancellationToken.None))
            {
                UpdateStatus("Connected");
            }
            else
            {
                UpdateStatus("Failed to connect");
            }
        }
        finally
        { 
            _isConnecting = false;
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
        } 
    }

    private bool CanExecuteConnectCommand() => !_serialCommService.IsConnected;

    private async Task ExecuteDisconnectCommand()
    {
        await _serialCommService.DisconnectAsync();
        UpdateStatus("Disconnected");
    }

    private bool CanExecuteDisconnectCommand() => _serialCommService.IsConnected;

    // Jog and Home command execution
    private void ExecuteJogCommand(object param)
    {
        _cncController.JogAsync("X", 10, new CancellationTokenSource().Token).Wait();
        UpdateStatus("Jogging...");
    }

    private bool CanExecuteJogCommand(object param) => true;

    private void ExecuteHomeCommand()
    {
        _cncController.HomeAsync(new CancellationTokenSource().Token).Wait();
        UpdateStatus("Homing...");
    }

    private bool CanExecuteHomeCommand() => true;

    private void UpdateStatus(string message)
    {
        CurrentStatus = new CNCStatus { StateMessage = message };
    }
    
    public void RefreshAvailablePorts()
    {
        var ports = SerialPort.GetPortNames();
        AvailablePorts = new ObservableCollection<string>(ports);
        OnPropertyChanged(nameof(AvailablePorts));
    }
}