﻿using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static Moq.It;

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
                IsAny<EventId>(),
                Is<IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled Exception")),
                exception,
                IsAny<Func<IsAnyType, Exception, string>>()),
            Times.Once);
    }
}