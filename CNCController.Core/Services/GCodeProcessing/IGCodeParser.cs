namespace CNCController.Core.Services.GCodeProcessing
{
    public interface IGCodeParser
    {
        GCodeModel ParseGCode(string gCodeText);
        bool ValidateGCode(string gCodeText);
        GCodeCommandType DetermineCommandType(string commandText);
    }
}
