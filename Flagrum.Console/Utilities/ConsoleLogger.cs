using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Flagrum.Desktop.Services;

public class ConsoleLoggerConfiguration
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> Colors { get; set; } = new()
    {
        {LogLevel.Error, ConsoleColor.Red},
        {LogLevel.Critical, ConsoleColor.DarkRed},
        {LogLevel.Debug, ConsoleColor.DarkGray},
        {LogLevel.Trace, ConsoleColor.Gray},
        {LogLevel.Information, ConsoleColor.DarkGray},
        {LogLevel.Warning, ConsoleColor.Yellow},
        {LogLevel.None, ConsoleColor.DarkGray}
    };
}

public class ConsoleLogger<T> : ILogger<T>
{
    private readonly Func<ConsoleLoggerConfiguration> _getCurrentConfiguration;
    private readonly string _logFile;
    private readonly string _name;

    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public ConsoleLogger(
        string name,
        Func<ConsoleLoggerConfiguration> getCurrentConfiguration)
    {
        _name = name;
        _getCurrentConfiguration = getCurrentConfiguration;
        _logFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Flagrum\\Log.txt";
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // var configuration = _getCurrentConfiguration();
        // var originalColor = Console.ForegroundColor;
        // Console.ForegroundColor = configuration.Colors[logLevel];
        // Console.WriteLine($"[{logLevel.ToString()}] {formatter(state, exception)}");
        // Console.ForegroundColor = originalColor;
        _lock.Wait();
        try
        {
            File.AppendAllText(_logFile, formatter(state, exception) + "\r\n");
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return default!;
    }
}

public class ConsoleLoggerProvider<T> : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ConsoleLogger<T>> _loggers = new();
    private readonly IDisposable _onChangeToken;
    private ConsoleLoggerConfiguration _currentConfig;

    public ConsoleLoggerProvider(
        IOptionsMonitor<ConsoleLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ConsoleLogger<T>(name, GetCurrentConfig));
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken.Dispose();
    }

    private ConsoleLoggerConfiguration GetCurrentConfig()
    {
        return _currentConfig;
    }
}