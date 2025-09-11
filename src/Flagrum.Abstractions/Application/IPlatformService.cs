namespace Flagrum.Abstractions;

public interface IPlatformService
{
    Task OpenFileDialogAsync(string filter, Func<string, Task> onFileSelected);
    Task OpenFolderDialogAsync(string initialDirectory, Func<string, Task> onFolderSelected);
    Task OpenSaveFileDialogAsync(string defaultName, string filter, Func<string, Task> onFileSelected);
    Version GetVersion();
    void Restart();
    string GetFmodPath();
    void ClearFmodPath();
    Task SetClipboardTextAsync(string text);
    void RefreshPatreonButton();
}