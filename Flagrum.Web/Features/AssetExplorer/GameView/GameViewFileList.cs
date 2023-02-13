using System.Linq;
using System.Timers;
using Flagrum.Web.Features.AssetExplorer.Base;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Web.Features.AssetExplorer.GameView;

public class GameViewFileList : FileList
{
    private Timer _timer;

    protected override void OnInitialized()
    {
        if (AppState.RootGameViewNode == null)
        {
            _timer = new Timer(1000);
            _timer.Elapsed += (_, _) =>
            {
                if (AppState.RootGameViewNode != null)
                {
                    InvokeAsync(() =>
                    {
                        CurrentNode = AppState.CurrentGameViewNode ?? AppState.RootGameViewNode;
                        AssetExplorer.AddressBar.SetCurrentPath(CurrentNode.Path);
                        AssetExplorer.CallStateHasChanged();
                    });

                    _timer.Stop();
                }
            };
            _timer.Start();
        }
        else
        {
            var nodeId = Context.GetInt(StateKey.CurrentAssetNode);
            if (nodeId > 0)
            {
                var uri = Context.AssetExplorerNodes.FirstOrDefault(n => n.Id == nodeId)?.GetUri() ?? "";
                var processedUri = uri.Trim().Replace("://", ":/").TrimEnd('/');
                var tokens = processedUri.Split('/')
                    .Where(t => t != string.Empty)
                    .Select(t => t.ToLower())
                    .ToList();

                var currentNode = tokens.Aggregate(AppState.RootGameViewNode,
                    (current, token) => (AssetExplorerNode)current!.Children
                        .FirstOrDefault(n => n.Name == token));

                SetCurrentNode(currentNode!.HasChildren ? currentNode : currentNode.Parent);
                AssetExplorer.AddressBar.SetCurrentPath(uri);
            }
        }
    }

    protected override void PersistCurrentNode()
    {
        if (CurrentNode != null)
        {
            var currentNode = CurrentNode.Type == ExplorerItemType.Directory ? CurrentNode : CurrentNode.Parent;
            Context.SetInt(StateKey.CurrentAssetNode, ((AssetExplorerNode)currentNode).Id);
        }
    }
}