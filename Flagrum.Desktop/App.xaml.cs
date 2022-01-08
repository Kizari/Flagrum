using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Flagrum.Web.Services;
using Squirrel;

namespace Flagrum.Desktop;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        SetCurrentProcessExplicitAppUserModelID("Flagrum");
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => DumpCrash((Exception)e.ExceptionObject);
        Current.DispatcherUnhandledException += (sender, e) => DumpCrash(e.Exception);
        TaskScheduler.UnobservedTaskException += (sender, e) => DumpCrash(e.Exception);

        SquirrelAwareApp.HandleEvents(
            OnInstall,
            OnUpdate,
            onAppUninstall: OnUninstall);
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

    private static void OnInstall(Version version)
    {
        using var manager = new UpdateManager("");
        manager.CreateShortcutForThisExe();
    }

    private static async void OnUpdate(Version version)
    {
        // Disgusting hack to present update restart confirmation window in Flagrum.Web
        while (WpfServiceHelper.OnAppUpdated == null)
        {
            await Task.Delay(100);
        }

        WpfServiceHelper.OnAppUpdated(() => UpdateManager.RestartApp());
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