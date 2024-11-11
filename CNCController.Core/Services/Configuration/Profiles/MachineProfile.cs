namespace CNCController.Core.Services.Configuration.Profiles;

public class MachineProfile
{
    public string Name { get; set; }
    public int StepsPerMM { get; set; }
    public int MaxSpeed { get; set; }
    public int SpindleSpeedMin { get; set; }
    public int SpindleSpeedMax { get; set; }
    // Other properties as needed
}