using System;
using System.Collections.Generic;
using Flagrum.Abstractions;

namespace Flagrum.Application.Features.Information.News;

/// <summary>
/// Creates and handles the flow of the application through a series of informational popups on application start.
/// </summary>
/// <remarks>
/// This is used to show important news to the user, and the latest patch notes.
/// </remarks>
public class NewsFlowBuilder(
    IProfileService profile,
    IApplication application)
{
    private readonly List<NewsModalContext> _modals = [];

    /// <summary>
    /// Adds a modal to the flow if the user hasn't seen the app since before the given version.
    /// </summary>
    /// <param name="version">Version that the user must have been below to see the modal.</param>
    /// <typeparam name="TModal">Type of the modal to display.</typeparam>
    /// <returns>This builder.</returns>
    public NewsFlowBuilder PresentIfBelow<TModal>(Version version) where TModal : NewsModal
    {
        _modals.Add(new NewsModalContext
        {
            Type = typeof(TModal),
            Predicate = (p, _) => p.LastVersion < version,
            Profile = profile,
            Application = application
        });

        return this;
    }

    /// <summary>
    /// Adds a modal to the flow that will be presented unconditionally.
    /// </summary>
    /// <returns>This builder.</returns>
    public NewsFlowBuilder PresentAlways<TModal>() where TModal : NewsModal
    {
        _modals.Add(new NewsModalContext
        {
            Type = typeof(TModal),
            Predicate = (_, _) => true,
            Profile = profile,
            Application = application
        });

        return this;
    }

    /// <summary>
    /// Builds the news flow from the configured builder.
    /// </summary>
    /// <returns>The newly created news flow.</returns>
    public NewsFlow Build() => new(profile, application, _modals)
    {
        RenderFragment = builder =>
        {
            for (var i = 0; i < _modals.Count; i++)
            {
                var modal = _modals[i];
                builder.OpenComponent(i, modal.Type);
                builder.AddComponentParameter(i, nameof(NewsModal.Context), modal);
                builder.AddComponentReferenceCapture(i, component => modal.Reference = (NewsModal)component);
                builder.CloseComponent();
            }
        }
    };
}