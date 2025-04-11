using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Components.Modals;

namespace Flagrum.Application.Features.AssetExplorer.Base;

public interface IAssetExplorerParent
{
    AssetExplorerView CurrentView { get; set; }
    AlertModal Alert { get; set; }
    void CallStateHasChanged();
}