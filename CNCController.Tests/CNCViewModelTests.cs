using System.Threading;
using System.Threading.Tasks;
using CNCController.Core.Models;
using CNCController.Core.Services.CNCControl;
using CNCController.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

public class CNCViewModelTests
{
    private readonly Mock<ICNCController> _cncControllerMock;
    private readonly Mock<ILogger<CNCViewModel>> _loggerMock;
    private readonly CNCViewModel _viewModel;

    public CNCViewModelTests()
    {
        _cncControllerMock = new Mock<ICNCController>();
        _loggerMock = new Mock<ILogger<CNCViewModel>>();

        _viewModel = new CNCViewModel(_cncControllerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task JogCommand_ExecutesJogAsync()
    {
        // Arrange
        _cncControllerMock.Setup(x => x.JogAsync(It.IsAny<string>(), It.IsAny<double>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await ((AsyncRelayCommand)_viewModel.JogCommand).ExecuteAsync();

        // Assert
        _cncControllerMock.Verify(x => x.JogAsync("X", 10, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HomeCommand_ExecutesHomeAsync()
    {
        // Arrange
        _cncControllerMock.Setup(x => x.HomeAsync(CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await ((AsyncRelayCommand)_viewModel.HomeCommand).ExecuteAsync();

        // Assert
        _cncControllerMock.Verify(x => x.HomeAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void OnStatusUpdated_UpdatesStatusMessage()
    {
        // Arrange
        var status = new CncStatus { StateMessage = "Running" };

        // Act
        _cncControllerMock.Raise(x => x.StatusUpdated += null, null, status);

        // Assert
        Assert.Equal("Running", _viewModel.StatusMessage);
    }
}