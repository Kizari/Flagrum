using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager;
using Flagrum.Application;
using Flagrum.Application.Services;
using Flagrum.ApplicationHost.Native;
using Flagrum.ApplicationHost.WebView;
using Flagrum.Migrations;
using Flagrum.Platform.Abstractions;
using Flagrum.Services;
using Flagrum.Utilities;
using Injectio.Attributes;
using Serilog;

namespace Flagrum.ApplicationHost;

/// <summary>
/// The Flagrum application.
/// </summary>
[RegisterSingleton<AppHost>]
public class AppHost(
    IConfiguration configuration,
    NativeApplication application,
    NativeWindow window,
    BlazorWebView webView,
    SteppedMigrationUpgrader migrationUpgrader,
    IGameLauncher launcher,
    UpdateService updater,
    MigrationRunner migrations,
    AppStateService appState,
    IPlatformManager platform,
    ISplashScreen splash,
    NativeDispatcher dispatcher,
    ObservedTaskScheduler scheduler)
{
    /// <summary>
    /// Runs the application, does not return until the main window is closed.
    /// </summary>
    public Task RunAsync(string[] args)
    {
        // Handle file association
        if (args.Length == 1 && args[0].EndsWith(".fmod", StringComparison.OrdinalIgnoreCase))
        {
            application.FmodPath = args[0];
        }

        // Handle game launch mode
        if (args.Any(a => a == "--launch"))
        {
            Launch();
            return Task.CompletedTask; // Flagrum was invoked only to launch the game, so terminate here
        }

        // Handle standard run mode
        return RunAsync();
    }

    private async Task RunAsync()
    {
        // Upgrade from the legacy data migrations system if needed
        migrationUpgrader.Run();

        // Initialize the application
        SetCulture();
        platform.EnableTaskbarStacking();
        platform.SetFileTypeAssociation();

        // Run startup code on separate thread so event loop can show/update the splash screen during load
        scheduler.RunAsyncObserved(StartAsync);

        // Run the application until the main window is closed
        application.Run();

        // Ensure the log is written and closed before terminating
        await Log.CloseAndFlushAsync();
    }

    private async Task StartAsync()
    {
        var start = DateTime.UtcNow;

        // Show splash screen
        dispatcher.Invoke(splash.Show);

        // Check for updates
        if (await updater.TryUpdate())
        {
            await Log.CloseAndFlushAsync();
            return; // Application is restarting, finish here
        }

        // Run all data migrations
        await migrations.RunMigrationsAsync();

        // Check the application version
        if (!platform.IsVersionSupported)
        {
            NativeMessageBox.Show("Error", "This version of Flagrum is no longer supported.",
                MessageType.Error);

            await Log.CloseAndFlushAsync();
            return;
        }

        // Start initializing the asset explorer
        appState.LoadNodes();

        // Set up the Blazor web view
        await webView.SetRootComponentAsync<App>("#app");
        dispatcher.Invoke(() => { webView.Navigate(BlazorWebViewManager.CreateUri("/")); });

#if !DEBUG
        // Ensure the splash screen is displayed no less than two seconds
        var elapsed = DateTime.UtcNow - start;
        var remaining = TimeSpan.FromSeconds(2) - elapsed;
        if (remaining.TotalMilliseconds > 0)
        {
            await Task.Delay(remaining);
        }
#endif

        // Initialize the main window
        dispatcher.Invoke(() =>
        {
            window.SetWebView(webView.NativeImpl);
            window.Resize(1680, 1024);
            window.Show();
            splash.Close();
        });
    }

    /// <summary>
    /// Launches the game.
    /// </summary>
    private void Launch()
    {
        var result = launcher.TryLaunch(false);
        if (result != GameLaunchResult.Success)
        {
            var message = result switch
            {
                GameLaunchResult.GameAlreadyRunning =>
                    "Flagrum detected that the game is already running, " +
                    "so it cannot launch again until the game is closed.",
                GameLaunchResult.UnsupportedExecutable =>
                    "Flagrum did not recognize the FFXV executable, " +
                    "the mod loader only supports the latest Steam release of the game.",
                GameLaunchResult.AccessDenied =>
                    "Flagrum was unable to launch FFXV due to insufficient permissions. " +
                    "Please run Flagrum as administrator and try again.",
                _ => throw new NotSupportedException($"Did not recognize launch result {result}.")
            };

            NativeMessageBox.Show("Error", message, MessageType.Error);
        }
    }

    private void SetCulture()
    {
        // Seems that the application culture needs to be set in the constructor
        // See https://github.com/Kizari/Flagrum/issues/94
        try
        {
            // Set culture based on stored language settings if any
            var cultureName = configuration.Get<string?>(StateKey.Language);
            if (cultureName != null)
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
        }
        catch
        {
            // Ignore silently, not important
        }
    }
}