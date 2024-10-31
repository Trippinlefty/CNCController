using CNCController.Core.Services.RelayCommand;
using CNCController.Core.Services.SerialCommunication;
using CNCController.ViewModels;
using Microsoft.Extensions.Logging;
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

        var mockLogger = new Mock<ILogger<CNCViewModel>>();

        var viewModel = new CNCViewModel(null, mockSerialCommService.Object, mockLogger.Object);

        ((AsyncRelayCommand)viewModel.ConnectCommand).Execute(null);

        await Task.Delay(500); // Wait for async execution if needed
        Assert.Equal("Connected", viewModel.CurrentStatus.StateMessage);
    }

}