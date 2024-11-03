using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.SerialCommunication;
using CNCController.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CNCController
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private CancellationTokenSource _cancellationTokenSource;

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

            // Initialize the CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();

            // Initialize services asynchronously
            await InitializeServicesAsync(_cancellationTokenSource.Token);
            

            // Start the main window
            var viewModel = _serviceProvider.GetRequiredService<CNCViewModel>();
            viewModel.RefreshAvailablePorts();
            var mainWindow = new MainWindow(viewModel);
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IConfigurationService, ConfigurationService>(); // Add this line
            services.AddSingleton<ISerialCommService, SerialCommService>();
            services.AddSingleton<ICNCController, CNCController.Core.Services.CNCControl.CNCController>();
            services.AddSingleton<CNCViewModel>();
            services.AddSingleton<MainWindow>();
        }
        
        private async Task InitializeServicesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Resolve and load configuration
                var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
                var config = await configService.LoadConfigAsync();

                // Resolve and configure Serial Communication Service
                var serialCommService = _serviceProvider.GetRequiredService<ISerialCommService>();
                //await serialCommService.ConnectAsync(config.PortName, config.BaudRate, cancellationToken);

                // CNC Controller is managed by DI
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Initialization canceled.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
