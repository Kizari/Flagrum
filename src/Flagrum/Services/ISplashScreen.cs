namespace Flagrum.Services;

/// <summary>
/// Represents the application splash screen.
/// </summary>
public interface ISplashScreen
{
    /// <summary>
    /// Sets the text associated with the loading bar to inform the user of what is currently happening.
    /// </summary>
    void SetLoadingText(string text);
}