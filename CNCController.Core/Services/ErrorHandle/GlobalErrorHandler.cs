using Microsoft.Extensions.Logging;

namespace CNCController.Core.Services.ErrorHandle
{
    public class GlobalErrorHandler : IErrorHandler
    {
        private readonly ILogger<GlobalErrorHandler> _logger;

        public GlobalErrorHandler(ILogger<GlobalErrorHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void HandleException(Exception exception, string contextMessage = "")
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            // Log the exception with optional context information
            var logMessage = string.IsNullOrEmpty(contextMessage) ? exception.Message : $"{contextMessage}: {exception.Message}";
            _logger.LogError(exception, logMessage);

            // Optional: Decide if certain exceptions should trigger additional actions, like sending alerts.
        }
    }
}