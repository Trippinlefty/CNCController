using CNCController.Core.Services.CNCControl;

namespace CNCController.Core.Models
{
    public class CncStatus
    {
        public CncState State { get; set; }
        public string StateMessage { get; set; }
        public string Position { get; set; }
        public bool IsConnected { get; set; }

        public CncStatus()
        {
            // Default initialization
            State = CncState.Idle;
            StateMessage = "Idle";
            Position = "X: 0, Y: 0, Z: 0";
            IsConnected = false;
        }
    }
}