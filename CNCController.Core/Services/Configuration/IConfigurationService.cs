using CNCController.Core.Models;

namespace CNCController.Core.Services.Configuration
{
    public interface IConfigurationService
    {
        Task<AppConfig> LoadConfigAsync();
        Task<bool> SaveConfigAsync(AppConfig config);
        Task<bool> UpdateConfigAsync(string key, string value);
        Task<bool> ResetToDefaultsAsync();  // Reset to default settings
    }
}