using Flagrum.Abstractions;
using Flagrum.Application.Features.AssetExplorer.Base;
using Flagrum.Application.Features.AssetExplorer.Data;

namespace Flagrum.Application.Features.AssetExplorer.FileSystem;

public class FileSystemFileList : FileList
{
    protected override void OnInitialized()
    {
        var path = AppState.GetCurrentAssetExplorerPath();
        AssetExplorer.AddressBar.SetCurrentPath(path);
        SetCurrentNode(new FileSystemNode(path));
    }

    protected override void PersistCurrentNode()
    {
        Configuration.Set(StateKey.CurrentAssetExplorerPath, CurrentNode.Path);
    }
}