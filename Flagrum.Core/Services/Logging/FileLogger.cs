using System;

namespace Flagrum.Core.Services.Logging;

public class FileLogger : Logger
{
    protected override void Log(LogLevel logLevel, Exception exception, string message)
    {
        Console.WriteLine($"{DateTime.Now:d-M h:mmtt} - [{logLevel.ToString()[0]}] {message}");

        if (exception != null)
        {
            Console.WriteLine(exception.Message);
        }
    }
}