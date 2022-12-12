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
                var uri = Context.AssetExplorerNodes.FirstOrDefault(n => n.Id == nodeId)?.GetUri() ?? "data://";
                var processedUri = uri.Trim().Replace("data://", "data:/").TrimEnd('/');
                var tokens = processedUri.Split('/')
                    .Select(t => t.ToLower())
                    .ToList();

                var currentNode = AppState.RootGameViewNode;
                for (var i = 1; i < tokens.Count; i++)
                {
                    currentNode = (AssetExplorerNode)currentNode.Children.FirstOrDefault(n => n.Name == tokens[i]);
                }
                
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