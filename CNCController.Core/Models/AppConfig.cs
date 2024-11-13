namespace CNCController.Core.Models
{
    public class AppConfig
    {
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 115200;
        public int PollingInterval { get; set; } = 1000;
        public Dictionary<string, string> MachineSettings { get; set; } = new Dictionary<string, string>();

        // Validation for configuration values
        public bool IsValid() => BaudRate > 0 && PollingInterval > 0;
    }
}