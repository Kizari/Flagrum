using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Flagrum.Application.Features.AssetExplorer.Base;

public class AssetExplorerComponent : ComponentBase
{
    [CascadingParameter] public IAssetExplorerParent Parent { get; set; }

    [Inject] protected IStringLocalizer<Index> Localizer { get; set; }

    [Inject] protected IApplication Application { get; set; }

    [Inject] protected IFileIndex FileIndex { get; set; }

    [Inject] protected AppStateService AppState { get; set; }

    [Inject] protected IConfiguration Configuration { get; set; }
}