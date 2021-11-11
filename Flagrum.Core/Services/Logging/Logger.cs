using System;

namespace Flagrum.Core.Services.Logging
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    public abstract class Logger
    {
        protected abstract void Log(LogLevel logLevel, Exception exception, string message);

        public void LogTrace(string message) => Log(LogLevel.Trace, null, message);
        public void LogTrace(Exception exception, string message) => Log(LogLevel.Trace, exception, message);

        public void LogDebug(string message) => Log(LogLevel.Debug, null, message);
        public void LogDebug(Exception exception, string message) => Log(LogLevel.Debug, exception, message);

        public void LogInformation(string message) => Log(LogLevel.Information, null, message);
        public void LogInformation(Exception exception, string message) => Log(LogLevel.Information, exception, message);

        public void LogWarning(string message) => Log(LogLevel.Warning, null, message);
        public void LogWarning(Exception exception, string message) => Log(LogLevel.Warning, exception, message);

        public void LogError(string message) => Log(LogLevel.Error, null, message);
        public void LogError(Exception exception, string message) => Log(LogLevel.Error, exception, message);

        public void LogCritical(string message) => Log(LogLevel.Critical, null, message);
        public void LogCritical(Exception exception, string message) => Log(LogLevel.Critical, exception, message);
    }
}
