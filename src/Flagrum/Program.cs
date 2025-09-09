using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Flagrum.Abstractions;
using Flagrum.Generators;
using Flagrum.Migrations;
using Flagrum.Utilities;
using Flagrum.Application.Features.ModManager.Launcher;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NuGet.Versioning;
using Velopack;

namespace Flagrum;

internal static class Program
{
    /// <summary>
    /// The dependency injection service container for the application.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Commandline arguments that Flagrum was launched with.</param>
    [STAThread]
    private static async Task Main(string[] args)
    {
        // Program setup
        CrashHelper.Initialize();
        Services = ServiceHelper.ConfigureServices();
        
        OnFreshInstall(new SemanticVersion(1, 6, 5));

        // Initialize Velopack
        VelopackApp.Build()
            .WithFirstRun(OnFreshInstall)
            .WithBeforeUninstallFastCallback(OnBeforeUninstall)
            .Run();

        // Handle commandline arguments
        if (args.Any(a => a == "--launch"))
        {
            var launcher = Services.GetRequiredService<GameLauncher>();
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

                await MessageBoxManager.GetMessageBoxStandard("Error", message, ButtonEnum.Ok, Icon.Error)
                    .ShowAsync();
            }

            // Flagrum was invoked only to launch the game, so terminate here
            return;
        }

        // Run pending data migrations
        Services.GetRequiredService<SteppedMigrationUpgrader>().Run();

        // Run the application
        RunApp(args);
    }

    /// <summary>
    /// Runs the WPF application until shutdown is requested.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void RunApp(string[] args)
    {
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Runs setup code for fresh installations of Flagrum.
    /// </summary>
    private static void OnFreshInstall(SemanticVersion version)
    {
        // Ensure that past data migrations are set as completed to prevent them running
        var configuration = Services.GetRequiredService<IConfiguration>();
        configuration.OnFreshInstall(SteppedMigrationHelper.ApplicationSteps, SteppedMigrationHelper.ProfileSteps);
    }

    /// <summary>
    /// Cleans up residual data when the user uninstalls Flagrum.
    /// </summary>
    private static void OnBeforeUninstall(SemanticVersion version)
    {
        var profile = Services.GetRequiredService<IProfileService>();
        
        try
        {
            Directory.Delete(profile.TemporaryDirectory, true);
        }
        catch
        {
            // Too late to recover from this and try to resolve it, nothing more to do
        }
    }
}