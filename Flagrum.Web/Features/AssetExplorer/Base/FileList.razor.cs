using Flagrum.Web.Persistence.Entities;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public abstract partial class FileList
{
    protected RenderFragment FileListHeaderTemplate { get; set; }

    public void SetCurrentNode(AssetExplorerNode node)
    {
        AppState.Node = node;
        StateHasChanged();
    }
}