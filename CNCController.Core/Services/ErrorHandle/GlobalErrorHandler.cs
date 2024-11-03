using System;
using System.IO;
using Microsoft.Extensions.Logging;

public class GlobalErrorHandler
{
    private readonly ILogger _logger;

    public GlobalErrorHandler(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void HandleException(Exception ex)
    {
        string message = ex switch
        {
            IOException => "Communication error. Check the CNC connection and port settings.",
            OperationCanceledException => "Operation canceled. Please verify CNC state and try again.",
            FormatException => "Invalid command format detected. Please correct the command and retry.",
            TimeoutException => "Operation timed out. Check connection and retry.",
            _ => "An unexpected error occurred. Please restart or contact support."
        };

        _logger.LogError(ex, message);
        // Optional: Display this message to the user in the UI or log it as needed
    }
}