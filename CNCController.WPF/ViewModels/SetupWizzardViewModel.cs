using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;

namespace CNCController.ViewModels;

public class SetupWizardViewModel : ViewModelBase
{
    private readonly ISerialCommService _serialCommService;
    private readonly IConfigurationService _configService;

    public ObservableCollection<string> AvailablePorts { get; } = new();
    public ObservableCollection<int> BaudRates { get; } = new() { 9600, 115200 };

    public ICommand TestConnectionCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand RefreshPortsCommand { get; }

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

    public SetupWizardViewModel(ISerialCommService serialCommService, IConfigurationService configService)
    {
        _serialCommService = serialCommService;
        _configService = configService;

        TestConnectionCommand = new AsyncRelayCommand(TestConnection);
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAndClose);
        RefreshPortsCommand = new AsyncRelayCommand(RefreshAvailablePorts);

        // Refresh ports initially
        _ = RefreshAvailablePorts();
    }

    private async Task RefreshAvailablePorts()
    {
        var ports = await _serialCommService.GetAvailablePortsAsync();
        AvailablePorts.Clear();
        foreach (var port in ports)
        {
            AvailablePorts.Add(port);
        }
    }

    private async Task TestConnection()
    {
        var success = await _serialCommService.ConnectAsync(SelectedPort, SelectedBaudRate, CancellationToken.None);
        MessageBox.Show(success ? "Connection successful" : "Connection failed", success ? "Success" : "Error");
    }

    private async Task SaveSettingsAndClose()
    {
        await _configService.UpdateConfigAsync("PortName", SelectedPort);
        await _configService.UpdateConfigAsync("BaudRate", SelectedBaudRate.ToString());
        Application.Current.Windows.OfType<SetupWizard>().FirstOrDefault()?.Close();
    }
}
