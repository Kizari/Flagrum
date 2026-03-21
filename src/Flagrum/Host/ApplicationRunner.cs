using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Application;
using Flagrum.Abstractions.ModManager;
using Flagrum.Application;
using Flagrum.Application.Services;
using Flagrum.Components;
using Flagrum.Migrations;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Velopack;
using Velopack.Sources;

namespace Flagrum.Host;

/// <summary>
/// Handles initializing and executing the application.
/// </summary>
[RegisterSingleton<ApplicationRunner>]
public class ApplicationRunner(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ApplicationHost application,
    SteppedMigrationUpgrader migrationUpgrader,
    IGameLauncher launcher,
    MigrationRunner migrations,
    AppStateService appState,
    IPlatformManager platform,
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
            application.AssociatedFile = args[0];
        }

        // Handle game launch mode
        if (args.Any(a => a == "--launch"))
        {
            LaunchGame();
            return Task.CompletedTask; // Flagrum was invoked only to launch the game, so terminate here
        }
        
        // Handle Linux game launch mode
        var launchCommand = args.FirstOrDefault(a => a.StartsWith("--launch-command"));
        if (launchCommand != null)
        {
            LaunchGame(launchCommand.Split('=')[1]);
            return Task.CompletedTask; // Flagrum was invoked only to launch the game, so terminate here
        }

        // Handle standard run mode
        return RunAsync();
    }

    /// <summary>
    /// Executes the standard run procedure (i.e. no special launch flags were set).
    /// </summary>
    private async Task RunAsync()
    {
        // Upgrade from the legacy data migrations system if needed
        migrationUpgrader.Run();

        // Initialize the application
        SetCulture();
        platform.EnableTaskbarStacking();
        platform.SetFileTypeAssociation();

        // Run initialization code on separate thread so event loop can show/update the splash screen during load
        scheduler.RunAsyncObserved(InitializeAsync);

        // Run the application until the main window is closed
        application.Run();

        // Ensure the log is written and closed before terminating
        await Log.CloseAndFlushAsync();
    }

    /// <summary>
    /// Runs through the initialization process with the splash screen active,
    /// then launches the main application window.
    /// </summary>
    private async Task InitializeAsync()
    {
        var start = DateTime.UtcNow;

        // Show splash screen
        application.OpenSplash();

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
            application.ShowMessageBox("Error", "This version of Flagrum is no longer supported.",
                MessageBoxType.Critical);

            await Log.CloseAndFlushAsync();
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
        // TODO: Consider if it's possible to load the window and web view during splash screen before showing it
        application.OpenMainWindow();
        var webView = serviceProvider.GetRequiredService<BlazorWebView>(); // Must be resolved after window opened
        await webView.SetRootComponentAsync<App>("#app");
        webView.Navigate(BlazorWebViewManager.CreateUri("/"));
        application.CloseSplash();
    }

    /// <summary>
    /// Launches the game.
    /// </summary>
    private void LaunchGame(string? command = null)
    {
        var result = launcher.TryLaunch(false, command);
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

            application.ShowMessageBox("Error", message, MessageBoxType.Critical);
        }
    }

    /// <summary>
    /// Sets the culture of the application as per user preferences.
    /// </summary>
    private void SetCulture()
    {
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
    
    /// <summary>
    /// Attempts to update the application.
    /// </summary>
    /// <returns><c>true</c> if an update was applied, otherwise <c>false</c>.</returns>
    private async Task<bool> TryUpdateAsync()
    {
        try
        {
            application.SetSplashText("Checking for updates");

            // Check for updates
            var github = new GithubSource("https://github.com/Kizari/Flagrum", null, false);
            var manager = new UpdateManager(github);
            var newVersion = await manager.CheckForUpdatesAsync();

            // Download and apply updates if any were available
            if (newVersion != null)
            {
                application.SetSplashText("Downloading updates");
                await manager.DownloadUpdatesAsync(newVersion);
                application.SetSplashText("Updating Flagrum");
                manager.ApplyUpdatesAndRestart();
                return true;
            }
        }
        catch
        {
            // Not much to be done if this fails, most likely due to no internet or user blocking the update URL
            // Let Flagrum continue as normal
        }

        application.SetSplashText("Loading");
        return false;
    }
}