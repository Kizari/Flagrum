using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Application;
using Flagrum.Application;
using Flagrum.Application.Services;
using Flagrum.Components;
using Flagrum.Host.Shell;
using Flagrum.Host.WebView;
using Flagrum.Migrations;
using MsBox.Avalonia.Enums;
using Serilog;
using Velopack;
using Velopack.Sources;

namespace Flagrum.Host;

/// <summary>
/// Main Avalonia application for Flagrum.
/// </summary>
public partial class AvaloniaApplication(
    IServiceProvider services,
    IApplication application,
    ObservedTaskScheduler scheduler,
    MigrationRunner migrations,
    IPlatformManager platform,
    AppStateService appState) : Avalonia.Application
{
    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Creates and shows the main window when Avalonia is ready.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += OnShutdown;
            ((ApplicationHost)application).AvaloniaApplication = desktop;
            
            // Show the splash screen
            var splash = new SplashWindow();
            desktop.MainWindow = splash;
            splash.Show();

            // Run startup code asynchronously so the splash screen can update during process
            application.AssociatedFile = desktop.Args?.Length == 1
                                         && desktop.Args[0].EndsWith(".fmod", StringComparison.OrdinalIgnoreCase)
                ? desktop.Args[0]
                : null;
            
            scheduler.RunAsyncObserved(StartAsync);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Sets the splash screen caption.
    /// </summary>
    private void SetSplashText(string text)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime {MainWindow: SplashWindow splash})
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => splash.SetText(text));
        }
    }

    /// <summary>
    /// Startup sequence to run during the splash screen, launches the main window when completed.
    /// </summary>
    private async Task StartAsync()
    {
        var start = DateTime.UtcNow;
        
        // Check for updates
        if (await TryUpdateAsync())
        {
            await Log.CloseAndFlushAsync();
            return; // Application is restarting, finish here
        }
        
        // Run all data migrations
        await migrations.RunMigrationsAsync();
        
        // Check the application version
        if (!platform.IsVersionSupported)
        {
            await MessageBox.ShowAsync("Error", 
                "This version of Flagrum is no longer supported.", Icon.Error);

            Shutdown();
            return;
        }
        
        // Start initializing the asset explorer
        appState.LoadNodes();
        
        // Ensure the splash screen is displayed no less than three seconds
        var elapsed = DateTime.UtcNow - start;
        var remaining = TimeSpan.FromSeconds(3) - elapsed;
        if (remaining.TotalMilliseconds > 0)
        {
            await Task.Delay(remaining);
        }
        
        // Launch the main window
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var desktop = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
            var splash = desktop.MainWindow!;
            var mainWindow = new MainWindow(services);
            desktop.MainWindow = mainWindow;
            desktop.MainWindow.Show();
            var webView = (BlazorWebView)mainWindow.MainGrid.Children[1]!;
            await webView.SetRootComponentAsync<App>("#app");
            webView.Navigate(BlazorWebViewManager.CreateUri("/"));
            splash.Close();
        });
    }

    /// <summary>
    /// Gracefully closes Flagrum.
    /// </summary>
    private void Shutdown()
    {
        Avalonia.Threading.Dispatcher.UIThread
            .Invoke(() => ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown());
    }

    /// <summary>
    /// Cleanup to run when the application is closed.
    /// </summary>
    private void OnShutdown(object? sender, ShutdownRequestedEventArgs? args)
    {
        Log.CloseAndFlush();
    }
    
    /// <summary>
    /// Attempts to update the application.
    /// </summary>
    /// <returns><c>true</c> if an update was applied, otherwise <c>false</c>.</returns>
    private async Task<bool> TryUpdateAsync()
    {
        try
        {
            SetSplashText("Checking for updates");

            // Check for updates
            var github = new GithubSource("https://github.com/Kizari/Flagrum", null, false);
            var manager = new UpdateManager(github);
            var newVersion = await manager.CheckForUpdatesAsync();

            // Download and apply updates if any were available
            if (newVersion != null)
            {
                SetSplashText("Downloading updates");
                await manager.DownloadUpdatesAsync(newVersion);
                SetSplashText("Updating Flagrum");
                manager.ApplyUpdatesAndRestart();
                return true;
            }
        }
        catch
        {
            // Not much to be done if this fails, most likely due to no internet or user blocking the update URL
            // Let Flagrum continue as normal
        }

        SetSplashText("Loading");
        return false;
    }
}