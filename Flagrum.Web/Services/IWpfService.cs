using System;

namespace Flagrum.Web.Services;

public interface IWpfService
{
    void OpenFileDialog(string filter, Action<string> onFileSelected);
}