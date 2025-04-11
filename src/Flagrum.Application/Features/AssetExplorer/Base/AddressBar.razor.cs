using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Flagrum.Application.Features.AssetExplorer.Base;

public abstract partial class AddressBar
{
    [CascadingParameter(Name = "AssetExplorer")]
    public AssetExplorer AssetExplorer { get; set; }

    public string CurrentPath { get; protected set; } = "";
    private bool AddressBarSelect { get; set; }
    protected abstract bool IsDisabled { get; }

    public abstract void NavigateToCurrentPath();

    protected override void OnInitialized()
    {
        AddressBarSelect = Configuration.Get<bool>(StateKey.AssetExplorerAddressBarSelect);
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
            PlatformService.Set3DViewportVisibility(false);
        }

        Parent.CurrentView = (AssetExplorerView)view;
        Configuration.Set(StateKey.CurrentAssetExplorerView, view);
        Parent.CallStateHasChanged();
    }
}