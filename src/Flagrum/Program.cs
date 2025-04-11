using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Flagrum.Abstractions;
using Flagrum.Generators;
using Flagrum.Migrations;
using Flagrum.Utilities;
using Flagrum.Application.Features.ModManager.Launcher;
using Flagrum.Application.Features.Settings.Data;
using Microsoft.Extensions.DependencyInjection;
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
    private static void Main(string[] args)
    {
        // Program setup
        CrashHelper.Initialize();
        Services = ServiceHelper.ConfigureServices();

        // Initialize Velopack
        VelopackApp.Build()
            .WithFirstRun(OnFreshInstall)
            .Run();
        
        OnFreshInstall(new SemanticVersion(1, 5, 22));

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

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Flagrum was invoked only to launch the game, so terminate here
            return;
        }

        // Run pending data migrations
        Services.GetRequiredService<SteppedMigrationUpgrader>().Run();

        // Run the application
        RunApp();
    }

    /// <summary>
    /// Runs the WPF application until shutdown is requested.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void RunApp()
    {
        var app = new App();
        app.InitializeComponent();
        app.Run();
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
}