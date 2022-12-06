using Flagrum.Web.Features.AssetExplorer.Base;

namespace Flagrum.Web.Features.AssetExplorer.GameView;

public class GameViewFileList : FileList
{
    protected override void OnInitialized()
    {
        FileListHeaderTemplate = RenderComponent<GameViewFileListHeader>();
    }
}