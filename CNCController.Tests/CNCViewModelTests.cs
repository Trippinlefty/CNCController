using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.ErrorHandle;
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
    [Fact]
    public async Task ConnectCommand_SetsStatusToConnected_OnSuccess()
    {
        var mockCncController = new Mock<ICncController>(); // Added mock for ICNCController
        var mockSerialCommService = new Mock<ISerialCommService>();
        mockSerialCommService.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        ((AsyncRelayCommand)viewModel.ConnectCommand).Execute(null);

        await Task.Delay(500); // Wait for async execution if needed
        Assert.Equal("Connected", viewModel.CurrentStatus.StateMessage);
    }

    [Fact]
    public void JogCommand_UpdatesStatusToJogging_OnSuccess()
    {
        var mockCncController = new Mock<ICncController>();
        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        viewModel.JogCommand.Execute(null);

        mockCncController.Verify(c => c.JogAsync("X", 10, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Jogging...", viewModel.CurrentStatus.StateMessage);
    }
    
    [Fact]
    public void JogCommand_SetsErrorMessage_OnJoggingException()
    {
        var mockCncController = new Mock<ICncController>();
        mockCncController.Setup(c => c.JogAsync("X", 10, It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("Jogging error"));

        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        viewModel.JogCommand.Execute(null);

        // Assert that error message starts with "Error:" and includes specific content
        Assert.StartsWith("Error:", viewModel.ErrorMessage);
        Assert.Contains("Invalid operation attempted", viewModel.ErrorMessage);
    }

    [Fact]
    public void HomeCommand_UpdatesStatusToHoming_OnSuccess()
    {
        var mockCncController = new Mock<ICncController>();
        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        viewModel.HomeCommand.Execute(null);

        mockCncController.Verify(c => c.HomeAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Homing...", viewModel.CurrentStatus.StateMessage);
    }

    [Fact]
    public void HomeCommand_SetsErrorMessage_OnHomingException()
    {
        var mockCncController = new Mock<ICncController>();
        mockCncController.Setup(c => c.HomeAsync(It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("Homing error"));

        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        viewModel.HomeCommand.Execute(null);

        // Expect the error message to contain "Invalid operation attempted" as specified in HandleError
        Assert.Contains("Invalid operation attempted", viewModel.ErrorMessage);
    }

    [Test]
    public async Task StartCommand_SetsStatusToRunning_OnSuccess()
    {
        var mockCncController = new Mock<ICncController>();
        mockCncController.Setup(c => c.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        // Cast to AsyncRelayCommand to call ExecuteAsync()
        if (viewModel.StartCommand is AsyncRelayCommand startCommand)
        {
            await startCommand.ExecuteAsync();
        }

        Assert.Equal("Running", viewModel.CurrentStatus.StateMessage);
    }

    [Fact]
    public async Task StartCommand_SetsErrorMessage_OnStartException()
    {
        var mockCncController = new Mock<ICncController>();
        mockCncController.Setup(c => c.StartAsync(It.IsAny<CancellationToken>()))
            .Throws(new Exception("Start error"));

        var mockSerialCommService = new Mock<ISerialCommService>();
        var mockConfigurationService = new Mock<IConfigurationService>();
        var mockErrorHandler = new Mock<IErrorHandler>();
        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(mockCncController.Object, mockSerialCommService.Object, mockLogger.Object, mockConfigurationService.Object, mockErrorHandler.Object);

        if (viewModel.StartCommand is AsyncRelayCommand startCommand)
        {
            await startCommand.ExecuteAsync();
        }

        // Check that the message contains specific keywords for error
        Assert.Contains("Failed to start the CNC", viewModel.ErrorMessage);
        Assert.Contains("An unexpected error occurred", viewModel.ErrorMessage);
    }
}
