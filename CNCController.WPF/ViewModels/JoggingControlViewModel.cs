using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController.Core.Services.CNCControl;

namespace CNCController.ViewModels
{
    public class JoggingControlViewModel : ViewModelBase
    {
        private readonly ICncController _cncController;

        public JoggingControlViewModel(ICncController cncController)
        {
            _cncController = cncController;

            // Asynchronous jogging commands
            JogUpCommand = new AsyncRelayCommand(() => JogAsync("Y", 10));
            JogDownCommand = new AsyncRelayCommand(() => JogAsync("Y", -10));
            JogLeftCommand = new AsyncRelayCommand(() => JogAsync("X", -10));
            JogRightCommand = new AsyncRelayCommand(() => JogAsync("X", 10));
        }

        public ICommand JogUpCommand { get; }
        public ICommand JogDownCommand { get; }
        public ICommand JogLeftCommand { get; }
        public ICommand JogRightCommand { get; }

        private async Task JogAsync(string axis, double distance)
        {
            // Execute the jog command asynchronously on the CNC controller
            await _cncController.JogAsync(axis, distance, CancellationToken.None);
        }
    }
}