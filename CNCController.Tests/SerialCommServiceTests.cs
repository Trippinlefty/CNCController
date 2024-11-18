using Xunit;
using Moq;
using System.Threading.Tasks;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;
using CNCController.Core.Services.ErrorHandle;
using Assert = Xunit.Assert;

namespace CNCController.Tests
{
    public class SerialCommServiceTests
    {
        private readonly SerialCommService _service;
        private readonly Mock<IErrorHandler> _mockErrorHandler;
        private readonly Mock<ILogger<SerialCommService>> _mockLogger;

        public SerialCommServiceTests()
        {
            _mockErrorHandler = new Mock<IErrorHandler>();
            _mockLogger = new Mock<ILogger<SerialCommService>>();
            _service = new SerialCommService(_mockLogger.Object, _mockErrorHandler.Object);
        }
        
        [Fact]
        public async Task ConnectAsync_ShouldReturnFalse_OnException()
        {
            // Setup to simulate failure
            var result = await _service.ConnectAsync("INVALID_PORT", 9600, default);
            Assert.False(result, "Connection should fail for invalid port.");
        }

        [Fact]
        public async Task SendCommandAsync_ShouldReturnFalse_WhenNotConnected()
        {
            var result = await _service.SendCommandAsync("VALID_COMMAND", default);

            Assert.False(result, "Command should not be sent when not connected.");
        }
    }
}