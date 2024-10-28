namespace CNCController.Core.Services.CNCControl;

public class CNCStatus
{
    public CNCState State { get; set; }
    public string Position { get; set; } = "0, 0, 0";
    public bool IsRunning { get; set; } = false;
    public string CurrentTool { get; set; } = "None";
    public string StateMessage { get; set; } = "Idle";
}