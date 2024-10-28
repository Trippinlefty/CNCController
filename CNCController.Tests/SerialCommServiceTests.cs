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
        var cancellationToken = new CancellationTokenSource().Token;
        _mockSerialCommService.Setup(s => s.ConnectAsync("COM1", 115200, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _mockSerialCommService.Object.ConnectAsync("COM1", 115200, cancellationToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendCommandAsync_ShouldInvokeSend_WhenCommandIsValid()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        _mockSerialCommService.Setup(s => s.SendCommandAsync("G28", cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _mockSerialCommService.Object.SendCommandAsync("G28", cancellationToken);

        // Assert
        _mockSerialCommService.Verify(s => s.SendCommandAsync("G28", cancellationToken), Times.Once);
        Assert.True(result);
    }
}