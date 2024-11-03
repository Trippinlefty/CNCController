namespace CNCController.Core.Services.ErrorHandle;

public interface IErrorHandler
{
    void HandleException(Exception ex);
}