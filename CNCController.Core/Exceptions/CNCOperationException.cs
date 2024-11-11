namespace CNCController.Core.Exceptions
{
    public class CNCOperationException : Exception
    {
        public CNCOperationException() { }

        public CNCOperationException(string message) : base(message) { }

        public CNCOperationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}