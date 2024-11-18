namespace CNCController.Core.Models
{
    public class GrblStatus
    {
        public string State { get; set; } = string.Empty; // e.g., "Idle", "Run", "Alarm"
        public double[] MachinePosition { get; set; } = new double[3]; // e.g., [X, Y, Z]
        public double[] WorkPosition { get; set; } = new double[3]; // e.g., [X, Y, Z]
        public double[] FeedAndSpindleSpeeds { get; set; } = new double[2]; // e.g., [Feed, Spindle]

        public GrblStatus() { }
    }
}