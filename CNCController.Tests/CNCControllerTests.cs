using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using Moq;
using Xunit;

public class CNCControllerTests
{
    private readonly Mock<ISerialCommService> _mockSerialCommService;
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly ICNCController _cncController;

    public CNCControllerTests()
    {
        _mockSerialCommService = new Mock<ISerialCommService>();
        _mockConfigService = new Mock<IConfigurationService>();

        _cncController = new CNCController.Core.Services.CNCControl.CNCController(_mockSerialCommService.Object, _mockConfigService.Object);
    }

    [Fact]
    public async Task JogAsync_ShouldSendJogCommand()
    {
        // Arrange
        _mockSerialCommService.Setup(s => s.SendCommandAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _cncController.JogAsync("X", 10);

        // Assert
        _mockSerialCommService.Verify(s => s.SendCommandAsync("G91 G0 X10 F100", 2000), Times.Once);
    }

    [Fact]
    public async Task HomeAsync_ShouldSendHomeCommand()
    {
        // Arrange
        _mockSerialCommService.Setup(s => s.SendCommandAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _cncController.HomeAsync();

        // Assert
        _mockSerialCommService.Verify(s => s.SendCommandAsync("G28", 2000), Times.Once);
    }
}