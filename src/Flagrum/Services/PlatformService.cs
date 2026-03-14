using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Flagrum.Abstractions;
using MainViewModel = Flagrum.ApplicationHost.MainViewModel;

namespace Flagrum.Services;

public class PlatformService : IPlatformService
{
    private IStorageProvider? _storageProvider;
    private IClipboard? _clipboard;
    
    public MainViewModel Main { get; set; } = null!;

    public void SetClipboard(IClipboard clipboard) => _clipboard = clipboard;
    public void SetStorageProvider(IStorageProvider storageProvider) => _storageProvider = storageProvider;

    public async Task OpenFileDialogAsync(string filter, Func<string, Task> onFileSelected)
    {
        var result = await _storageProvider!.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = [new FilePickerFileType(filter)]
        });

        if (result.Count > 0)
        {
            await onFileSelected(result[0].Path.LocalPath);
        }
    }

    public async Task OpenFolderDialogAsync(string initialDirectory, Func<string, Task> onFolderSelected)
    {
        var result = await _storageProvider!.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            SuggestedStartLocation = await _storageProvider.TryGetFolderFromPathAsync(initialDirectory)
        });

        if (result.Count > 0)
        {
            await onFolderSelected(result[0].Path.LocalPath);
        }
    }

    public async Task OpenSaveFileDialogAsync(string defaultName, string filter, Func<string, Task> onFileSelected)
    {
        var result = await _storageProvider!.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = defaultName,
            FileTypeChoices = [new FilePickerFileType(filter)],
            ShowOverwritePrompt = true
        });

        if (result != null)
        {
            await onFileSelected(result.Path.LocalPath);
        }
    }

    public Version GetVersion() => typeof(PlatformService).Assembly.GetName().Version!;

    public void Restart()
    {
        var executablePath = Path.Combine(Directory.GetCurrentDirectory(), "Flagrum");
        if (!File.Exists(executablePath))
        {
            executablePath += ".exe";
        }

        ((IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!).Shutdown();

        // Changed from System.Windows.Forms.Application.Restart() to solve https://github.com/Kizari/Flagrum/issues/81
        // Keeping this comment here because it's worth noting that Application.Restart would replay the startup args
        Process.Start(executablePath);
    }

    // TODO: Handle this properly once MainViewModel is removed/reworked
    public string? GetFmodPath() => Main?.FmodPath;

    public void ClearFmodPath()
    {
        Main.FmodPath = null;
    }

    public Task SetClipboardTextAsync(string text) => _clipboard!.SetTextAsync(text);

    public void RefreshPatreonButton()
    {
        Main.RefreshPatreonButton();
    }
}