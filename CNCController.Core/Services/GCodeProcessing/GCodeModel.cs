using CNCController.Core.Models;

namespace CNCController.Core.Services.GCodeProcessing;

public class GCodeModel
{
    public List<GCodeCommand> Commands { get; set; } = new List<GCodeCommand>();

    public void AddCommand(GCodeCommand command)
    {
        Commands.Add(command);
    }

    public void ClearCommands()
    {
        Commands.Clear();
    }
}