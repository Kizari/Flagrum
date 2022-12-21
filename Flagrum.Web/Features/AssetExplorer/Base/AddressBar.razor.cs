using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public abstract partial class AddressBar
{
    [CascadingParameter(Name = "AssetExplorer")]
    public AssetExplorer AssetExplorer { get; set; }

    public string CurrentPath { get; protected set; } = "data://";
    private bool AddressBarSelect { get; set; }

    public abstract void NavigateToCurrentPath();
    protected abstract bool IsDisabled { get; }

    protected override void OnInitialized()
    {
        AddressBarSelect = Context.GetBool(StateKey.AssetExplorerAddressBarSelect);
    }

    private void CheckEnter(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter" && !IsDisabled)
        {
            NavigateToCurrentPath();
        }
    }

    public void SetCurrentPath(string path)
    {
        CurrentPath = path;
        StateHasChanged();
    }

    private void Up()
    {
        if (AssetExplorer.FileList.CurrentNode.Parent != null)
        {
            AssetExplorer.FileList.SetCurrentNode(AssetExplorer.FileList.CurrentNode.Parent);
            SetCurrentPath(AssetExplorer.FileList.CurrentNode.Path);
        }
    }

    private void OnViewChanged(int view)
    {
        if (AppState.Is3DViewerOpen)
        {
            WpfService.Set3DViewportVisibility(false);
        }

        Parent.CurrentView = (AssetExplorerView)view;
        Context.SetEnum(StateKey.CurrentAssetExplorerView, view);
        Parent.CallStateHasChanged();
    }
}