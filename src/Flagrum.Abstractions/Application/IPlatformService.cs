using Flagrum.Abstractions.AssetExplorer;

namespace Flagrum.Abstractions;

public interface IPlatformService
{
    Task OpenFileDialogAsync(string filter, Func<string, Task> onFileSelected);
    Task OpenFolderDialogAsync(string initialDirectory, Func<string, Task> onFolderSelected);
    Task OpenSaveFileDialogAsync(string defaultName, string filter, Func<string, Task> onFileSelected);
    Version GetVersion();
    void Restart();
    void Resize3DViewport(int left, int top, int width, int height);
    void Set3DViewportVisibility(bool isVisible);
    int ChangeModel(IAssetExplorerNode gmdlNode, AssetExplorerView view, int lodLevel);

    void Update3DViewportBindings(ModifierKeys rotateModifierKey, MouseAction rotateMouseAction,
        ModifierKeys panModifierKey, MouseAction panMouseAction);

    string GetFmodPath();
    void ClearFmodPath();
    void SetClipboardText(string text);
    void RefreshPatreonButton();
}