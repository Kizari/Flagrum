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
        var processedUri = CurrentPath.Trim().Replace("data://", "data:/").TrimEnd('/');
        var tokens = processedUri.Split('/')
            .Select(t => t.ToLower())
            .ToList();

        if (tokens[0] == "data:")
        {
            var currentNode = AppState.RootGameViewNode;
            for (var i = 1; i < tokens.Count; i++)
            {
                currentNode = (AssetExplorerNode)currentNode.Children.FirstOrDefault(n => n.Name == tokens[i]);

                if (currentNode == null)
                {
                    Parent.Alert.Open("Error", "Invalid URI", "Nothing was found at the given address.", null);
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
        else
        {
            Parent.Alert.Open("Error", "Invalid URI", "Nothing was found at the given address.", null);
        }
    }
}