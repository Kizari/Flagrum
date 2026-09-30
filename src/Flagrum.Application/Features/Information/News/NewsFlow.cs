using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application.Features.Information.News;

/// <summary>
/// Handles presenting a flow of news modals.
/// </summary>
/// <param name="modals">Modals to present, in order they should appear.</param>
public class NewsFlow(
    IProfileService profile,
    IApplication application,
    List<NewsModalContext> modals)
{
    /// <summary>
    /// Renders the modals in the target page.
    /// </summary>
    public required RenderFragment RenderFragment { get; init; }

    /// <summary>
    /// Presents the modals in order, until they have all been shown.
    /// </summary>
    public Task ExecuteAsync()
    {
        // Skip showing news if user has already seen news for the current application version
        if (profile.LastVersion == application.Version)
        {
            return Task.CompletedTask;
        }

        // Clear build cache in case any build changes have been made in the new version that need to be applied
        foreach (var file in Directory.EnumerateFiles(profile.CacheDirectory))
        {
            File.Delete(file);
        }

        // Link modals together
        for (var i = 0; i < modals.Count; i++)
        {
            var modal = modals[i];
            modal.Next = i + 1 < modals.Count ? modals[i + 1] : null;
        }

        // Start the flow by presenting the first modal
        return modals[0].Reference!.PresentAsync();
    }
}