using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Squirrel;

namespace Flagrum.Desktop;

public partial class App
{
    public App()
    {
        // This is needed so Flagrum stacks with its pinned icon on the taskbar
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

        SetFileTypeAssociation();

        // Migrate the database if required
        Profile = new ProfileService();
        AssetExplorerNode.Profile = Profile;
        using var context = new FlagrumDbContext(Profile);
        context.Database.MigrateAsync().Wait();

        // If this is the first time using the profiles system
        if (Profile.DidMigrateThisSession)
        {
            // Move the paths out into the profiles file
            var gamePath = context.GetString(StateKey.GamePath);
            var binmodListPath = context.GetString(StateKey.BinmodListPath);

            Profile.GamePath = gamePath;
            Profile.BinmodListPath = binmodListPath;

            context.DeleteStateKey(StateKey.GamePath);
            context.DeleteStateKey(StateKey.BinmodListPath);

            var oldRoot = context.AssetExplorerNodes
                .Where(n => n.ParentId == null)
                .Select(n => n.Id)
                .First();

            var newRoot = new AssetExplorerNode {Name = ""};
            context.AssetExplorerNodes.Add(newRoot);
            context.SaveChanges();

            var oldRootEntity = context.AssetExplorerNodes.Find(oldRoot)!;
            oldRootEntity.ParentId = newRoot.Id;
            oldRootEntity.Name = "data:";
            context.SaveChanges();
            
            // Update the mod paths to point to the new locations
            foreach (var file in context.EarcModReplacements)
            {
                if (file.ReplacementFilePath?.StartsWith($@"{Profile.FlagrumDirectory}\earc") == true)
                {
                    file.ReplacementFilePath = file.ReplacementFilePath.Replace($@"{Profile.FlagrumDirectory}\earc", Profile.EarcModsDirectory);
                }
            }
            
            context.SaveChanges();

            foreach (var file in context.EarcModLooseFile)
            {
                if (file.FilePath?.StartsWith($@"{Profile.FlagrumDirectory}\earc") == true)
                {
                    file.FilePath = file.FilePath.Replace($@"{Profile.FlagrumDirectory}\earc", Profile.EarcModsDirectory);
                }
            }
            
            context.SaveChanges();
        }

        // Start loading asset explorer nodes so the user won't be waiting too long
        var appStateContext = new FlagrumDbContext(Profile);
        AppState = new AppStateService(Profile, appStateContext, new UriMapper(appStateContext, Profile));
        AppState.LoadNodes();

        // Set default key bindings for 3D viewer
        if (!context.StatePairs.Any(p => p.Key == StateKey.ViewportRotateModifierKey))
        {
            context.SetEnum(StateKey.ViewportRotateModifierKey, ModifierKeys.None);
            context.SetEnum(StateKey.ViewportRotateMouseAction, MouseAction.MiddleClick);
        }

        if (!context.StatePairs.Any(p => p.Key == StateKey.ViewportPanModifierKey))
        {
            context.SetEnum(StateKey.ViewportPanModifierKey, ModifierKeys.Shift);
            context.SetEnum(StateKey.ViewportPanMouseAction, MouseAction.MiddleClick);
        }

        if (!context.StatePairs.Any(p => p.Key == StateKey.CurrentAssetNode))
        {
            context.SetInt(StateKey.CurrentAssetNode, 1);
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

    public AppStateService AppState { get; }
    public ProfileService Profile { get; }

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

    [DllImport("Shell32.dll")]
    private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

    private void SetFileTypeAssociation()
    {
        var flagrumPath =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Flagrum\\Flagrum.exe \"%1\"";
        //var flagrumPath = $"{IOHelper.GetExecutingDirectory()}\\Flagrum.exe \"%1\"";
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

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        string? fmodPath = null;

        if (e.Args.Length == 1 && e.Args[0].EndsWith(".fmod"))
        {
            fmodPath = e.Args[0];
        }

        var window = new MainWindow(Profile, AppState, fmodPath);
        window.Show();
    }
}