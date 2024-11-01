using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;

public class GCodeParser : IGCodeParser
{
    public GCodeModel ParseGCode(string gCodeText)
    {
        var gCodeModel = new GCodeModel();
        var lines = gCodeText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || !IsValidLine(trimmedLine)) continue;

            var commandType = DetermineCommandType(trimmedLine);
            var parameters = ExtractParameters(trimmedLine);

            var gCodeCommand = new GCodeCommand(commandType, parameters); // Correct type reference
            if (gCodeCommand.Validate())
            {
                gCodeModel.AddCommand(gCodeCommand);
            }
        }

        return gCodeModel;
    }

    public bool ValidateGCode(string gCodeText)
    {
        var lines = gCodeText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return lines.All(IsValidLine);
    }

    private bool IsValidLine(string line)
    {
        return line.StartsWith("G") || line.StartsWith("M") || line.StartsWith("T");
    }

    public static GCodeCommandType DetermineCommandType(string commandText)
    {
        if (commandText.StartsWith("G0") || commandText.StartsWith("G1") || commandText.StartsWith("G2") || commandText.StartsWith("G3"))
            return GCodeCommandType.Motion;
        if (commandText.StartsWith("M3") || commandText.StartsWith("M5"))
            return GCodeCommandType.SpindleControl;
        if (commandText.StartsWith("T"))
            return GCodeCommandType.ToolChange;
        if (commandText.StartsWith("M8") || commandText.StartsWith("M9"))
            return GCodeCommandType.CoolantControl;
        if (commandText.StartsWith("M0") || commandText.StartsWith("M1"))
            return GCodeCommandType.Pause;

        return GCodeCommandType.Other;
    }

    public static GCodeCommand Parse(string rawCommand)
    {
        GCodeCommandType commandType = DetermineCommandType(rawCommand);
        Dictionary<string, double> parameters = ExtractParameters(rawCommand);
        return new GCodeCommand(commandType, parameters);
    }

    private static Dictionary<string, double> ExtractParameters(string command)
    {
        var parameters = new Dictionary<string, double>();
        string[] parts = command.Split(' ');

        foreach (var part in parts)
        {
            if (part.StartsWith("X") && double.TryParse(part.Substring(1), out double xVal))
                parameters["X"] = xVal;
            else if (part.StartsWith("Y") && double.TryParse(part.Substring(1), out double yVal))
                parameters["Y"] = yVal;
        }

        return parameters;
    }
}
