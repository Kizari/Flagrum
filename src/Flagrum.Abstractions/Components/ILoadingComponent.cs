namespace Flagrum.Abstractions.Components;

/// <summary>
/// Represents a Razor component with a "loading" state.
/// </summary>
public interface ILoadingComponent
{
    /// <summary>
    /// Sets the state of the component to "loading," preventing the user from interacting with the component
    /// until <see cref="ClearLoading" /> is called.
    /// </summary>
    /// <param name="text">Text to show the user while the loading happens.</param>
    void SetLoading(string text);

    /// <summary>
    /// Clears the "loading" state from the component, enabling the user to interact with the component
    /// again, and hiding the loading indicator and text.
    /// </summary>
    void ClearLoading();

    /// <summary>
    /// Triggers a render to update the component state.
    /// </summary>
    void RefreshState();
}