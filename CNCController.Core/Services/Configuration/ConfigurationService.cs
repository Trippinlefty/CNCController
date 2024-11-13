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

    public ConfigurationService(
        ILogger<ConfigurationService> logger, 
        IErrorHandler errorHandler, 
        string configFilePath = "config.json")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _configFilePath = configFilePath;
    }

    public async Task<AppConfig> LoadConfigAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
                return await LoadDefaultConfig();

            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);
            return config ?? _defaultConfig;
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is JsonException)
        {
            HandleConfigError(ex, "Failed to load configuration. Using defaults.");
            return _defaultConfig;
        }
    }

    public async Task<bool> SaveConfigAsync(AppConfig config)
    {
        return await HandleFileOperationAsync(async () =>
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configFilePath, json);
            return true;
        }, "Error saving configuration.");
    }

    public async Task<bool> UpdateConfigAsync(string key, string value)
    {
        try
        {
            var config = await LoadConfigAsync();

            if (key == nameof(config.PortName))
                config.PortName = value;
            else if (key == nameof(config.BaudRate) && int.TryParse(value, out var baudRate) && baudRate > 0)
                config.BaudRate = baudRate;
            else if (key == nameof(config.PollingInterval) && int.TryParse(value, out var interval) && interval > 0)
                config.PollingInterval = interval;
            else
                config.MachineSettings[key] = value;

            return await SaveConfigAsync(config);
        }
        catch (Exception ex)
        {
            HandleConfigError(ex, $"Error updating config key '{key}' with value '{value}'.");
            return false;
        }
    }

    public async Task<bool> ResetToDefaultsAsync()
    {
        return await SaveConfigAsync(_defaultConfig);
    }

    private async Task<AppConfig> LoadDefaultConfig()
    {
        _logger.LogWarning("Config file missing; saving and loading defaults.");
        await SaveConfigAsync(_defaultConfig);
        return _defaultConfig;
    }

    private async Task<bool> HandleFileOperationAsync(Func<Task<bool>> operation, string errorMessage)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            HandleConfigError(ex, errorMessage);
            return false;
        }
    }

    private void HandleConfigError(Exception ex, string message)
    {
        _errorHandler.HandleException(ex, message);
        _logger.LogError(ex, message);
    }
}
