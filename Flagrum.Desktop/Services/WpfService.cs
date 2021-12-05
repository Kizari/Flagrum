using System;
using Flagrum.Web.Services;
using Microsoft.Win32;

namespace Flagrum.Desktop.Services;

public class WpfService : IWpfService
{
    public void OpenFileDialog(string filter, Action<string> onFileSelected)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter
        };

        if (dialog.ShowDialog() == true)
        {
            onFileSelected(dialog.FileName);
        }
    }
}