using System;
using System.Threading;
using System.Threading.Tasks;

namespace CNCController.Core.Services.CNCControl
{
    public interface ICNCController
    {
        Task JogAsync(string direction, double distance, CancellationToken cancellationToken);
        Task HomeAsync(CancellationToken cancellationToken);
        Task ChangeToolAsync(int toolNumber, CancellationToken cancellationToken);
        Task EmergencyStopAsync(CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
        Task PauseAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        
        CNCStatus GetCurrentStatus();

        event EventHandler<CNCStatus>? StatusUpdated;  // Event to update CNC status
        event EventHandler<string>? ErrorOccurred;     // Event to notify errors
    }
}