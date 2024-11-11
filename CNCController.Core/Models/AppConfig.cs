namespace CNCController.Core.Models;

public class AppConfig
{
    public string PortName { get; set; } = "COM1";   // Default port
    public int BaudRate { get; set; } = 115200;      // Default baud rate
    public int PollingInterval { get; set; } = 1000; // Default polling interval in milliseconds
    public Dictionary<string, string> MachineSettings { get; set; } = new Dictionary<string, string>();
}