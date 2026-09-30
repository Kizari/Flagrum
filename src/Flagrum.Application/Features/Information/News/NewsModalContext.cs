using System;
using Flagrum.Abstractions;

namespace Flagrum.Application.Features.Information.News;

/// <summary>
/// Wraps an informational modal to determine which conditions are required to present it.
/// </summary>
public class NewsModalContext
{
    /// <summary>
    /// Active user profile.
    /// </summary>
    public required IProfileService Profile { get; init; }

    /// <summary>
    /// Application functionality.
    /// </summary>
    public required IApplication Application { get; init; }

    /// <summary>
    /// Type of the modal component.
    /// </summary>
    public required Type Type { get; init; }

    /// <summary>
    /// Modal to present.
    /// </summary>
    public NewsModal? Reference { get; set; }

    /// <summary>
    /// Condition under which the modal is presented.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> if the modal should be presented in the news flow.
    /// </remarks>
    public required Func<IProfileService, IApplication, bool> Predicate { get; init; }

    /// <summary>
    /// Modal that follows this in the flow, if applicable.
    /// </summary>
    public NewsModalContext? Next { get; set; }
}