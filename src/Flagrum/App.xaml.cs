using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Main;
using Flagrum.Migrations;
using Flagrum.Utilities;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Serilog;

namespace Flagrum;

public partial class App
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

    public new static App Current => (App)System.Windows.Application.Current;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Show the splash screen
        var splash = new SplashWindow();
        MainWindow = splash;
        splash.Show();

        new Thread(() =>
        {
            var fmodPath = e.Args.Length == 1 && e.Args[0].EndsWith(".fmod") ? e.Args[0] : null;
            StartupAsync(splash, fmodPath).Wait();
        }).Start();
    }

    private async Task StartupAsync(SplashWindow splash, string? fmodPath)
    {
        // Initialise application
        SetCurrentProcessExplicitAppUserModelID("Flagrum"); // Makes Flagrum stack with pinned taskbar icon
        SetFileTypeAssociation();

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

        // Ensure WebView2 is installed
        await WebView2Helper.EnsureInstalled();

        // Check the application version
        if (!_versionHelper.IsCurrent())
        {
            MessageBox.Show("This version of Flagrum is no longer supported.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Dispatcher.InvokeAsync(() => Current.Shutdown(0));
            return;
        }

        // Start initialising the asset explorer
        Program.Services.GetRequiredService<AppStateService>().LoadNodes();

        // Startup is complete, show the main window
        Dispatcher.Invoke(() =>
        {
            MainWindow = new MainWindow(fmodPath);
            MainWindow.Show();
            splash.Close();
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
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