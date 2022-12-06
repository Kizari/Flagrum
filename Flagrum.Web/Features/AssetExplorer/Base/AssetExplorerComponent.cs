using System;
using Flagrum.Web.Persistence;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public class AssetExplorerComponent : ComponentBase
{
    [CascadingParameter] public Index2 Parent { get; set; }

    [Inject] protected IStringLocalizer<Index> Localizer { get; set; }

    [Inject] protected IWpfService WpfService { get; set; }

    [Inject] protected FlagrumDbContext Context { get; set; }

    [Inject] protected AppStateService AppState { get; set; }

    protected RenderFragment RenderComponent<TComponent>(Action<object> setReference = null)
    {
        return builder =>
        {
            builder.OpenComponent(0, typeof(TComponent));

            if (setReference != null)
            {
                builder.AddComponentReferenceCapture(1, setReference);
            }

            builder.CloseComponent();
        };
    }
}