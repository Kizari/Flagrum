using System.Linq;
using Flagrum.Web.Features.AssetExplorer.Base;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;

namespace Flagrum.Web.Features.AssetExplorer.GameView;

public class GameViewAddressBar : AddressBar
{
    protected override bool IsDisabled => AppState.RootGameViewNode == null;

    public override void NavigateToCurrentPath()
    {
        if (CurrentPath == null || CurrentPath.Trim() == "")
        {
            AssetExplorer.FileList.SetCurrentNode(AppState.RootGameViewNode);
            return;
        }
        
        var processedUri = CurrentPath.Trim().Replace("://", ":/").TrimEnd('/');
        var tokens = processedUri.Split('/')
            .Select(t => t.ToLower())
            .ToList();

        var currentNode = AppState.RootGameViewNode;
        foreach (var token in tokens)
        {
            currentNode = (AssetExplorerNode)currentNode.Children.FirstOrDefault(n => n.Name == token);

            if (currentNode == null)
            {
                if (AppState.Is3DViewerOpen)
                {
                    WpfService.Set3DViewportVisibility(false);
                }
                
                Parent.Alert.Open("Error", "Invalid URI", "Nothing was found at the given address.", () =>
                {
                    WpfService.Set3DViewportVisibility(true);
                });
                
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