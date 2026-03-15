using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.ApplicationHost.Native;
using Injectio.Attributes;

namespace Flagrum.Services;

[RegisterSingleton<IPlatformService>]
public class PlatformService(
    NativeApplication application,
    NativeFileDialog fileDialog) : IPlatformService
{
    public async Task OpenFileDialogAsync(string filter, Func<string, Task> onFileSelected)
    {
        var result = fileDialog.OpenFile("Open File", "", filter);
        if (result != null)
        {
            await onFileSelected(result);
        }
    }

    public async Task OpenFolderDialogAsync(string initialDirectory, Func<string, Task> onFolderSelected)
    {
        var result = fileDialog.OpenDirectory("Select Folder", initialDirectory);
        if (result != null)
        {
            await onFolderSelected(result);
        }
    }

    public async Task OpenSaveFileDialogAsync(string defaultName, string filter, Func<string, Task> onFileSelected)
    {
        var result = fileDialog.SaveFile("Save File", defaultName, filter);
        if (result != null)
        {
            await onFileSelected(result);
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

        application.Exit(0);

        // Changed from System.Windows.Forms.Application.Restart() to solve https://github.com/Kizari/Flagrum/issues/81
        // Keeping this comment here because it's worth noting that Application.Restart would replay the startup args
        Process.Start(executablePath);
    }

    public string? GetFmodPath() => application.FmodPath;

    public void ClearFmodPath()
    {
        application.FmodPath = null;
    }

    public Task SetClipboardTextAsync(string text)
    {
        application.SetClipboardText(text);
        return Task.CompletedTask;
    }

    public void RefreshPatreonButton()
    {
        // TODO: Implement this
    }
}