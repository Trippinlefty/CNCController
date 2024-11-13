using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;

namespace CNCController.Core.Services.CNCControl
{
    public interface ICncController
    {
        ISerialCommService SerialCommService { get; }
        IConfigurationService ConfigurationService { get; }

        Task JogAsync(string direction, double distance, CancellationToken cancellationToken);
        Task HomeAsync(CancellationToken cancellationToken);
        Task ChangeToolAsync(int toolNumber, CancellationToken cancellationToken);
        Task EmergencyStopAsync(CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
        Task PauseAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);

        CncStatus GetCurrentStatus();

        event EventHandler<CncStatus>? StatusUpdated;
        event EventHandler<string>? ErrorOccurred;
    }
}