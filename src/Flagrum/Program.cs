using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Application.Services;
using Flagrum.Core.Utilities;
using Flagrum.Host;
using Flagrum.Generators;
using Flagrum.Migrations;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NuGet.Versioning;
using Serilog;
using Serilog.Events;
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
        InitializeLogging();
        _services = ConfigureServices();

        // Initialize Velopack
        VelopackApp.Build()
            .WithFirstRun(OnFreshInstall)
#if WINDOWS
            .WithBeforeUninstallFastCallback(OnBeforeUninstall)
#endif
            .Run();

        // Run the application
        await _services.GetRequiredService<ApplicationRunner>().RunAsync(args);
    }

    /// <summary>
    /// Initializes logging for the application.
    /// </summary>
    private static void InitializeLogging()
    {
        // Set up Serilog to write to log files that roll over daily
        var logDirectory = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "logs");
        IOHelper.EnsureDirectoryExists(logDirectory);
        var path = Path.Combine(logDirectory, "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(path, LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
    
    /// <summary>
    /// Sets up IoC for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        // Populate the service collection
        var services = new ServiceCollection()
            .AddLogging(l => l.AddSerilog())
            .AddSingleton<IProfileService, ProfileService>()
            .AddSingleton<AppStateService>()
            .AddSingleton<JSComponentConfigurationStore>()
            .AddSingleton<IFileProvider>(_ => new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")))
            .AddBlazorWebView()
            .AddFlagrum()
            .AddFlagrumApplicationManual()
            .AddDataMigrations();

        return services.BuildServiceProvider();
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