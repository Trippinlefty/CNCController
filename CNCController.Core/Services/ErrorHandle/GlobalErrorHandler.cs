using Microsoft.Extensions.Logging;

namespace CNCController.Core.Services.ErrorHandle
{
    public class GlobalErrorHandler : IErrorHandler
    {
        private readonly ILogger<GlobalErrorHandler> _logger;
        
        public event Action<string>? ErrorOccurred;

        public GlobalErrorHandler(ILogger<GlobalErrorHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void HandleException(Exception ex, string? customMessage = null)
        {
            var message = customMessage ?? ex switch
            {
                IOException => "Communication error. Check the CNC connection and port settings.",
                OperationCanceledException => "Operation canceled. Please verify CNC state and try again.",
                FormatException => "Invalid command format detected. Please correct the command and retry.",
                TimeoutException => "Operation timed out. Check connection and retry.",
                _ => "An unexpected error occurred. Please restart or contact support."
            };

            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(message);
        }
    }
}