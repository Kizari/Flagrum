using System.Reflection;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Utilities;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application;

public partial class App
{
    [Inject] private IFileIndex FileIndex { get; set; }

    private Assembly[] AdditionalAssemblies { get; set; }

    protected override void OnInitialized()
    {
        // Register dynamically loaded pages from the premium assembly with the router
        if (PremiumHelper.Instance.Assembly != null)
        {
            AdditionalAssemblies = [PremiumHelper.Instance.Assembly];
        }

        // Ensure app state is updated when file indexing begins/ends
        FileIndex.OnIsRegeneratingChanged += _ => InvokeAsync(StateHasChanged);
    }
}