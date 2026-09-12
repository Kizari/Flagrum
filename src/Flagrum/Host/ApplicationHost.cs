using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Application;
using Flagrum.Components;
using Flagrum.Host.Utilities;
using Injectio.Attributes;

namespace Flagrum.Host;

/// <summary>
/// Wraps the <see cref="Avalonia.Application" /> to expose platform functionality to the rest of the application.
/// </summary>
[RegisterSingleton<IApplication>]
public sealed class ApplicationHost(ObservedTaskScheduler scheduler) : IApplication
{
    public IClassicDesktopStyleApplicationLifetime? AvaloniaApplication { get; set; }

    /// <inheritdoc />
    public Version Version => typeof(Program).Assembly.GetName().Version!;

    /// <inheritdoc />
    public string? AssociatedFile { get; set; }

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(Action callback) => Dispatcher.UIThread.Invoke(callback);

    /// <inheritdoc />
    public async Task<string?> OpenFileAsync(
        FileDialogFileType[] filter,
        string? initialDirectory = null,
        string caption = "Open File")
    {
        var storage = AvaloniaApplication!.MainWindow!.StorageProvider;

        var result = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = filter.ToAvaloniaFilter()
        });

        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    /// <inheritdoc />
    public async Task<string?> SaveFileAsync(
        FileDialogFileType[] filter,
        string? defaultFileName = null,
        string caption = "Save File")
    {
        var storage = AvaloniaApplication!.MainWindow!.StorageProvider;

        var result = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = defaultFileName,
            FileTypeChoices = filter.ToAvaloniaFilter(),
            ShowOverwritePrompt = true
        });

        return result?.Path.LocalPath;
    }

    /// <inheritdoc />
    public async Task<string?> OpenDirectoryAsync(string caption = "Select Folder")
    {
        var storage = AvaloniaApplication!.MainWindow!.StorageProvider;
        var result = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    /// <inheritdoc />
    public void Restart()
    {
        // Determine path to the Flagrum executable
        var executablePath = Path.Combine(Directory.GetCurrentDirectory(), "Flagrum");
        if (!File.Exists(executablePath))
        {
            executablePath += ".exe";
        }

        // Restart the application
        scheduler.RunAsyncObserved(async () =>
        {
            // Delay is required to prevent a TaskCanceledException from occurring when shutting down the application
            // due to this method itself running from the WebViewDispatcher queue.
            // This exception does not appear to be interceptible in the current version of Avalonia.Controls.WebView
            // and so the delay is the only way around it, although not guaranteed to work every time on every system
            await Task.Delay(500);

            // Shutdown must be done from the UI thread
            Invoke(() =>
            {
                AvaloniaApplication!.Shutdown();
                Process.Start(executablePath);
            });
        });
    }

    /// <inheritdoc />
    public async Task SetClipboardTextAsync(string text)
    {
        if (AvaloniaApplication!.MainWindow?.Clipboard != null)
        {
            await AvaloniaApplication.MainWindow.Clipboard.SetTextAsync(text);
        }
    }

    /// <inheritdoc />
    public void RefreshPatreonButton()
    {
        if (AvaloniaApplication?.MainWindow is MainWindow window)
        {
            window.RefreshPatreonButton();
        }
    }

    /// <inheritdoc />
    public void SetSplashText(string text)
    {
        if (AvaloniaApplication!.MainWindow is SplashWindow splash)
        {
            splash.SetText(text);
        }
    }
}