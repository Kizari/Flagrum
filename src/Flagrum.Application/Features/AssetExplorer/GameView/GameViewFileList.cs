using System.Linq;
using System.Timers;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Features.AssetExplorer.Base;
using FileIndexNode = Flagrum.Application.Features.AssetExplorer.Indexing.FileIndexNode;

namespace Flagrum.Application.Features.AssetExplorer.GameView;

public class GameViewFileList : FileList
{
    private Timer _timer;

    protected override void OnInitialized()
    {
        if (FileIndex.IsRegenerating)
        {
            FileIndex.OnIsRegeneratingChanged += isRegenerating =>
            {
                if (isRegenerating)
                {
                    CurrentNode = null;
                    AssetExplorer.CallStateHasChanged();
                }
                else
                {
                    InvokeAsync(() =>
                    {
                        CurrentNode = AppState.CurrentGameViewNode ?? FileIndex.RootNode;
                        AssetExplorer.AddressBar.SetCurrentPath(CurrentNode.Path);
                        AssetExplorer.CallStateHasChanged();
                    });
                }
            };
        }
        else
        {
            try
            {
                var uri = Configuration.Get<string>(StateKey.CurrentAssetNode);
                if (uri != null)
                {
                    var processedUri = uri.Trim().Replace("://", ":/").TrimEnd('/');
                    var tokens = processedUri.Split('/')
                        .Where(t => t != string.Empty)
                        .Select(t => t.ToLower())
                        .ToList();

                    var currentNode = tokens.Aggregate(FileIndex.RootNode,
                        (current, token) => (FileIndexNode)current!.Children
                            .FirstOrDefault(n => n.Name == token));

                    SetCurrentNode(currentNode!.HasChildren ? currentNode : currentNode.Parent);
                    AssetExplorer.AddressBar.SetCurrentPath(uri);
                }
                else
                {
                    SetCurrentNode(FileIndex.RootNode);
                }
            }
            catch
            {
                SetCurrentNode(FileIndex.RootNode);
            }
        }
    }

    protected override void PersistCurrentNode()
    {
        if (CurrentNode != null)
        {
            var currentNode = CurrentNode.Type == ExplorerItemType.Directory ? CurrentNode : CurrentNode.Parent;
            Configuration.Set(StateKey.CurrentAssetNode, ((FileIndexNode)currentNode).Path);
        }
    }
}