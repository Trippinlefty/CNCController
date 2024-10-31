using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;
using CNCController.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests;

public class SerialCommServiceTests
{
    private readonly Mock<ISerialCommService> _mockSerialCommService = new();


    [Fact]
    public async Task ConnectAsync_ShouldReturnTrue_WhenConnectionIsSuccessful()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        _mockSerialCommService.Setup(s => s.ConnectAsync("COM1", 115200, cancellationToken))
            .ReturnsAsync(true);

        var result = await _mockSerialCommService.Object.ConnectAsync("COM1", 115200, cancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task SendCommandAsync_ShouldInvokeSend_WhenCommandIsValid()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        _mockSerialCommService.Setup(s => s.SendCommandAsync("G28", cancellationToken))
            .ReturnsAsync(true);

        var result = await _mockSerialCommService.Object.SendCommandAsync("G28", cancellationToken);

        _mockSerialCommService.Verify(s => s.SendCommandAsync("G28", cancellationToken), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task ConnectCommand_SetsStatusToFailed_OnConnectionFailure()
    {
        var mockSerialCommService = new Mock<ISerialCommService>();
        mockSerialCommService.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var mockLogger = new Mock<ILogger<CNCViewModel>>();
        var viewModel = new CNCViewModel(null, mockSerialCommService.Object, mockLogger.Object);

        if (viewModel.ConnectCommand is AsyncRelayCommand asyncConnectCommand)
        {
            await asyncConnectCommand.ExecuteAsync();
        }

        Assert.Equal("Failed to connect", viewModel.CurrentStatus.StateMessage);
        Assert.Null(viewModel.ErrorMessage);
    }

    [Fact]
    public async Task ConnectCommand_SetsErrorMessage_OnConnectionException()
    {
        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();
        var mockCncController = new Mock<ICNCController>();
        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object);
        
        mockSerialCommService.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection error"));

        if (viewModel.ConnectCommand is AsyncRelayCommand asyncConnectCommand)
        {
            await asyncConnectCommand.ExecuteAsync();
        }

        Assert.Equal("Failed to connect", viewModel.CurrentStatus.StateMessage);
        Assert.Equal("Connection error", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task DisconnectCommand_SetsErrorMessage_OnDisconnectionException()
    {
        var mockSerialCommService = new Mock<ISerialCommService>();
        mockSerialCommService.Setup(s => s.DisconnectAsync())
            .ThrowsAsync(new Exception("Disconnection error"));

        var mockLogger = new Mock<ILogger<CNCViewModel>>();
        var viewModel = new CNCViewModel(null, mockSerialCommService.Object, mockLogger.Object);

        if (viewModel.DisconnectCommand is AsyncRelayCommand asyncDisconnectCommand)
        {
            await asyncDisconnectCommand.ExecuteAsync();
        }

        Assert.Equal("Disconnection error", viewModel.ErrorMessage);
    }
}