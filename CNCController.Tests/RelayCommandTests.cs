using CNCController.Core.Services.RelayCommand;
using Assert = Xunit.Assert;

namespace CNCController.Tests;

[TestFixture]
public class RelayCommandTests
{
    [Test]
    public void CanExecute_ReturnsTrue_WhenPredicateIsNull()
    {
        var command = new RelayCommand(_ => { });
        Assert.True(command.CanExecute(null));
    }

    [Test]
    public void Execute_InvokesAction()
    {
        var actionExecuted = false;
        var command = new RelayCommand(_ => actionExecuted = true);

        command.Execute(null);

        Assert.True(actionExecuted);
    }

}