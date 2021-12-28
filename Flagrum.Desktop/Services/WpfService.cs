using System;
using System.Threading.Tasks;
using Flagrum.Web.Services;
using Microsoft.Win32;

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

    public Version GetVersion()
    {
        return typeof(WpfService).Assembly.GetName().Version;
    }
}