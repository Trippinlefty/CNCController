using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests
{
    public class RelayCommandTests
    {
        [Fact]
        public void CanExecute_ReturnsTrue_WhenPredicateIsNull()
        {
            var command = new RelayCommand(() => { });
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void Execute_InvokesAction()
        {
            bool actionExecuted = false;
            var command = new RelayCommand(() => actionExecuted = true);

            command.Execute(null);

            Assert.True(actionExecuted);
        }

        [Fact]
        public void CanExecute_CanBeControlledExternally()
        {
            bool canExecute = false;
            var command = new RelayCommand(() => { }, () => canExecute);

            Assert.False(command.CanExecute(null));

            canExecute = true;
            Assert.True(command.CanExecute(null));
        }
    }
}