using System;
using System.IO;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.ApplicationHost;
using Flagrum.Generators;
using Flagrum.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;
using Velopack;

namespace Flagrum;

internal static class Program
{
    private static IServiceProvider _services = null!;

    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Commandline arguments that Flagrum was launched with.</param>
    [STAThread]
    private static async Task Main(string[] args)
    {
        // Program setup
        CrashHelper.Initialize();
        _services = ServiceHelper.ConfigureServices();

        // Initialize Velopack
        VelopackApp.Build()
            .WithFirstRun(OnFreshInstall)
#if WINDOWS
            .WithBeforeUninstallFastCallback(OnBeforeUninstall)
#endif
            .Run();

        // Run the application
        await _services.GetRequiredService<AppHost>().RunAsync(args);
    }

    /// <summary>
    /// Runs setup code for fresh installations of Flagrum.
    /// </summary>
    private static void OnFreshInstall(SemanticVersion version)
    {
        // Ensure that past data migrations are set as completed to prevent them running
        var configuration = _services.GetRequiredService<IConfiguration>();
        configuration.OnFreshInstall(SteppedMigrationHelper.ApplicationSteps, SteppedMigrationHelper.ProfileSteps);
    }

    /// <summary>
    /// Cleans up residual data when the user uninstalls Flagrum.
    /// </summary>
    private static void OnBeforeUninstall(SemanticVersion version)
    {
        var profile = _services.GetRequiredService<IProfileService>();

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