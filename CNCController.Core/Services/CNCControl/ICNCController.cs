using CNCController.Core.Models;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;

namespace CNCController.Core.Services.CNCControl
{
    public interface ICNCController
    {
        Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken);
        Task DisconnectAsync();
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        Task PauseAsync(CancellationToken cancellationToken);
        Task EmergencyStopAsync(CancellationToken cancellationToken);
        Task HomeAsync(CancellationToken cancellationToken);
        Task JogAsync(string direction, double distance, CancellationToken cancellationToken);

        // These properties need to be included in the interface
        ISerialCommService SerialCommService { get; }
        IConfigurationService ConfigurationService { get; }
        
        event EventHandler<CncStatus>? StatusUpdated;
        event EventHandler<string>? ErrorOccurred;
    }
}