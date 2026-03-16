using System.Threading;
using Flagrum.ApplicationHost.Native;
using Injectio.Attributes;

namespace Flagrum.Services;

/// <inheritdoc />
[RegisterSingleton<ISplashScreen>]
public class SplashScreen : ISplashScreen
{
    private readonly Lock _lock = new();

    private NativeSplashWindow? _splash = new();

    /// <inheritdoc />
    public void Show()
    {
        lock (_lock)
        {
            _splash?.Show();
        }
    }

    /// <inheritdoc />
    public void Close()
    {
        lock (_lock)
        {
            _splash?.Close();
            _splash?.Dispose();
            _splash = null;
        }
    }

    /// <inheritdoc />
    public void SetLoadingText(string text)
    {
        lock (_lock)
        {
            _splash?.SetLoadingText(text);
        }
    }
}