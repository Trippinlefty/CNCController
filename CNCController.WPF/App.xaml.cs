using System;
using System.Threading.Tasks;
using System.Windows;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using Microsoft.Extensions.DependencyInjection;

namespace CNCController;

public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global error handling
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            MessageBox.Show($"Unexpected error: {exception?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        // Configure and build services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Initialize services asynchronously
        await InitializeServicesAsync();

        // Start the main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register services with appropriate lifetimes
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ISerialCommService, SerialCommService>();
        services.AddSingleton<ICNCController, CNCController.Core.Services.CNCControl.CNCController>();
        
        // MainWindow registration
        services.AddSingleton<MainWindow>();
        
        // Example of scoped or transient services (if needed in the future)
        // services.AddScoped<IMachineStatusService, MachineStatusService>();
        // services.AddTransient<IGCodeParser, GCodeParser>();
    }
    
    private async Task InitializeServicesAsync()
    {
        try
        {
            // Resolve and load configuration
            var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
            var config = await configService.LoadConfigAsync();

            // Resolve and configure Serial Communication Service
            var serialCommService = _serviceProvider.GetRequiredService<ISerialCommService>();
            await serialCommService.ConnectAsync(config.PortName, config.BaudRate);

            // CNC Controller is managed automatically by DI
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();  // Shut down if initialization fails
        }
    }
}