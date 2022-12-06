using Flagrum.Web.Features.AssetExplorer.Base;

namespace Flagrum.Web.Features.AssetExplorer.FileSystem;

public class FileSystemFileList : FileList
{
    protected override void OnInitialized()
    {
        FileListHeaderTemplate = RenderComponent<FileSystemFileListHeader>();
    }
}