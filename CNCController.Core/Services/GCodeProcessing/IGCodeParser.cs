namespace CNCController.Core.Services.GCodeProcessing
{
    public interface IGCodeParser
    {
        GCodeModel ParseGCode(string gCodeText);     // Parses and validates GCode text into a model
        bool ValidateGCode(string gCodeText);        // Optionally, allows independent validation if needed
    }
}
