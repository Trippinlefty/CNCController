using CNCController.Core.Services.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CNCController.Core.Exceptions;
using CNCController.Core.Models;
using CNCController.Core.Services.ErrorHandle;
using Assert = Xunit.Assert;

namespace CNCController.Tests
{
    public class ConfigurationServiceTests : IDisposable
    {
        private readonly Mock<ILogger<ConfigurationService>> _mockLogger;
        private readonly Mock<IErrorHandler> _mockErrorHandler;
        private readonly string _tempConfigPath;
        private readonly IConfigurationService _configService;

        public ConfigurationServiceTests()
        {
            // Create a unique path for each test instance to avoid conflicts
            _tempConfigPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + "_test_config.json");

            _mockLogger = new Mock<ILogger<ConfigurationService>>();
            _mockErrorHandler = new Mock<IErrorHandler>();

            // Inject dependencies with the temporary config path
            _configService = new ConfigurationService(_mockLogger.Object, _mockErrorHandler.Object, _tempConfigPath);
        }

        [Fact]
        public async Task LoadConfigAsync_LoadsDefaultConfig_WhenFileNotFound()
        {
            // Ensure the file does not exist
            if (File.Exists(_tempConfigPath))
                File.Delete(_tempConfigPath);

            var config = await _configService.LoadConfigAsync();

            // Verify the loaded config matches the expected defaults
            Assert.Equal("COM1", config.PortName);
            Assert.Equal(115200, config.BaudRate);
            Assert.Equal(1000, config.PollingInterval);
        }

        [Fact]
        public async Task LoadConfigAsync_HandlesJsonException()
        {
            // Create an invalid JSON file to simulate a corrupt config file
            await File.WriteAllTextAsync(_tempConfigPath, "{invalid json}");

            // Expect an exception due to invalid JSON
            await Assert.ThrowsAsync<ConfigurationException>(() => _configService.LoadConfigAsync());
        }

        [Fact]
        public async Task SaveConfigAsync_SavesConfigSuccessfully()
        {
            var config = new AppConfig
            {
                PortName = "COM3",
                BaudRate = 9600,
                PollingInterval = 500
            };

            // Attempt to save and reload the configuration
            bool result = await _configService.SaveConfigAsync(config);
            Assert.True(result);

            var loadedConfig = await _configService.LoadConfigAsync();
            Assert.Equal("COM3", loadedConfig.PortName);
            Assert.Equal(9600, loadedConfig.BaudRate);
            Assert.Equal(500, loadedConfig.PollingInterval);
        }

        // Clean up the temporary file after each test
        public void Dispose()
        {
            if (File.Exists(_tempConfigPath))
            {
                File.Delete(_tempConfigPath);
            }
        }
    }
}
