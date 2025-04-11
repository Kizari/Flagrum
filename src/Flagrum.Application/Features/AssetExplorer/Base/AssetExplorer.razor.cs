using System;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Features.AssetExplorer.Data;
using Flagrum.Application.Features.AssetExplorer.Export;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application.Features.AssetExplorer.Base;

public partial class AssetExplorer
{
    [Parameter] public RenderFragment AddressBarTemplate { get; set; }
    [Parameter] public RenderFragment FileListTemplate { get; set; }
    [Parameter] public Action<IAssetExplorerNode> ItemSelectedOverride { get; set; }
    [Parameter] public Func<IAssetExplorerNode, bool> ItemFilter { get; set; } = _ => true;

    public AddressBar AddressBar { get; set; }
    public FileList FileList { get; set; }
    public Preview Preview { get; set; }
    public ExportContextMenu ExportContextMenu { get; set; }

    public IAssetExplorerNode ContextNode { get; set; }
    public FileListLayout CurrentLayout { get; set; }

    protected bool IsLoading { get; set; }
    protected string LoadingMessage { get; set; }
    private int PreviewType { get; set; }

    private string FileListHeaderDisplayName
    {
        get
        {
            if (CurrentLayout == FileListLayout.ListView)
            {
                return FileList?.CurrentNode == null || FileList?.CurrentNode.Name == ""
                    ? "Game View"
                    : FileList.CurrentNode.Name;
            }

            return Parent.CurrentView == AssetExplorerView.FileSystem ? "This PC" : "Game View";
        }
    }

    protected override void OnInitialized()
    {
        CurrentLayout = Configuration.Get<FileListLayout>(StateKey.CurrentAssetExplorerLayout);
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
        Configuration.Set(StateKey.CurrentAssetExplorerLayout, layout);
    }

    public void SetPreviewType(int type)
    {
        PreviewType = type;
        StateHasChanged();
    }

    public void CallStateHasChanged()
    {
        StateHasChanged();
    }
}