using CNCController.Core.Exceptions;
using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;
using Microsoft.Extensions.Logging;

public class GCodeParser : IGCodeParser
{
    private readonly ILogger<GCodeParser> _logger;
    private readonly GlobalErrorHandler _globalErrorHandler;

    public GCodeParser(ILogger<GCodeParser> logger, GlobalErrorHandler globalErrorHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _globalErrorHandler = globalErrorHandler ?? throw new ArgumentNullException(nameof(globalErrorHandler));
    }

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

    public GCodeCommand Parse(string rawCommand)
    {
        if (string.IsNullOrWhiteSpace(rawCommand))
        {
            _logger.LogError("G-code command is empty or whitespace.");
            throw new InvalidGCodeException("The G-code command is empty or whitespace.");
        }

        // Check if the command has a valid G-code format
        if (!IsValidLine(rawCommand))
        {
            _logger.LogError("The G-code command is improperly formatted.");
            throw new InvalidGCodeException("The G-code command is improperly formatted.");
        }

        try
        {
            GCodeCommandType commandType = DetermineCommandType(rawCommand);

            // If the command is valid but unsupported, throw a specific error
            if (commandType == GCodeCommandType.Other)
            {
                _logger.LogError("Unsupported G-code command.");
                throw new InvalidGCodeException("Unsupported G-code command.");
            }

            Dictionary<string, double> parameters = ExtractParameters(rawCommand);
            return new GCodeCommand(commandType, parameters);
        }
        catch (FormatException ex)
        {
            _globalErrorHandler.HandleException(ex);
            _logger.LogError(ex, "G-code format is invalid.");
            throw new InvalidGCodeException("The G-code command is improperly formatted.", ex);
        }
        catch (Exception ex)
        {
            _globalErrorHandler.HandleException(ex);
            _logger.LogError(ex, "Unexpected error occurred while parsing G-code.");
            throw;
        }
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
