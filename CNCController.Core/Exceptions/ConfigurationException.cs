namespace CNCController.Core.Exceptions
{
    public class ConfigurationException : Exception
    {
        public string? ConfigKey { get; }

        public ConfigurationException() { }

        public ConfigurationException(string message) : base(message) { }

        public ConfigurationException(string message, Exception innerException) 
            : base(message, innerException) { }

        public ConfigurationException(string message, string configKey) 
            : base(message)
        {
            ConfigKey = configKey;
        }
        
        public override string ToString() => $"{base.ToString()}, Config Key: {ConfigKey}";
    }
}