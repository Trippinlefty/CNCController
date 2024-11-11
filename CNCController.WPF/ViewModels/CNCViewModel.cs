using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController.Core.Models;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.ErrorHandle;
using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;

namespace CNCController.ViewModels;

public class CNCViewModel : ViewModelBase
{
    private readonly ICncController _cncController;
    private readonly ILogger<CNCViewModel> _logger;
    private readonly IErrorHandler _errorHandler;
    private readonly ISerialCommService _serialCommService;
    private CncStatus _currentStatus;
    private string _statusMessage;
    
    public string ErrorMessage { get; private set; }

    public CncStatus CurrentStatus
    {
        get => _currentStatus;
        set => SetProperty(ref _currentStatus, value, nameof(CurrentStatus));
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand JogCommand { get; }
    public ICommand HomeCommand { get; }
    
    public ICommand RunSetupWizardCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public CNCViewModel(ICncController cncController, ILogger<CNCViewModel> logger, IErrorHandler errorHandler, ISerialCommService serialCommService)
    {
        _cncController = cncController;
        _logger = logger;
        _errorHandler = errorHandler;
        _serialCommService = serialCommService;

        StartCommand = new AsyncRelayCommand(ExecuteStartCommand, CanExecuteStartCommand);
        StopCommand = new AsyncRelayCommand(ExecuteStopCommand, CanExecuteStopCommand);
        PauseCommand = new AsyncRelayCommand(ExecutePauseCommand, CanExecutePauseCommand);
        JogCommand = new RelayCommand(ExecuteJogCommand);
        HomeCommand = new RelayCommand(ExecuteHomeCommand); // Now compatible with RelayCommand
        RunSetupWizardCommand = new AsyncRelayCommand(OpenSetupWizard);
    }

    private Task OpenSetupWizard()
    {
        // Create and show the SetupWizard window
        var setupWizard = new SetupWizard
        {
            DataContext = new SetupWizardViewModel(_serialCommService) // Pass dependency here
        };
        setupWizard.ShowDialog(); // Open as a modal dialog
        return Task.CompletedTask;
    }

    private bool SetProperty<T>(ref T backingField, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value)) return false;
        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task ExecuteStartCommand() { /* Start logic */ }
    private async Task ExecuteStopCommand() { /* Stop logic */ }
    private async Task ExecutePauseCommand() { /* Pause logic */ }

    private void ExecuteJogCommand(object param) { /* Jog logic */ }
    private void ExecuteHomeCommand(object? param)
    {
        try
        {
            _cncController?.HomeAsync(new CancellationTokenSource().Token).Wait();
            UpdateStatus("Homing...");
            _logger.LogInformation("Homing...");
        }
        catch (Exception ex)
        {
            HandleError("Failed to execute home command", ex);
        }
    }

    private bool CanExecuteStartCommand() => _currentStatus.StateMessage != "Running";
    private bool CanExecutePauseCommand() => _currentStatus.StateMessage == "Running";
    private bool CanExecuteStopCommand() => _currentStatus.StateMessage != "Idle";
    
    private void HandleError(string context, Exception ex)
    {
        ErrorMessage = $"Error: {context}";
        _errorHandler.HandleException(ex); // This now handles both logging and error context
        UpdateStatus(ErrorMessage);
    }

    void UpdateStatus(string errorMessage)
    {
        
    }
}
