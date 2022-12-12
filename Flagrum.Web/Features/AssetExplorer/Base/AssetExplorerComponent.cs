using System;
using Flagrum.Web.Persistence;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public class AssetExplorerComponent : ComponentBase
{
    [CascadingParameter] public IAssetExplorerParent Parent { get; set; }

    [Inject] protected IStringLocalizer<Index> Localizer { get; set; }

    [Inject] protected IWpfService WpfService { get; set; }

    [Inject] protected FlagrumDbContext Context { get; set; }

    [Inject] protected AppStateService AppState { get; set; }
}