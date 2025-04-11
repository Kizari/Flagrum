using Flagrum.Abstractions.AssetExplorer;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application.Features.AssetExplorer.Base;

public partial class ExplorerRow
{
    [CascadingParameter(Name = "AssetExplorer")]
    public AssetExplorer AssetExplorer { get; set; }

    [CascadingParameter] public ExplorerListView Parent { get; set; }

    [Parameter] public IAssetExplorerNode Node { get; set; }

    private void OnClick(IAssetExplorerNode node)
    {
        AssetExplorer.AddressBar.SetCurrentPath(node.Path);

        if (node.HasChildren)
        {
            AssetExplorer.FileList.SetCurrentNode(node);
        }
        else
        {
            if (AssetExplorer.ItemSelectedOverride == null)
            {
                AssetExplorer.Preview.SetItem(node);
            }
            else
            {
                AssetExplorer.ItemSelectedOverride(node);
            }
        }
    }
}