using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CNCController.Tests;

public class GlobalErrorHandlerTests
{
    [Fact]
    public void GlobalErrorHandler_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalErrorHandler>>();
        var globalErrorHandler = new GlobalErrorHandler(mockLogger.Object);
        var exception = new Exception("Test Exception");

        // Act
        globalErrorHandler.HandleException(exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled Exception")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}