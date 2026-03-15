using System;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions.ModManager;
using Flagrum.Application;
using Flagrum.ApplicationHost.Native;
using Flagrum.ApplicationHost.WebView;
using Flagrum.Migrations;
using Injectio.Attributes;

namespace Flagrum.ApplicationHost;

/// <summary>
/// The Flagrum application.
/// </summary>
[RegisterSingleton<AppHost>]
public class AppHost(
    NativeApplication application,
    NativeWindow window,
    BlazorWebView webView,
    SteppedMigrationUpgrader migrationUpgrader,
    IGameLauncher launcher)
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
        // Run startup logic
        Initialize();

        // Set up the Blazor web view
        await webView.SetRootComponentAsync<App>("#app");
        webView.Navigate(BlazorWebViewManager.CreateUri("/"));

        // Initialize the main window
        window.SetWebView(webView.NativeImpl);
        window.Resize(1680, 1024);
        window.Show();

        // Run the application until the main window is closed
        application.Run();
    }

    /// <summary>
    /// Initializes the application.
    /// </summary>
    private void Initialize()
    {
        // Upgrades from the legacy data migrations system if needed
        migrationUpgrader.Run();
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
}