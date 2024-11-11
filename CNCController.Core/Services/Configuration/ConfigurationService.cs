using System.Text.Json;
using CNCController.Core.Exceptions;
using CNCController.Core.Models;
using CNCController.Core.Services.ErrorHandle;
using Microsoft.Extensions.Logging;

namespace CNCController.Core.Services.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly IErrorHandler _errorHandler;
    private readonly string _configFilePath;

    // Default config values
    private readonly AppConfig _defaultConfig = new()
    {
        PortName = "COM1",
        BaudRate = 115200,
        PollingInterval = 1000,
        MachineSettings = new Dictionary<string, string>
        {
            { "StepsPerMM", "200" },
            { "MaxSpeed", "5000" }
        }
    };

    // Constructor with customizable config file path for testing
    public ConfigurationService(
        ILogger<ConfigurationService> logger, 
        IErrorHandler globalErrorHandler, 
        string configFilePath = "config.json")  // Default file path
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorHandler = globalErrorHandler ?? throw new ArgumentNullException(nameof(globalErrorHandler));
        _configFilePath = configFilePath;
    }

    public async Task<AppConfig> LoadConfigAsync()
    {
        try
        {
            // If file doesn't exist, save and return default configuration
            if (!File.Exists(_configFilePath))
            {
                await SaveConfigAsync(_defaultConfig);
                return _defaultConfig;
            }

            // Read and deserialize config file
            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);

            return config ?? _defaultConfig;
        }
        catch (FileNotFoundException ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Configuration file not found.");
            return _defaultConfig;
        }
        catch (JsonException ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Configuration file is corrupted.");
            throw new ConfigurationException("Configuration file is invalid.", ex);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Unexpected error occurred while loading configuration.");
            throw;
        }
    }

    public async Task<bool> SaveConfigAsync(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configFilePath, json);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Access denied while saving configuration.");
            return false;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, "Unexpected error occurred while saving configuration.");
            return false;
        }
    }

    public async Task<bool> UpdateConfigAsync(string key, string value)
    {
        try
        {
            var config = await LoadConfigAsync();

            switch (key)
            {
                case "PortName":
                    config.PortName = value;
                    break;
                case "BaudRate":
                    if (int.TryParse(value, out var baudRate) && baudRate > 0)
                        config.BaudRate = baudRate;
                    else
                        throw new ArgumentException("Invalid baud rate.");
                    break;
                case "PollingInterval":
                    if (int.TryParse(value, out var interval) && interval > 0)
                        config.PollingInterval = interval;
                    else
                        throw new ArgumentException("Invalid polling interval.");
                    break;
                default:
                    config.MachineSettings[key] = value;
                    break;
            }

            return await SaveConfigAsync(config);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex);
            _logger.LogError(ex, $"Error updating configuration key '{key}' with value '{value}'.");
            return false;
        }
    }
    
    private async Task<AppConfig> LoadDefaultConfig()
    {
        if (!File.Exists(_configFilePath))
        {
            _logger.LogWarning("Config file missing; saving and loading defaults.");
            await SaveConfigAsync(_defaultConfig);
        }
        return await LoadConfigAsync();
    }

    public async Task<bool> ResetToDefaultsAsync()
    {
        return await SaveConfigAsync(_defaultConfig);
    }
}