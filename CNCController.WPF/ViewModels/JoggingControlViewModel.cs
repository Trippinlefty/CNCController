using System.Threading;
using System.Windows.Input;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.RelayCommand;

namespace CNCController.ViewModels;

public class JoggingControlViewModel : ViewModelBase
{
    private readonly ICncController _cncController;

    public JoggingControlViewModel(ICncController cncController)
    {
        _cncController = cncController;

        JogUpCommand = new RelayCommand((_ => Jog("Y", 10)));
        JogDownCommand = new RelayCommand((_ => Jog("Y", -10)));
        JogLeftCommand = new RelayCommand((_ => Jog("X", -10)));
        JogRightCommand = new RelayCommand((_ => Jog("X", 10)));
    }

    public ICommand JogUpCommand { get; }
    public ICommand JogDownCommand { get; }
    public ICommand JogLeftCommand { get; }
    public ICommand JogRightCommand { get; }

    private async void Jog(string axis, double distance)
    {
        // Execute the jog command on the CNC controller
        await _cncController.JogAsync(axis, distance, CancellationToken.None);
    }
}