using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public abstract partial class AddressBar
{
    [CascadingParameter(Name = "AssetExplorer")]
    public AssetExplorer AssetExplorer { get; set; }

    protected string CurrentPath { get; set; }

    public abstract void NavigateToCurrentPath();
    protected abstract string GetPersistedPath();

    protected override void OnInitialized()
    {
        CurrentPath = AppState.Node?.GetUri(Context);
    }

    private void CheckEnter(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter")
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
        if (AppState.Node.Parent != null)
        {
            AssetExplorer.FileList.SetCurrentNode(AppState.Node.Parent);
            SetCurrentPath(AppState.Node.GetUri(Context));
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

        CurrentPath = GetPersistedPath();
        Parent.CallStateHasChanged();
    }
}