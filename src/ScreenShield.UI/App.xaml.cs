using Microsoft.Extensions.DependencyInjection;
using ScreenShield.Core.Interfaces;
using ScreenShield.Infrastructure.Services;
using ScreenShield.UI.ViewModels;
using ScreenShield.UI.Views;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ScreenShield.UI.Services;

using ScreenShield.Core.Services;

namespace ScreenShield.UI;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "ScreenShield-.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Setup Global Exception Handling
        SetupExceptionHandling();

        // Configure Services
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Show Main Window
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        // mainWindow.Show(); // We don't show it, the TaskbarIcon is the main entry.
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IInputService, WindowsInputService>();
        services.AddSingleton<IMonitorService, WindowsMonitorService>();
        services.AddSingleton<IWindowService, WpfWindowService>();
        services.AddSingleton<IdleDetector>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Views (as transient services)
        services.AddTransient<MainWindow>();
        services.AddTransient<OverlayWindow>();
    }

    private void SetupExceptionHandling()
    {
        // UI Thread Exceptions
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Task Scheduler Exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // AppDomain-wide Exceptions
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception on UI thread.");
        // Mark as handled to prevent app from crashing
        e.Handled = true;
        // Optionally, show a friendly error message to the user
        MessageBox.Show("An unexpected error occurred. Please check the logs for details.", "ScreenShield Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception.");
        e.SetObserved(); // Prevent the process from terminating
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.ExceptionObject as Exception, "Fatal unhandled exception.");
        // This is a crashing exception, so we can't prevent termination.
        // We can just log it and maybe show a final message if possible.
        MessageBox.Show("A fatal error occurred and the application will now close. Please check the logs.", "ScreenShield Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}