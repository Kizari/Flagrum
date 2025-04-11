namespace Flagrum.Abstractions;

/// <summary>
/// Represents a class that manages application navigation.
/// </summary>
public interface INavigationManager
{
    /// <summary>
    /// Navigates the application to the given page.
    /// </summary>
    /// <param name="path">Path to the page to navigate to.</param>
    void NavigateTo(string path);
}