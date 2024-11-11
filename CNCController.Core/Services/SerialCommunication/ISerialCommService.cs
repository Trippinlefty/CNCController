namespace CNCController.Core.Services.SerialCommunication;

public interface ISerialCommService
{
    Task<bool> ConnectAsync(string portName, int baudRate, CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken);
    
    IEnumerable<string> GetAvailablePorts();    // New method for retrieving available COM ports

    event EventHandler<string>? DataReceived;   // Event for receiving data
    event EventHandler? ConnectionOpened;       // Event for successful connection
    event EventHandler? ConnectionClosed;       // Event for disconnection
    event EventHandler<string>? ErrorOccurred;  // Event for errors

    bool IsConnected { get; }
}