using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests;

public class CncControllerTests
{
    private readonly Mock<ISerialCommService> _mockSerialCommService;
    private readonly ICNCController _cncController;

    public CncControllerTests()
    {
        _mockSerialCommService = new Mock<ISerialCommService>();
        Mock<IConfigurationService> mockConfigService = new();

        _cncController = new CNCController.Core.Services.CNCControl.CNCController(_mockSerialCommService.Object, mockConfigService.Object);
    }

    [Fact]
    public async Task JogAsync_ShouldSendJogCommand()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        _mockSerialCommService.Setup(s => s.SendCommandAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _cncController.JogAsync("X", 10, cancellationToken);

        // Assert
        _mockSerialCommService.Verify(s => s.SendCommandAsync("G91 G0 X10 F100", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HomeAsync_ShouldSendHomeCommand()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        _mockSerialCommService.Setup(s => s.SendCommandAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _cncController.HomeAsync(cancellationToken);

        // Assert
        _mockSerialCommService.Verify(s => s.SendCommandAsync("G28", cancellationToken), Times.Once);
    }
    
    [Fact]
    public void CNCController_ShouldUpdateStatus_WhenJogging()
    {
        // Arrange
        var mockSerialComm = new Mock<ISerialCommService>();
        var mockConfigService = new Mock<IConfigurationService>();
        var controller = new CNCController.Core.Services.CNCControl.CNCController(mockSerialComm.Object, mockConfigService.Object);
        var stateChanged = false;
    
        controller.StatusUpdated += (_, _) => stateChanged = true;

        // Act
        controller.JogAsync("X", 10, new CancellationToken())?.Wait();

        // Assert
        Assert.True(stateChanged);
    }

}