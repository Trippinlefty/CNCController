using Microsoft.Extensions.Logging;

public class GlobalErrorHandler
{
    private readonly ILogger _logger;

    public GlobalErrorHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void HandleException(Exception ex)
    {
        _logger.LogError(ex, "Unhandled Exception");
        // Optional UI notifications or actions
    }
}