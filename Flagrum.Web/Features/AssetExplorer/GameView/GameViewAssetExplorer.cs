using System.Linq;
using System.Timers;
using Flagrum.Web.Features.AssetExplorer.Base;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Web.Features.AssetExplorer.GameView;

public class GameViewAssetExplorer : Base.AssetExplorer
{
    private Timer _timer;
    
    protected override void OnInitialized()
    {
        AddressBarTemplate = RenderComponent<GameViewAddressBar>(r => AddressBar = (AddressBar)r);
        FileListTemplate = RenderComponent<GameViewFileList>(r => FileList = (FileList)r);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (AppState.Node == null)
            {
                _timer = new Timer(1000);
                _timer.Elapsed += (_, _) =>
                {
                    if (AppState.Node != null)
                    {
                        _timer.Stop();
                        InvokeAsync(StateHasChanged);
                    }
                };
                _timer.Start();
            }
            else
            {
                var nodeId = Context.GetInt(StateKey.CurrentAssetNode);
                if (nodeId > 0)
                {
                    AppState.Node = Context.AssetExplorerNodes
                        .FirstOrDefault(n => n.Id == nodeId);
                    AddressBar.SetCurrentPath(AppState.Node?.GetUri(Context));
                    AddressBar.NavigateToCurrentPath();
                }
            }
        }
    }
}