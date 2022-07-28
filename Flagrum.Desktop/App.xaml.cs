using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Squirrel;

namespace Flagrum.Desktop;

public partial class App
{
    public App()
    {
        SetCurrentProcessExplicitAppUserModelID("Flagrum");
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => DumpCrash((Exception)e.ExceptionObject);
        Current.DispatcherUnhandledException += (sender, e) => DumpCrash(e.Exception);
        TaskScheduler.UnobservedTaskException += (sender, e) => DumpCrash(e.Exception);

        SquirrelAwareApp.HandleEvents(
            OnInstall,
            onAppUninstall: OnUninstall);

        using var context = new FlagrumDbContext();
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