namespace CNCController.Core.Exceptions
{
    public class CncOperationException : Exception
    {
        public string? OperationDetails { get; }

        public CncOperationException() { }

        public CncOperationException(string message) : base(message) { }

        public CncOperationException(string message, Exception innerException) 
            : base(message, innerException) { }

        public CncOperationException(string message, string operationDetails) 
            : base(message)
        {
            OperationDetails = operationDetails;
        }
        
        public override string ToString() => $"{base.ToString()}, Operation Details: {OperationDetails}";
    }
}