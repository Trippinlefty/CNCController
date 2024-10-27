using System;
using System.Threading.Tasks;
using CNCController.Core.Services.SerialCommunication;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

public class SerialCommServiceTests
{
    private readonly Mock<ISerialCommService> _mockSerialCommService;

    public SerialCommServiceTests()
    {
        _mockSerialCommService = new Mock<ISerialCommService>();
    }

    [Fact]
    public async Task ConnectAsync_ShouldReturnTrue_WhenConnectionIsSuccessful()
    {
        // Arrange
        _mockSerialCommService.Setup(s => s.ConnectAsync("COM1", 115200)).ReturnsAsync(true);

        // Act
        var result = await _mockSerialCommService.Object.ConnectAsync("COM1", 115200);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendCommandAsync_ShouldInvokeSend_WhenCommandIsValid()
    {
        // Arrange
        _mockSerialCommService.Setup(s => s.SendCommandAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockSerialCommService.Object.SendCommandAsync("G28", 2000);

        // Assert
        _mockSerialCommService.Verify(s => s.SendCommandAsync("G28", 2000), Times.Once);
        Assert.True(result);
    }
}