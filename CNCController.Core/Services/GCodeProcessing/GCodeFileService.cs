using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;
using Microsoft.Extensions.Logging;

public class GCodeFileService
{
    private readonly IGCodeParser _parser;
    private readonly ILogger<GCodeFileService> _logger;

    public GCodeFileService(IGCodeParser parser, ILogger<GCodeFileService> logger)
    {
        _parser = parser;
        _logger = logger;
    }

    public List<GCodeCommand> ImportGCodeFile(string filePath)
    {
        try
        {
            _logger.LogInformation($"Loading GCode file: {filePath}");
        
            // Cast or map the IGCodeCommand list to GCodeCommand list
            return _parser.ParseFile(filePath).OfType<GCodeCommand>().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import GCode file.");
            throw new InvalidOperationException("Failed to import GCode file.", ex);
        }
    }
}