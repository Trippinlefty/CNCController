namespace CNCController.Core.Services.GCodeProcessing;

public interface IGCodeCommand
{
    GCodeCommandType CommandType { get; }
    Dictionary<string, double> Parameters { get; }
}