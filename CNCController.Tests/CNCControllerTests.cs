using System;
using System.Threading;
using System.Threading.Tasks;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.ErrorHandle;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

public class CNCControllerTests
{
    private readonly Mock<ISerialCommService> _serialCommServiceMock;
    private readonly Mock<IConfigurationService> _configurationServiceMock;
    private readonly Mock<ILogger<CncController>> _loggerMock;
    private readonly Mock<IErrorHandler> _errorHandlerMock;
    private readonly CncController _cncController;

    public CNCControllerTests()
    {
        _serialCommServiceMock = new Mock<ISerialCommService>();
        _configurationServiceMock = new Mock<IConfigurationService>();
        _loggerMock = new Mock<ILogger<CncController>>();
        _errorHandlerMock = new Mock<IErrorHandler>();

        _cncController = new CncController(
            _serialCommServiceMock.Object,
            _configurationServiceMock.Object,
            _loggerMock.Object,
            _errorHandlerMock.Object
        );
    }

    [Fact]
    public async Task JogAsync_SendsCorrectCommand()
    {
        // Arrange
        var direction = "X";
        var distance = 10.0;
        var expectedCommand = "G91 G0 X10.00 F100";

        _serialCommServiceMock
            .Setup(x => x.SendCommandAsync(expectedCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _cncController.JogAsync(direction, distance, CancellationToken.None);

        // Assert
        _serialCommServiceMock.Verify(
            x => x.SendCommandAsync(expectedCommand, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HomeAsync_SendsHomeCommand()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _serialCommServiceMock.Setup(x => x.SendCommandAsync(It.IsAny<string>(), cancellationToken))
                              .ReturnsAsync(true);

        // Act
        await _cncController.HomeAsync(cancellationToken);

        // Assert
        _serialCommServiceMock.Verify(x => x.SendCommandAsync("G28", cancellationToken), Times.Once);
    }

    [Fact]
    public async Task EmergencyStopAsync_SendsEmergencyStopCommand()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _serialCommServiceMock.Setup(x => x.SendCommandAsync(It.IsAny<string>(), cancellationToken))
                              .ReturnsAsync(true);

        // Act
        await _cncController.EmergencyStopAsync(cancellationToken);

        // Assert
        _serialCommServiceMock.Verify(x => x.SendCommandAsync("M112", cancellationToken), Times.Once);
    }

    [Fact]
    public void OnDataReceived_UpdatesStatusToAlarm()
    {
        // Arrange
        var statusUpdatedEventTriggered = false;
        _cncController.StatusUpdated += (_, _) => statusUpdatedEventTriggered = true;

        // Act
        _serialCommServiceMock.Raise(x => x.DataReceived += null, this, "ALARM");

        // Assert
        Assert.True(statusUpdatedEventTriggered);
    }
}
