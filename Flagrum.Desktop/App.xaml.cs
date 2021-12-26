using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

//using Squirrel;

namespace Flagrum.Desktop;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => DumpCrash((Exception)e.ExceptionObject);
        Current.DispatcherUnhandledException += (sender, e) => DumpCrash(e.Exception);
        TaskScheduler.UnobservedTaskException += (sender, e) => DumpCrash(e.Exception);

// #if !DEBUG
//         using var manager = new UpdateManager("");
//         {
//             SquirrelAwareApp.HandleEvents(
//                 onInitialInstall: v => manager.CreateShortcutForThisExe(),
//                 onAppUpdate: v => manager.CreateShortcutForThisExe(),
//                 onAppUninstall: v => manager.RemoveShortcutForThisExe());
//         }
// #endif
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