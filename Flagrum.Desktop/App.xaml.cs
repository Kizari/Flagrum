using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Squirrel;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Flagrum.Desktop;

public partial class App
{
    public App()
    {
        SetCurrentProcessExplicitAppUserModelID("Flagrum");
        var resources = new ResourceManager("Flagrum.Desktop.Resources.Localisation", Assembly.GetExecutingAssembly());

        // Catch and log any exceptions that occur during runtime
        AppDomain.CurrentDomain.UnhandledException += (_, e) => DumpCrash((Exception)e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, e) => DumpCrash(e.Exception);
        Current.DispatcherUnhandledException += (_, e) =>
        {
            DumpCrash(e.Exception);

            if (e.Exception.InnerException != null && e.Exception.InnerException.Message.Contains("0x800704EC"))
            {
                MessageBox.Show(resources.GetString("WebView2Error"),
                    resources.GetString("Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        };

        // Declare events for Squirrel installer/updater software
        SquirrelAwareApp.HandleEvents(
            OnInstall,
            onAppUninstall: OnUninstall);

        // Migrate the database if required
        using var context = new FlagrumDbContext();
        context.Database.MigrateAsync().Wait();

        // Set default key bindings for 3D viewer
        if (!context.StatePairs.Any(p => p.Key == StateKey.ViewportRotateGesture))
        {
            context.SetString(StateKey.ViewportRotateGesture, "MiddleClick");
        }
        if (!context.StatePairs.Any(p => p.Key == StateKey.ViewportPanGesture))
        {
            context.SetString(StateKey.ViewportPanGesture, "Shift+MiddleClick");
        }

        // Set culture based on stored language settings if any
        var cultureName = context.GetString(StateKey.Language);

        if (cultureName != null)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

    private static void OnInstall(Version version)
    {
        using var manager = new UpdateManager("");
        manager.CreateShortcutForThisExe();
    }

    private static void OnUninstall(Version version)
    {
        using var manager = new UpdateManager("");
        manager.RemoveShortcutForThisExe();
    }

    private void DumpCrash(Exception? exception)
    {
        var flagrum = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Flagrum";

        if (!Directory.Exists(flagrum))
        {
            Directory.CreateDirectory(flagrum);
        }

        using var writer = new StreamWriter($"{flagrum}\\crash.txt");
        while (exception != null)
        {
            writer.WriteLine(exception.GetType().FullName);
            writer.WriteLine("Message: " + exception.Message);
            writer.WriteLine("Stack Trace: " + exception.StackTrace);

            exception = exception.InnerException;
        }
    }
}