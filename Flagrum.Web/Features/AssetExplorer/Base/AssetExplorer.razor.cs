using System;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Features.AssetExplorer.Export;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public partial class AssetExplorer
{
    [Parameter] public RenderFragment AddressBarTemplate { get; set; }
    [Parameter] public RenderFragment FileListTemplate { get; set; }
    [Parameter] public Action<IAssetExplorerNode> ItemSelectedOverride { get; set; }

    public AddressBar AddressBar { get; set; }
    public FileList FileList { get; set; }
    public Preview Preview { get; set; }
    public ExportContextMenu ExportContextMenu { get; set; }

    public IAssetExplorerNode ContextNode { get; set; }
    public FileListLayout CurrentLayout { get; set; }

    protected bool IsLoading { get; set; }
    protected string LoadingMessage { get; set; }

    private string FileListHeaderDisplayName
    {
        get
        {
            if (CurrentLayout == FileListLayout.ListView)
            {
                return FileList?.CurrentNode == null ? "data://" : FileList.CurrentNode.Name;
            }

            return Parent.CurrentView == AssetExplorerView.FileSystem ? "This PC" : "data://";
        }
    }

    protected override void OnInitialized()
    {
        CurrentLayout = Context.GetEnum<FileListLayout>(StateKey.CurrentAssetExplorerLayout);
    }

    public void SetLoading(bool isLoading, string loadingMessage = null)
    {
        IsLoading = isLoading;
        LoadingMessage = loadingMessage;
        StateHasChanged();
    }

    public void SetCurrentLayout(FileListLayout layout)
    {
        CurrentLayout = layout;
        StateHasChanged();
        Context.SetEnum(StateKey.CurrentAssetExplorerLayout, layout);
    }

    public void CallStateHasChanged() => StateHasChanged();
}