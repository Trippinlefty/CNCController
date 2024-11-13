using CNCController.Core.Exceptions;
using CNCController.Core.Models;
using CNCController.Core.Services.ErrorHandle;
using Microsoft.Extensions.Logging;

namespace CNCController.Core.Services.GCodeProcessing
{
    public class GCodeParser : IGCodeParser
    {
        private readonly ILogger<GCodeParser> _logger;
        private readonly IErrorHandler _errorHandler;

        public GCodeParser(ILogger<GCodeParser> logger, IErrorHandler errorHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        public GCodeModel ParseGCode(string gCodeText)
        {
            var gCodeModel = new GCodeModel();
            var lines = gCodeText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    var command = Parse(line.Trim());
                    if (command.Validate())
                        gCodeModel.AddCommand(command);
                }
                catch (InvalidGCodeException ex)
                {
                    _errorHandler.HandleException(ex, null);
                    _logger.LogWarning($"Skipped invalid G-code line: {line}");
                }
            }

            return gCodeModel;
        }

        public bool ValidateGCode(string gCodeText)
        {
            return gCodeText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .All(IsValidLine);
        }

        public GCodeCommand Parse(string rawCommand)
        {
            if (string.IsNullOrWhiteSpace(rawCommand))
                throw new InvalidGCodeException("G-code command cannot be empty or whitespace.");

            if (!IsValidLine(rawCommand))
                throw new InvalidGCodeException("The G-code command is improperly formatted.");

            GCodeCommandType commandType = DetermineCommandType(rawCommand);
            if (commandType == GCodeCommandType.Other)
                throw new InvalidGCodeException("Unsupported G-code command.");

            var parameters = ExtractParameters(rawCommand);
            return new GCodeCommand(commandType, parameters, rawCommand);
        }

        private bool IsValidLine(string line)
        {
            return line.StartsWith("G") || line.StartsWith("M") || line.StartsWith("T");
        }

        private static Dictionary<string, double> ExtractParameters(string command)
        {
            var parameters = new Dictionary<string, double>();
            var parts = command.Split(' ');

            foreach (var part in parts)
            {
                if (part.Length < 2) continue;

                var key = part.Substring(0, 1);
                if (double.TryParse(part.Substring(1), out double value))
                {
                    parameters[key] = value;
                }
            }

            return parameters;
        }

        private GCodeCommandType DetermineCommandType(string commandText)
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
}
