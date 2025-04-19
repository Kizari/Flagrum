using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Main;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Input_ModifierKeys = System.Windows.Input.ModifierKeys;
using Input_MouseAction = System.Windows.Input.MouseAction;
using ModifierKeys = Flagrum.Abstractions.AssetExplorer.ModifierKeys;
using MouseAction = Flagrum.Abstractions.AssetExplorer.MouseAction;

namespace Flagrum.Services;

public class PlatformService : IPlatformService
{
    public MainViewModel Main { get; set; } = null!;
    public ViewportViewModel Viewport { get; set; } = null!;

    public async Task OpenFileDialogAsync(string filter, Func<string, Task> onFileSelected)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter
        };

        var result = await App.Current.Dispatcher.InvokeAsync(() => dialog.ShowDialog());

        if (result == true)
        {
            await onFileSelected(dialog.FileName);
        }
    }

    public async Task OpenFolderDialogAsync(string initialDirectory, Func<string, Task> onFolderSelected)
    {
        var dialog = new CommonOpenFileDialog
        {
            InitialDirectory = initialDirectory,
            IsFolderPicker = true
        };

        var result = await App.Current.Dispatcher.InvokeAsync(() => dialog.ShowDialog());

        if (result == CommonFileDialogResult.Ok)
        {
            await onFolderSelected(dialog.FileName);
        }
    }

    public async Task OpenSaveFileDialogAsync(string defaultName, string filter, Func<string, Task> onFileSelected)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultName
        };

        var result = await App.Current.Dispatcher.InvokeAsync(() => dialog.ShowDialog());

        if (result == true)
        {
            await onFileSelected(dialog.FileName);
        }
    }

    public Version GetVersion() => typeof(PlatformService).Assembly.GetName().Version!;

    public void Restart()
    {
        App.Current.Shutdown(0);

        // Changed from System.Windows.Forms.Application.Restart() to solve https://github.com/Kizari/Flagrum/issues/81
        // Keeping this comment here because it's worth noting that Application.Restart would replay the startup args
        Process.Start(System.Windows.Forms.Application.ExecutablePath);
    }

    public void Resize3DViewport(int left, int top, int width, int height)
    {
        Viewport.ViewportLeft = left;
        Viewport.ViewportTop = top;
        Viewport.ViewportWidth = width;
        Viewport.ViewportHeight = height;
    }

    public void Update3DViewportBindings(ModifierKeys rotateModifierKey, MouseAction rotateMouseAction,
        ModifierKeys panModifierKey,
        MouseAction panMouseAction)
    {
        Viewport.ViewportRotateGesture = new MouseGesture(
            (Input_MouseAction)rotateMouseAction,
            (Input_ModifierKeys)rotateModifierKey);

        Viewport.ViewportPanGesture = new MouseGesture(
            (Input_MouseAction)panMouseAction,
            (Input_ModifierKeys)panModifierKey);
    }

    public void Set3DViewportVisibility(bool isVisible)
    {
        Viewport.IsViewportVisible = isVisible;
    }

    public int ChangeModel(IAssetExplorerNode gmdlNode, AssetExplorerView view, int lodLevel) =>
        Viewport.ViewportHelper!.ChangeModel(gmdlNode, view, lodLevel);

    public string? GetFmodPath() => Main.FmodPath;

    public void ClearFmodPath()
    {
        Main.FmodPath = null;
    }

    public void SetClipboardText(string text)
    {
        Clipboard.SetText(text);
    }

    public void RefreshPatreonButton()
    {
        Main.RefreshPatreonButton();
    }
}