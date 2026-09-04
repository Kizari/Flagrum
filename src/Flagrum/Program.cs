using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Application;
using Flagrum.Abstractions.ModManager;
using Flagrum.Application.Services;
using Flagrum.Components;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Flagrum.Host;
using Flagrum.Host.Shell;
using Flagrum.Host.Utilities;
using Flagrum.Migrations;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MsBox.Avalonia.Enums;
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
        
        // Handle game launch mode
        if (args.Any(a => a == "--launch"))
        {
            await LaunchGameAsync();
            return; // Flagrum was invoked only to launch the game, so terminate here
        }
        
        // Handle Linux game launch mode
        var launchCommand = args.FirstOrDefault(a => a.StartsWith("--launch-command"));
        if (launchCommand != null)
        {
            await LaunchGameAsync(launchCommand.Split('=')[1]);
            return; // Flagrum was invoked only to launch the game, so terminate here
        }

        // Handle standard run mode
        _services.GetRequiredService<SteppedMigrationUpgrader>().Run(); // Upgrade from legacy data migrations if needed
        SetCulture();
        var platform = _services.GetRequiredService<IPlatformManager>();
        platform.EnableTaskbarStacking();
        platform.SetFileTypeAssociation();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Defines how to build the Avalonia application.
    /// </summary>
    /// <remarks>This method needs to be separate as it is also called by the XAML designer by convention.</remarks>
    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure(() =>
        {
            _services ??= ConfigureServices(); // Needed for XAML designer
            return new AvaloniaApplication(
                _services,
                _services.GetRequiredService<IApplication>(),
                _services.GetRequiredService<ObservedTaskScheduler>(),
                _services.GetRequiredService<MigrationRunner>(),
                _services.GetRequiredService<IPlatformManager>(),
                _services.GetRequiredService<AppStateService>());
        })
        .UsePlatformDetect()
#if DEBUG
        .WithDeveloperTools()
#endif
        .WithInterFont()
#pragma warning disable AVALONIA_X11_CSD
        .With(new X11PlatformOptions {EnableDrawnDecorations = true})
#pragma warning restore AVALONIA_X11_CSD
        .LogToTrace();

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
#if DEBUG
            .WriteTo.Console()
#endif
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
    
    /// <summary>
    /// Launches the game.
    /// </summary>
    private static async Task LaunchGameAsync(string? command = null)
    {
        var launcher = _services.GetRequiredService<IGameLauncher>();
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
                GameLaunchResult.InvalidLaunchConfiguration =>
                    "Flagrum was unable to launch FFXV due to an invalid launch configuration. " +
                    "Please run Flagrum normally and set up the launch configuration from the Mod Manager tab.",
                _ => throw new NotSupportedException($"Did not recognize launch result {result}.")
            };

            await MessageBox.ShowAsync("Error", message, Icon.Error);
        }
    }
    
    /// <summary>
    /// Sets the culture of the application as per user preferences.
    /// </summary>
    private static void SetCulture()
    {
        var configuration = _services.GetRequiredService<IConfiguration>();
        
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