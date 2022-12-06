using System.Linq;
using Flagrum.Web.Persistence.Entities;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public partial class ExplorerRow
{
    [CascadingParameter(Name = "AssetExplorer")]
    public AssetExplorer AssetExplorer { get; set; }

    [CascadingParameter] public ExplorerView Parent { get; set; }

    [Parameter] public AssetExplorerNode Node { get; set; }

    private void OnClick(AssetExplorerNode node)
    {
        AssetExplorer.AddressBar.SetCurrentPath(node.GetUri(Context));
        
        if (Context.AssetExplorerNodes.Any(n => n.ParentId == node.Id))
        {
            AssetExplorer.FileList.SetCurrentNode(node);
        }
        else
        {
            AssetExplorer.Preview.SetItem(node);
        }
    }
}