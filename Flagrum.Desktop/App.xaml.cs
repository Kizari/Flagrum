using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Flagrum.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            Dispatcher.UnhandledException += DispatcherOnUnhandledException;
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var flagrum = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Flagrum";

            if (!Directory.Exists(flagrum))
            {
                Directory.CreateDirectory(flagrum);
            }

            Exception exception = e.Exception;

            using (var writer = new StreamWriter($"{flagrum}\\crash.txt"))
            {
                while (exception != null)
                {
                    writer.WriteLine(exception.GetType().FullName);
                    writer.WriteLine("Message: " + exception.Message);
                    writer.WriteLine("Stack Trace: " + exception.StackTrace);

                    exception = exception.InnerException;
                }
            }
        }
    }
}
