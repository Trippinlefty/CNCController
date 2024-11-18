using System;
using System.Threading;
using System.Windows.Input;
using CNCController.Core.Models;
using CNCController.Core.Services.CNCControl;
using Microsoft.Extensions.Logging;

namespace CNCController.ViewModels
{
    public class CNCViewModel : ViewModelBase
    {
        private readonly ICNCController _cncController;
        private readonly ILogger<CNCViewModel> _logger;
        private string _statusMessage;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand JogCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PauseCommand { get; }

        public CNCViewModel(ICNCController cncController, ILogger<CNCViewModel> logger)
        {
            _cncController = cncController ?? throw new ArgumentNullException(nameof(cncController));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _cncController.StatusUpdated += OnStatusUpdated;

            JogCommand = new AsyncRelayCommand(async () => await _cncController.JogAsync("X", 10, CancellationToken.None));
            HomeCommand = new AsyncRelayCommand(async () => await _cncController.HomeAsync(CancellationToken.None));
            StartCommand = new AsyncRelayCommand(async () => await _cncController.StartAsync(CancellationToken.None));
            StopCommand = new AsyncRelayCommand(async () => await _cncController.StopAsync(CancellationToken.None));
            PauseCommand = new AsyncRelayCommand(async () => await _cncController.PauseAsync(CancellationToken.None));
        }

        private void OnStatusUpdated(object sender, CncStatus status)
        {
            StatusMessage = status.StateMessage;
        }
    }
}