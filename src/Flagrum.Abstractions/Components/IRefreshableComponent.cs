namespace Flagrum.Abstractions.Components;

/// <summary>
/// Represents a Razor component whose state can be refreshed.
/// </summary>
public interface IRefreshableComponent
{
    /// <summary>
    /// Triggers a render to update the component state.
    /// </summary>
    void Refresh();
}