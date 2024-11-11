namespace CNCController.Core.Exceptions;

public class CncOperationException : Exception
{
    public CncOperationException() { }

    public CncOperationException(string message) : base(message) { }

    public CncOperationException(string message, Exception innerException) 
        : base(message, innerException) { }
}