using Flagrum.Web.Features.AssetExplorer.Base;

namespace Flagrum.Web.Features.AssetExplorer.FileSystem;

public class FileSystemAssetExplorer : Base.AssetExplorer
{
    protected override void OnInitialized()
    {
        AddressBarTemplate = RenderComponent<FileSystemAddressBar>(r => AddressBar = (AddressBar)r);
        FileListTemplate = RenderComponent<FileSystemFileList>(r => FileList = (FileList)r);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            AddressBar.SetCurrentPath(AppState.GetCurrentAssetExplorerPath());
        }
    }
}