using CNCController.Core.Exceptions;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using CNCController.Core.Services.ErrorHandle;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests
{
    public class CNCControllerTests
    {
        private readonly Mock<ISerialCommService> _mockSerialCommService;
        private readonly Mock<IConfigurationService> _mockConfigService;
        private readonly Mock<ILogger<Core.Services.CNCControl.CncController>> _mockLogger;
        private readonly Mock<IErrorHandler> _mockErrorHandler; // Changed to IErrorHandler without constructor args
        private readonly Core.Services.CNCControl.CncController _cncController;

        public CNCControllerTests()
        {
            _mockSerialCommService = new Mock<ISerialCommService>();
            _mockConfigService = new Mock<IConfigurationService>();
            _mockLogger = new Mock<ILogger<Core.Services.CNCControl.CncController>>();
            _mockErrorHandler = new Mock<IErrorHandler>(); // No constructor args

            _cncController = new Core.Services.CNCControl.CncController(
                _mockSerialCommService.Object, 
                _mockConfigService.Object, 
                _mockLogger.Object, 
                _mockErrorHandler.Object);
        }

        [Fact]
        public async Task JogAsync_SendsJogCommand()
        {
            var cancellationToken = new CancellationTokenSource().Token;
            _mockSerialCommService
                .Setup(s => s.SendCommandAsync(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(true);

            await _cncController.JogAsync("X", 10, cancellationToken);

            _mockSerialCommService.Verify(
                s => s.SendCommandAsync("G91 G0 X10 F100", cancellationToken), 
                Times.Once);
        }

        [Fact]
        public async Task JogAsync_HandlesInvalidOperationException()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            _mockSerialCommService
                .Setup(s => s.SendCommandAsync(It.IsAny<string>(), cancellationToken))
                .ThrowsAsync(new InvalidOperationException("Jog error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CncOperationException>(() => 
                _cncController.JogAsync("X", 10, cancellationToken));

            Assert.Equal("Jog command failed.", exception.Message);
            _mockErrorHandler.Verify(e => e.HandleException(It.IsAny<InvalidOperationException>()), Times.Once);
        }
    }
}
