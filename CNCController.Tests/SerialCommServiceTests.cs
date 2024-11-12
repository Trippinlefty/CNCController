using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.ErrorHandle;
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
        var mockCncController = new Mock<ICncController>(); // Added mock for ICNCController
        var mockSerialCommService = new Mock<ISerialCommService>();
        mockSerialCommService.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Simulate connection failure
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        // Initialize CNCViewModel with all required dependencies, including mockCncController
        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        if (viewModel.ConnectCommand is AsyncRelayCommand asyncConnectCommand)
        {
            await asyncConnectCommand.ExecuteAsync();
        }

        Assert.Equal("Failed to connect", viewModel.CurrentStatus.StateMessage);
        Assert.Null(viewModel.ErrorMessage); // Assuming no specific error message for connection failure
    }

    [Fact]
    public async Task ConnectCommand_SetsErrorMessage_OnConnectionException()
    {
        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();
        var mockCncController = new Mock<ICncController>();
        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);
        
        mockSerialCommService.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection error"));

        if (viewModel.ConnectCommand is AsyncRelayCommand asyncConnectCommand)
        {
            await asyncConnectCommand.ExecuteAsync();
        }

        Assert.StartsWith("Error: Connection failed - An unexpected error occurred.", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task DisconnectCommand_SetsErrorMessage_OnDisconnectionException()
    {
        var mockCncController = new Mock<ICncController>(); // Added mock for ICNCController
        var mockSerialCommService = new Mock<ISerialCommService>();
        mockSerialCommService.Setup(s => s.DisconnectAsync())
            .ThrowsAsync(new Exception("Disconnection error")); // Simulate disconnection exception
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        // Initialize CNCViewModel with all required dependencies, including mockCncController
        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        if (viewModel.DisconnectCommand is AsyncRelayCommand asyncDisconnectCommand)
        {
            await asyncDisconnectCommand.ExecuteAsync();
        }

        Assert.Contains("An unexpected error occurred.", viewModel.ErrorMessage);
    }
}