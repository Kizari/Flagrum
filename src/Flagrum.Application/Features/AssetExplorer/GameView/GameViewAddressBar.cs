using System.Linq;
using Flagrum.Application.Features.AssetExplorer.Base;
using FileIndexNode = Flagrum.Application.Features.AssetExplorer.Indexing.FileIndexNode;

namespace Flagrum.Application.Features.AssetExplorer.GameView;

public class GameViewAddressBar : AddressBar
{
    protected override bool IsDisabled => FileIndex.IsRegenerating;

    public override void NavigateToCurrentPath()
    {
        if (CurrentPath == null || CurrentPath.Trim() == "")
        {
            AssetExplorer.FileList.SetCurrentNode(FileIndex.RootNode);
            return;
        }

        var processedUri = CurrentPath.Trim().Replace("://", ":/").TrimEnd('/');
        var tokens = processedUri.Split('/')
            .Select(t => t.ToLower())
            .ToList();

        var currentNode = FileIndex.RootNode;
        foreach (var token in tokens)
        {
            currentNode = (FileIndexNode)currentNode.Children.FirstOrDefault(n => n.Name == token);

            if (currentNode == null)
            {
                if (AppState.Is3DViewerOpen)
                {
                    PlatformService.Set3DViewportVisibility(false);
                }

                Parent.Alert.Open("Error", "Invalid URI", "Nothing was found at the given address.",
                    () => { PlatformService.Set3DViewportVisibility(true); });

                return;
            }
        }

        if (!currentNode.HasChildren)
        {
            var parent = currentNode.Parent;
            AssetExplorer.FileList.SetCurrentNode(parent);

            if (AssetExplorer.ItemSelectedOverride == null)
            {
                AssetExplorer.Preview.SetItem(currentNode);
            }
            else
            {
                AssetExplorer.ItemSelectedOverride(currentNode);
            }
        }
        else
        {
            AssetExplorer.FileList.SetCurrentNode(currentNode);
        }
    }
}