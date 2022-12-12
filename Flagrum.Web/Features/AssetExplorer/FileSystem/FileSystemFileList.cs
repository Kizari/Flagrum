using System.IO;
using Flagrum.Web.Features.AssetExplorer.Base;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Web.Features.AssetExplorer.FileSystem;

public class FileSystemFileList : FileList
{
    protected override void OnInitialized()
    {
        var path = AppState.GetCurrentAssetExplorerPath(Context);
        AssetExplorer.AddressBar.SetCurrentPath(path);
        SetCurrentNode(new FileSystemNode(path));
    }

    protected override void PersistCurrentNode()
    {
        Context.SetString(StateKey.CurrentAssetExplorerPath, CurrentNode.Path);
    }
}