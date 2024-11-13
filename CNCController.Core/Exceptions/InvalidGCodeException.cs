namespace CNCController.Core.Exceptions
{
    public class InvalidGCodeException : Exception
    {
        public string? GCodeCommand { get; }

        public InvalidGCodeException() { }

        public InvalidGCodeException(string message) : base(message) { }

        public InvalidGCodeException(string message, Exception innerException) 
            : base(message, innerException) { }

        public InvalidGCodeException(string message, string gCodeCommand) 
            : base(message)
        {
            GCodeCommand = gCodeCommand;
        }
        
        public override string ToString() => $"{base.ToString()}, G-Code Command: {GCodeCommand}";
    }
}