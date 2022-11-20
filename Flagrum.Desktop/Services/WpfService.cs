using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Flagrum.Web.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Flagrum.Desktop.Services;

public class WpfService : IWpfService
{
    private readonly MainViewModel _mainViewModel;
    private readonly MainWindow _window;

    public WpfService(MainWindow window)
    {
        _window = window;
        _mainViewModel = (MainViewModel)window.DataContext;
    }

    public async Task OpenFileDialogAsync(string filter, Action<string> onFileSelected)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter
        };

        var result = await _window.Dispatcher.InvokeAsync(() => dialog.ShowDialog());

        if (result == true)
        {
            onFileSelected(dialog.FileName);
        }
    }

    public async Task OpenFolderDialogAsync(string initialDirectory, Action<string> onFolderSelected)
    {
        var dialog = new CommonOpenFileDialog
        {
            InitialDirectory = initialDirectory,
            IsFolderPicker = true
        };

        var result = await _window.Dispatcher.InvokeAsync(() => dialog.ShowDialog());

        if (result == CommonFileDialogResult.Ok)
        {
            onFolderSelected(dialog.FileName);
        }
    }

    public async Task OpenSaveFileDialogAsync(string defaultName, string filter, Action<string> onFileSelected)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultName
        };

        var result = await _window.Dispatcher.InvokeAsync(() => dialog.ShowDialog());

        if (result == true)
        {
            onFileSelected(dialog.FileName);
        }
    }

    public Version GetVersion()
    {
        return typeof(WpfService).Assembly.GetName().Version;
    }

    public void ShowWindowsNotification(string message)
    {
        new ToastContentBuilder()
            .AddText(message)
            .Show();
    }

    public void Restart()
    {
        Application.Current.Shutdown(0);
        System.Windows.Forms.Application.Restart();
    }

    public void Resize3DViewport(int left, int top, int width, int height)
    {
        _mainViewModel.ViewportLeft = left;
        _mainViewModel.ViewportTop = top;
        _mainViewModel.ViewportWidth = width;
        _mainViewModel.ViewportHeight = height;
    }

    public void Update3DViewportBindings(string rotateModifierKey, string rotateMouseAction, string panModifierKey,
        string panMouseAction)
    {
        _mainViewModel.ViewportRotateGesture = new MouseGesture(
            Enum.Parse<MouseAction>(rotateMouseAction),
            Enum.Parse<ModifierKeys>(rotateModifierKey));

        _mainViewModel.ViewportPanGesture = new MouseGesture(
            Enum.Parse<MouseAction>(panMouseAction),
            Enum.Parse<ModifierKeys>(panModifierKey));
    }

    public void Set3DViewportVisibility(bool isVisible)
    {
        _mainViewModel.IsViewportVisible = isVisible;
    }

    public async Task ChangeModel(string gmdlUri)
    {
        await _mainViewModel.ViewportHelper.ChangeModel(gmdlUri);
    }

    public IEnumerable<string> GetModifierKeys()
    {
        return Enum.GetNames<ModifierKeys>();
    }

    public IEnumerable<string> GetMouseActions()
    {
        return Enum.GetNames<MouseAction>();
    }

    public string? GetFmodPath()
    {
        return _mainViewModel.FmodPath;
    }

    public void ClearFmodPath()
    {
        _mainViewModel.FmodPath = null;
    }
}