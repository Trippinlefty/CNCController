using CNCController.Core.Services.GCodeProcessing;

namespace CNCController.Core.Models;

public class GCodeCommand : IGCodeCommand
{
    public string CommandText { get; set; } // Optional for storing raw command text
    public GCodeCommandType CommandType { get; } // Use GCodeCommandType for type
    public Dictionary<string, double> Parameters { get; }

    // Unified constructor
    public GCodeCommand(GCodeCommandType commandType, Dictionary<string, double> parameters, string commandText = "")
    {
        CommandType = commandType;
        Parameters = parameters;
        CommandText = commandText;
    }

    public bool Validate()
    {
        return CommandType switch
        {
            GCodeCommandType.Motion => Parameters.ContainsKey("X") || Parameters.ContainsKey("Y") ||
                                       Parameters.ContainsKey("Z"),
            GCodeCommandType.SpindleControl => true,
            _ => CommandType != GCodeCommandType.Other
        };
    }
}