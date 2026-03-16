namespace Flagrum.Services;

/// <summary>
/// Represents the application splash screen.
/// </summary>
public interface ISplashScreen
{
    /// <summary>
    /// Creates and displays the splash screen window.
    /// </summary>
    void Show();
    
    /// <summary>
    /// Closes the splash screen window and destroys it.
    /// </summary>
    void Close();

    /// <summary>
    /// Sets the text associated with the loading bar to inform the user of what is currently happening.
    /// </summary>
    void SetLoadingText(string text);
}