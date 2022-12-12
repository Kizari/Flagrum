using Flagrum.Web.Features.AssetExplorer.Data;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public abstract partial class FileList
{
    [CascadingParameter(Name = "AssetExplorer")]
    public AssetExplorer AssetExplorer { get; set; }

    public IAssetExplorerNode CurrentNode { get; set; }
    public ExplorerTreeView ExplorerTreeView { get; set; }

    protected abstract void PersistCurrentNode();

    public void SetCurrentNode(IAssetExplorerNode node)
    {
        CurrentNode = node;
        Parent.CallStateHasChanged();
        PersistCurrentNode();
    }
}