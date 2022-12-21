using Flagrum.Web.Components.Modals;
using Flagrum.Web.Features.AssetExplorer.Data;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public interface IAssetExplorerParent
{
    AssetExplorerView CurrentView { get; set; }
    AlertModal Alert { get; set; }
    void CallStateHasChanged();
}