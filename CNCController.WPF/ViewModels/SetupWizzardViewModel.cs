using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController.Core.Services.SerialCommunication;
using CNCController.Core.Services.RelayCommand;
using CNCController.ViewModels;

public class SetupWizardViewModel : ViewModelBase
{
    private readonly ISerialCommService _serialCommService;

    public ObservableCollection<string> AvailablePorts { get; } = new();
    public ObservableCollection<int> BaudRates { get; } = new() { 9600, 115200 };

    private string _selectedPort;
    public string SelectedPort
    {
        get => _selectedPort;
        set => SetProperty(ref _selectedPort, value);
    }

    private int _selectedBaudRate;
    public int SelectedBaudRate
    {
        get => _selectedBaudRate;
        set => SetProperty(ref _selectedBaudRate, value);
    }

    public ICommand TestConnectionCommand { get; }

    public SetupWizardViewModel(ISerialCommService serialCommService)
    {
        _serialCommService = serialCommService;

        // Update TestConnectionCommand to use AsyncRelayCommand correctly
        TestConnectionCommand = new AsyncRelayCommand(TestConnection);

        // Populate available ports
        RefreshAvailablePorts();
    }

    private void RefreshAvailablePorts()
    {
        var ports = _serialCommService.GetAvailablePorts();
        AvailablePorts.Clear();
        foreach (var port in ports)
        {
            AvailablePorts.Add(port);
        }
    }

    private async Task TestConnection()
    {
        bool success = await _serialCommService.ConnectAsync(SelectedPort, SelectedBaudRate, CancellationToken.None);
        
        // Example handling based on connection success
        if (success)
        {
            // Connection succeeded - provide user feedback
            // e.g., show a success message
        }
        else
        {
            // Connection failed - provide user feedback
            // e.g., show an error message
        }
    }
}
