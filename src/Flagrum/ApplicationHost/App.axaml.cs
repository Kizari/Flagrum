using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Flagrum.Abstractions;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Services;
using Flagrum.Core.Utilities;
using Flagrum.Migrations;
using Flagrum.Services;
using Flagrum.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Serilog;

namespace Flagrum.ApplicationHost;

/// <summary>
/// Main Avalonia application for Flagrum.
/// </summary>
public partial class App : Avalonia.Application
{
    private VersionHelper _versionHelper = null!;

    public App()
    {
        CrashHelper.InitializeApplication();

        // Seems that the application culture needs to be set in the constructor
        // See https://github.com/Kizari/Flagrum/issues/94
        try
        {
            // Set culture based on stored language settings if any
            var configuration = Program.Services.GetRequiredService<Configuration>();
            var cultureName = configuration.Get<string>(StateKey.Language);
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
            
            DisableAvaloniaDataAnnotationValidation();
            
            // Show the splash screen
            var splash = new SplashWindow();
            desktop.MainWindow = splash;
            splash.Show();

            new Thread(() =>
            {
                var fmodPath = desktop.Args?.Length == 1 && desktop.Args[0].EndsWith(".fmod") ? desktop.Args[0] : null;
                StartupAsync(splash, fmodPath).Wait();
            }).Start();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    /// <summary>
    /// Disables Avalonia's data annotation validation to prevent duplicate validations
    /// when used alongside CommunityToolkit.Mvvm.
    /// </summary>
    /// <remarks>
    /// <a href="https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins">
    /// More Information
    /// </a>
    /// </remarks>
    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private async Task StartupAsync(SplashWindow splash, string? fmodPath)
    {
        // Initialise application
#if _WIN32
        SetCurrentProcessExplicitAppUserModelID("Flagrum"); // Makes Flagrum stack with pinned taskbar icon
        SetFileTypeAssociation();
#endif

        // Check for updates
        if (await UpdateHelper.Update())
        {
            // Application is restarting, no need to continue initialising
            return;
        }

        // Initialise the version helper
        _versionHelper = new VersionHelper();

        // Run all data migrations
        var migrations = Program.Services.GetRequiredService<MigrationRunner>();
        await migrations.RunMigrationsAsync();

        // Check the application version
        if (!_versionHelper.IsCurrent())
        {
            await MessageBoxManager.GetMessageBoxStandard("Error",
                "This version of Flagrum is no longer supported.",
                ButtonEnum.Ok, Icon.Error).ShowAsync();
            Dispatcher.UIThread.Invoke(() => ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown());
            return;
        }

        // Start initialising the asset explorer
        Program.Services.GetRequiredService<AppStateService>().LoadNodes();

        // Startup is complete, show the main window
        Dispatcher.UIThread.Invoke(() =>
        {
            var desktop = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
            desktop.MainWindow = new MainWindow(fmodPath);
            ((PlatformService)Program.Services.GetRequiredService<IPlatformService>())
                .SetStorageProvider(desktop.MainWindow.StorageProvider);
            desktop.MainWindow.Show();
            splash.Close();
        });
    }

    private static void OnShutdown(object? sender, ShutdownRequestedEventArgs? e)
    {
        Log.CloseAndFlush();
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

    [DllImport("Shell32.dll")]
    private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

    private void SetFileTypeAssociation()
    {
        var flagrumPath = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "Flagrum.exe") + " \"%1\"";
        if ((string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Classes\\.fmod", "", "Flagrum")! != flagrumPath)
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\Flagrum", "", "FMOD");
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\Flagrum", "FriendlyTypeName", "Flagrum Mod");
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\Flagrum\\shell\\open\\command", "",
                flagrumPath);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\.fmod", "", "Flagrum");

            //this call notifies Windows that it needs to redo the file associations and icons
            _ = SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);
        }
    }
}