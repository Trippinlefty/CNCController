namespace CNCController.Core.Services.SerialCommunication
{
    public interface ISerialCommService
    {
        Task<bool> ConnectAsync(string portName, int baudRate);
        Task DisconnectAsync();
        Task<bool> SendCommandAsync(string command, int timeout = 2000);
        event EventHandler<string>? DataReceived;   // Event for receiving data
        event EventHandler? ConnectionOpened;       // Event for successful connection
        event EventHandler? ConnectionClosed;       // Event for disconnection
        event EventHandler<string>? ErrorOccurred;  // Event for errors
        bool IsConnected { get; }
    }
}
