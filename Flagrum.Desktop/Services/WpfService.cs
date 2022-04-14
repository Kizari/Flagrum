using System;
using System.Threading.Tasks;
using Flagrum.Web.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Flagrum.Desktop.Services;

public class WpfService : IWpfService
{
    private readonly MainWindow _window;

    public WpfService(MainWindow window)
    {
        _window = window;
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
}