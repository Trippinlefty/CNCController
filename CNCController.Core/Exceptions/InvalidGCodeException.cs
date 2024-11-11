namespace CNCController.Core.Exceptions
{
    public class InvalidGCodeException : Exception
    {
        public InvalidGCodeException() { }

        public InvalidGCodeException(string message) : base(message) { }

        public InvalidGCodeException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}