using System.Linq;
using Flagrum.Web.Features.AssetExplorer.Base;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Features.AssetExplorer.GameView;

public class GameViewAddressBar : AddressBar
{
    public override void NavigateToCurrentPath()
    {
        var processedUri = CurrentPath.Trim().Replace("data://", "data:/");
        if (processedUri[^1] == '/')
        {
            processedUri = processedUri[..^1];
        }

        var tokens = processedUri.Split('/')
            .Select(t => t.ToLower())
            .ToList();

        if (tokens[0] == "data:")
        {
            var currentNode = Context.AssetExplorerNodes.Include(n => n.Children).First();
            for (var i = 1; i < tokens.Count; i++)
            {
                currentNode = Context.AssetExplorerNodes.Include(n => n.Children)
                    .FirstOrDefault(n => n.ParentId == currentNode.Id && n.Name == tokens[i]);

                if (currentNode == null)
                {
                    Parent.Alert.Open("Error", "Invalid URI", "Nothing was found at the given address.", null);
                    return;
                }
            }

            if (!Context.AssetExplorerNodes.Any(n => n.Id == currentNode.Id && n.Children.Any()))
            {
                var parent = Context.AssetExplorerNodes.First(n => n.Id == currentNode.ParentId);
                AssetExplorer.FileList.SetCurrentNode(parent);
                AssetExplorer.Preview.SetItem(currentNode);
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

    protected override string GetPersistedPath()
    {
        return AppState.Node == null ? "data://" : AppState.Node.GetUri(Context);
    }
}