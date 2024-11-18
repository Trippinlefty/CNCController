using System;
using System.Threading.Tasks;
using System.Windows.Input;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

    public void Execute(object? parameter)
    {
        // Call ExecuteAsync internally when Execute is invoked
        ExecuteAsync().FireAndForgetSafeAsync(ex => 
        {
            // Optionally handle exceptions from ExecuteAsync here
            // Log or display the error if necessary
            Console.WriteLine($"Error in AsyncRelayCommand: {ex.Message}");
        });
    }

    public async Task ExecuteAsync()
    {
        if (!CanExecute(null)) return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

// Extension to handle fire-and-forget tasks safely
public static class TaskExtensions
{
    public static async void FireAndForgetSafeAsync(this Task task, Action<Exception>? onException = null)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            onException?.Invoke(ex);
        }
    }
}