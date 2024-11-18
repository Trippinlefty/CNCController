using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests
{
    public class AsyncRelayCommandTests
    {
        [Fact]
        public async Task CanExecute_ReturnsFalse_WhileExecuting()
        {
            var taskCompletionSource = new TaskCompletionSource();
            var command = new AsyncRelayCommand(async () =>
            {
                await taskCompletionSource.Task; // Wait until explicitly signaled
            });

            // Assert CanExecute is initially true
            Assert.True(command.CanExecute(null));

            // Start execution
            var executionTask = command.ExecuteAsync();

            // After starting execution, CanExecute should return false
            Assert.False(command.CanExecute(null));

            // Signal completion of the task
            taskCompletionSource.SetResult();
            await executionTask; // Await completion of the execution

            // After completion, CanExecute should return true again
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public async Task Execute_InvokesAction()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var command = new AsyncRelayCommand(async () =>
            {
                await Task.Delay(1); // Simulate async operation
                taskCompletionSource.SetResult(true); // Signal action completion
            });

            command.Execute(null);

            var actionExecuted = await taskCompletionSource.Task; // Wait for action to complete
            Assert.True(actionExecuted);
        }
        
        [Fact]
        public async Task ExecuteAsync_HandlesExceptions()
        {
            var command = new AsyncRelayCommand(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Test exception");
            });

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => command.ExecuteAsync());
            Assert.True(command.CanExecute(null)); // Ensure CanExecute resets after failure
        }
    }
}