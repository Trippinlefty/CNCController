using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CNCController.Core.Services.CNCControl;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.ErrorHandle;
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
            // Global exception handling for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                var errorHandler = _serviceProvider.GetService<IErrorHandler>();
                errorHandler?.HandleException(exception, null);
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
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());

            // Register IErrorHandler to use GlobalErrorHandler
            services.AddSingleton<IErrorHandler, GlobalErrorHandler>();

            // Register other services
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ISerialCommService, SerialCommService>();
            services.AddSingleton<ICncController, CncController>();

            // Register ViewModels
            services.AddSingleton<CNCViewModel>();
            services.AddSingleton<SettingsViewModel>(); // Add SetupWizardViewModel here
            services.AddSingleton<StatusViewModel>();

            // Register SetupWizard and MainWindow with DI
            services.AddTransient<SetupWizard>();
            services.AddTransient<MainWindow>(provider => 
            {
                var viewModel = provider.GetRequiredService<CNCViewModel>();
                return new MainWindow(viewModel);
            });
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
                // Uncomment and configure as needed
                // await serialCommService.ConnectAsync(config.PortName, config.BaudRate, cancellationToken);

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

        protected override void OnExit(ExitEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
