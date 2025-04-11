using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Flagrum.Core.Utilities;

namespace Flagrum.Utilities;

public static class CrashHelper
{
    [Conditional("RELEASE")]
    public static void Initialize()
    {
        // Ensure the crash directory exists
        var crashDirectory = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "crashes");
        IOHelper.EnsureDirectoryExists(crashDirectory);

        // Setup exception events
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            DumpCrashLog((Exception)e.ExceptionObject, true);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            DumpCrashLog(e.Exception, false);
            throw new OffMainThreadException();
        };

        // Delete crash logs older than 30 days if there are more than 50 crash logs present
        var crashLogs = Directory.EnumerateFiles(crashDirectory).ToList();
        if (crashLogs.Count > 50)
        {
            foreach (var file in crashLogs
                         .Where(file => (DateTime.Now - File.GetLastWriteTime(file)).TotalDays > 30))
            {
                File.Delete(file);
            }
        }
    }

    [Conditional("RELEASE")]
    public static void InitializeApplication()
    {
        App.Current.DispatcherUnhandledException += (_, e) =>
        {
            DumpCrashLog(e.Exception, false);
            throw new OffMainThreadException();
        };
    }

    [Conditional("RELEASE")]
    private static void DumpCrashLog(Exception exception, bool fromMainThread)
    {
        // OffMainThreadException is thrown purely to crash the application as the true exception was already logged
        for (var i = exception; i != null; i = i.InnerException)
        {
            if (i is OffMainThreadException)
            {
                if (fromMainThread)
                {
                    MessageBox.Show("Flagrum has encountered a fatal error and must now close.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }
        }
        
        try
        {
            var crashDirectory = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "crashes");
            var now = DateTime.Now;
            var fileName =
                $"{now:yyyy-MM-dd}_{now:hh-mm-ss-ffff}_crash_{Math.Abs(exception.Message.GetHashCode())}.txt";

            var builder = new StringBuilder();
            var innerException = exception;
            while (innerException != null)
            {
                builder.AppendLine(innerException.GetType().FullName);
                builder.AppendLine("Message: " + innerException.Message);
                builder.AppendLine("Stack Trace: " + innerException.StackTrace);
                innerException = innerException.InnerException;
            }

            File.WriteAllText(Path.Combine(crashDirectory, fileName), builder.ToString());
        }
        catch (Exception)
        {
            try
            {
                var crashDirectory = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "crashes");
                var now = DateTime.Now;
                var fileName =
                    $"{now:yyyy-MM-dd}_{now:hh-mm-ss-ffff}_crash_{Math.Abs(exception.Message.GetHashCode())}.txt";

                var crash = "Failed to write standard crash log. Retrying with less information.\n";
                crash += exception.Message;

                File.WriteAllText(Path.Combine(crashDirectory, fileName), crash);
            }
            catch (Exception e)
            {
                if (fromMainThread)
                {
                    MessageBox.Show("Failed to save crash log.\n\n" + e.Message);
                }
            }
        }
        finally
        {
            if (fromMainThread)
            {
                MessageBox.Show("Flagrum has encountered a fatal error and must now close.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}