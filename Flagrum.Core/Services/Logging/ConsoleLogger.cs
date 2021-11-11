using System;

namespace Flagrum.Core.Services.Logging
{
    public class ConsoleLogger : Logger
    {
        protected override void Log(LogLevel logLevel, Exception exception, string message)
        {
            Console.WriteLine($"[{logLevel.ToString()[0]}] {message}");

            if (exception != null)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
