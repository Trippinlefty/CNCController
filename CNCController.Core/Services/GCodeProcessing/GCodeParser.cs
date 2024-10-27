using System;
using System.Linq;
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
            if (string.IsNullOrEmpty(trimmedLine)) continue;

            var commandType = DetermineCommandType(trimmedLine);
            gCodeModel.AddCommand(new GCodeCommand(trimmedLine, commandType));
        }

        return gCodeModel;
    }

    public bool ValidateGCode(string gCodeText)
    {
        var lines = gCodeText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;

            // Basic validation: Check if the line starts with a recognized G or M code
            if (!(trimmedLine.StartsWith("G") || trimmedLine.StartsWith("M") || trimmedLine.StartsWith("T")))
            {
                return false;
            }
        }
        return true;
    }

    public GCodeCommandType DetermineCommandType(string commandText)
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
}