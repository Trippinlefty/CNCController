using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;
using CNCController.ViewModels;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Assert = Xunit.Assert;

namespace CNCController.Tests;

[TestFixture]
public class CncViewModelTests
{
    [Test]
    public async Task ConnectCommand_SetsStatusToConnected_OnSuccess()
    {
        var mockSerialCommService = new Mock<ISerialCommService>();
        mockSerialCommService.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(null, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object);

        ((AsyncRelayCommand)viewModel.ConnectCommand).Execute(null);

        await Task.Delay(500); // Wait for async execution if needed
        Assert.Equal("Connected", viewModel.CurrentStatus.StateMessage);
    }
    [Fact]
    public void JogCommand_UpdatesStatusToJogging_OnSuccess()
    {
        // Arrange
        var mockCncController = new Mock<ICNCController>();
        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object);

        // Act
        viewModel.JogCommand.Execute(null);

        // Assert
        mockCncController.Verify(c => c.JogAsync("X", 10, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Jogging...", viewModel.CurrentStatus.StateMessage);
    }
    
    [Fact]
    public void JogCommand_SetsErrorMessage_OnJoggingException()
    {
        // Arrange
        var mockCncController = new Mock<ICNCController>();
        mockCncController.Setup(c => c.JogAsync("X", 10, It.IsAny<CancellationToken>()))
            .Throws(new Exception("Jogging error"));

        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object);

        // Act
        viewModel.JogCommand.Execute(null);

        // Assert
        Assert.Equal("Error: Jogging error", viewModel.ErrorMessage);
    }

    [Fact]
    public void HomeCommand_UpdatesStatusToHoming_OnSuccess()
    {
        // Arrange
        var mockCncController = new Mock<ICNCController>();
        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object );

        // Act
        viewModel.HomeCommand.Execute(null);

        // Assert
        mockCncController.Verify(c => c.HomeAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Homing...", viewModel.CurrentStatus.StateMessage);
    }

    [Fact]
    public void HomeCommand_SetsErrorMessage_OnHomingException()
    {
        // Arrange
        var mockCncController = new Mock<ICNCController>();
        mockCncController.Setup(c => c.HomeAsync(It.IsAny<CancellationToken>()))
            .Throws(new Exception("Homing error"));

        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object);

        // Act
        viewModel.HomeCommand.Execute(null);

        // Assert
        Assert.Equal("Error: Homing error", viewModel.ErrorMessage);
    }

}