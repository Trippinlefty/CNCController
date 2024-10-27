using CNCController.Core.Services.CNCControl;

public interface ICNCController
{
    Task JogAsync(string direction, double distance);
    Task HomeAsync();
    Task ChangeToolAsync(int toolNumber);
    Task EmergencyStopAsync();
    CNCStatus GetCurrentStatus();

    event EventHandler<CNCStatus>? StatusUpdated;  // Event to update CNC status
    event EventHandler<string>? ErrorOccurred;     // Event to notify errors
}