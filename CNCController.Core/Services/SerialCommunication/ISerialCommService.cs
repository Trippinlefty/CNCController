namespace CNCController.Core.Services.SerialCommunication
{
    public interface ISerialCommService : IDisposable
    {
        Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken);
        Task DisconnectAsync();
        Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken);
        
        Task<IEnumerable<string>> GetAvailablePortsAsync();  // Retained only the async version

        event EventHandler<string>? DataReceived;
        event EventHandler? ConnectionOpened;
        event EventHandler? ConnectionClosed;
        event EventHandler<string>? ErrorOccurred;

        bool IsConnected { get; }
    }
}