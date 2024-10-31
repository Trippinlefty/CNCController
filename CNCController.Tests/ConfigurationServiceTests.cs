using CNCController.Core.Services.Configuration;
using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests;

public class ConfigurationServiceTests
{
    private readonly IConfigurationService _configService;

    public ConfigurationServiceTests()
    {
        // Arrange
        _configService = new ConfigurationService();
    }

    [Fact]
    public async Task LoadConfigAsync_ShouldLoadDefaultConfig_WhenFileDoesNotExist()
    {
        // Arrange
        if (File.Exists("config.json"))
        {
            File.Delete("config.json");  // Ensure no config file exists
        }

        // Act
        var config = await _configService.LoadConfigAsync();

        // Assert
        Assert.Equal("COM1", config.PortName);
        Assert.Equal(115200, config.BaudRate);
        Assert.Equal(1000, config.PollingInterval);
    }

    [Fact]
    public async Task SaveConfigAsync_ShouldSaveConfigToFile()
    {
        // Arrange
        var newConfig = new AppConfig
        {
            PortName = "COM3",
            BaudRate = 9600,
            PollingInterval = 500
        };

        // Act
        var result = await _configService.SaveConfigAsync(newConfig);

        // Assert
        Assert.True(result);

        var loadedConfig = await _configService.LoadConfigAsync();
        Assert.Equal("COM3", loadedConfig.PortName);
        Assert.Equal(9600, loadedConfig.BaudRate);
        Assert.Equal(500, loadedConfig.PollingInterval);
    }

    [Fact]
    public async Task UpdateConfigAsync_ShouldUpdateSpecificSetting()
    {
        // Arrange
        await _configService.UpdateConfigAsync("PortName", "COM4");

        // Act
        var config = await _configService.LoadConfigAsync();

        // Assert
        Assert.Equal("COM4", config.PortName);
    }
}