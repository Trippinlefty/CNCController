using CNCController.Core.Exceptions;
using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests
{
    public class GCodeParserTests
    {
        private readonly Mock<ILogger<GCodeParser>> _mockLogger;
        private readonly GCodeParser _gCodeParser;

        public GCodeParserTests()
        {
            _mockLogger = new Mock<ILogger<GCodeParser>>();

            // Real instance of GlobalErrorHandler with mocked ILogger
            var errorHandlerLogger = new Mock<ILogger<GlobalErrorHandler>>();
            var globalErrorHandler = new GlobalErrorHandler(errorHandlerLogger.Object);

            _gCodeParser = new GCodeParser(_mockLogger.Object, globalErrorHandler);
        }

        [Fact]
        public void Parse_ValidGCode_ReturnsGCodeCommand()
        {
            var gCodeCommand = _gCodeParser.Parse("G1 X10 Y20");

            Assert.Equal(GCodeCommandType.Motion, gCodeCommand.CommandType);
            Assert.Equal(10, gCodeCommand.Parameters["X"]);
            Assert.Equal(20, gCodeCommand.Parameters["Y"]);
        }

        [Fact]
        public void Parse_EmptyGCode_ThrowsInvalidGCodeException()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidGCodeException>(() => _gCodeParser.Parse(""));
            Assert.Contains("The G-code command is empty or whitespace.", exception.Message);

            // Verify that LogError was called with the appropriate message
            _mockLogger.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("G-code command is empty or whitespace.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ), Times.Once);
        }

        [Fact]
        public void Parse_InvalidGCode_ThrowsInvalidGCodeException()
        {
            // Arrange & Act
            var exception = Assert.Throws<InvalidGCodeException>(() => _gCodeParser.Parse("InvalidCommand"));

            // Assert
            Assert.Contains("The G-code command is improperly formatted", exception.Message);

            // Verify that the logger logged the formatting error
            _mockLogger.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The G-code command is improperly formatted.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ), Times.Once);
        }

        [Fact]
        public void Parse_UnsupportedGCodeCommand_ThrowsInvalidGCodeException()
        {
            // Arrange & Act
            var exception = Assert.Throws<InvalidGCodeException>(() => _gCodeParser.Parse("M200"));

            // Assert
            Assert.Contains("Unsupported G-code command", exception.Message);

            // Verify that the logger logged the unsupported command error
            _mockLogger.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unsupported G-code command")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ), Times.Once);
        }

    }
}
