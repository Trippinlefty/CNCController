using CNCController.Core.Services.GCodeProcessing;

namespace CNCController.Core.Models
{
    public class GCodeCommand : IGCodeCommand
    {
        public string CommandText { get; set; }
        public GCodeCommandType CommandType { get; }
        public Dictionary<string, double> Parameters { get; }

        public GCodeCommand(GCodeCommandType commandType, Dictionary<string, double> parameters, string commandText = "")
        {
            CommandType = commandType;
            Parameters = parameters ?? new Dictionary<string, double>();
            CommandText = commandText;
        }

        public bool Validate()
        {
            return CommandType switch
            {
                GCodeCommandType.Motion => Parameters.ContainsKey("X") || Parameters.ContainsKey("Y") || Parameters.ContainsKey("Z"),
                GCodeCommandType.SpindleControl => Parameters.ContainsKey("Speed"),
                GCodeCommandType.ToolChange => Parameters.ContainsKey("ToolNumber"),
                _ => false
            };
        }

        // Convert command to a string representation (optional helper method)
        public override string ToString()
        {
            return CommandText != string.Empty ? CommandText : $"{CommandType} {string.Join(" ", Parameters.Select(p => $"{p.Key}{p.Value}"))}";
        }
    }
}