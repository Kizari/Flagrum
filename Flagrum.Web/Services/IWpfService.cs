using System;
using System.Threading.Tasks;

namespace Flagrum.Web.Services;

public static class WpfServiceHelper
{
    public static Action<Action> OnAppUpdated { get; set; }
}

public interface IWpfService
{
    Task OpenFileDialogAsync(string filter, Action<string> onFileSelected);
    Task OpenFolderDialogAsync(string initialDirectory, Action<string> onFolderSelected);
    Task OpenSaveFileDialogAsync(string defaultName, string filter, Action<string> onFileSelected);
    Version GetVersion();
}