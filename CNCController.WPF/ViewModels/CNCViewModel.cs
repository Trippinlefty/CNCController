using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.ErrorHandle;
using CNCController.Core.Services.RelayCommand;
using CNCController.ViewModels;
using Microsoft.Extensions.Logging;

public class CNCViewModel : ViewModelBase
{
    private readonly ICncController _cncController;
    private readonly ILogger<CNCViewModel> _logger;
    private readonly IErrorHandler _errorHandler;
    private readonly StatusViewModel _statusViewModel;
    
    private CncStatus _currentStatus;
    private string _statusMessage;

    public string ErrorMessage { get; private set; }

    public CncStatus CurrentStatus
    {
        get => _currentStatus;
        set => SetProperty(ref _currentStatus, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand JogCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand ShowJoggingControlCommand { get; }
    public ICommand RunSetupWizardCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public CNCViewModel(ICncController cncController, ILogger<CNCViewModel> logger, IErrorHandler errorHandler, StatusViewModel statusViewModel)
    {
        _cncController = cncController ?? throw new ArgumentNullException(nameof(cncController));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _statusViewModel = statusViewModel ?? throw new ArgumentNullException(nameof(statusViewModel));

        _cncController.StatusUpdated += OnStatusUpdated;
        _cncController.ErrorOccurred += OnErrorOccurred;

        StartCommand = new AsyncRelayCommand(ExecuteStartCommand, CanExecuteStartCommand);
        StopCommand = new AsyncRelayCommand(ExecuteStopCommand, CanExecuteStopCommand);
        PauseCommand = new AsyncRelayCommand(ExecutePauseCommand, CanExecutePauseCommand);
        JogCommand = new AsyncRelayCommand(ExecuteJogCommand);
        HomeCommand = new AsyncRelayCommand(ExecuteHomeCommand);
        RunSetupWizardCommand = new AsyncRelayCommand(OpenSetupWizard);
        ShowJoggingControlCommand = new RelayCommand(OpenJoggingControl);
    }

    private void OnErrorOccurred(object? sender, string errorMessage)
    {
        _statusViewModel.SetError(errorMessage);
    }

    private void OnStatusUpdated(object? sender, CncStatus status)
    {
        _statusViewModel.CurrentOperation = status.StateMessage;
    }

    private async Task OpenSetupWizard()
    {
        var setupWizard = new SetupWizard
        {
            DataContext = new SetupWizardViewModel(_cncController.SerialCommService, _cncController.ConfigurationService)
        };
        setupWizard.ShowDialog();
    }

    private async Task ExecuteStartCommand()
    {
        await _cncController.StartAsync(CancellationToken.None);
        _statusViewModel.CurrentOperation = "CNC started.";
    }

    private async Task ExecuteStopCommand()
    {
        await _cncController.StopAsync(CancellationToken.None);
        _statusViewModel.CurrentOperation = "CNC stopped.";
    }

    private async Task ExecutePauseCommand()
    {
        await _cncController.PauseAsync(CancellationToken.None);
        _statusViewModel.CurrentOperation = "CNC paused.";
    }

    private async Task ExecuteJogCommand()
    {
        await _cncController.JogAsync("X", 10, CancellationToken.None); // Placeholder for jogging logic
    }

    private async Task ExecuteHomeCommand()
    {
        try
        {
            await _cncController.HomeAsync(CancellationToken.None);
            _statusViewModel.CurrentOperation = "Homing...";
            _logger.LogInformation("Homing...");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _statusViewModel.SetError("Failed to execute home command.");
        }
    }

    private bool CanExecuteStartCommand() => _currentStatus?.StateMessage != "Running";
    private bool CanExecutePauseCommand() => _currentStatus?.StateMessage == "Running";
    private bool CanExecuteStopCommand() => _currentStatus?.StateMessage != "Idle";

    private void OpenJoggingControl(object? parameter)
    {
        var joggingWindow = new JoggingControlWindow
        {
            DataContext = new JoggingControlViewModel(_cncController)
        };
        joggingWindow.Show();
    }
}
