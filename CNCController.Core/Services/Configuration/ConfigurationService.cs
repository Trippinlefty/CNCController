using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CNCController.Core.Services.Configuration;

public class ConfigurationService : IConfigurationService
{
    private const string ConfigFilePath = "config.json";
    private readonly AppConfig _defaultConfig;

    public ConfigurationService()
    {
        // Define default configuration values
        _defaultConfig = new AppConfig
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
    }

    public async Task<AppConfig> LoadConfigAsync()
    {
        try
        {
            if (!File.Exists(ConfigFilePath))
            {
                // If no config file, save the default config
                await SaveConfigAsync(_defaultConfig);
                return _defaultConfig;
            }

            var json = await File.ReadAllTextAsync(ConfigFilePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);
            
            return config ?? _defaultConfig;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return _defaultConfig;
        }
    }

    public async Task<bool> SaveConfigAsync(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ConfigFilePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
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
                    if (int.TryParse(value, out int baudRate) && baudRate > 0)
                        config.BaudRate = baudRate;
                    else
                        throw new ArgumentException("Invalid baud rate.");
                    break;
                case "PollingInterval":
                    if (int.TryParse(value, out int interval) && interval > 0)
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
            Console.WriteLine($"Error updating configuration: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ResetToDefaultsAsync()
    {
        return await SaveConfigAsync(_defaultConfig);
    }
}
