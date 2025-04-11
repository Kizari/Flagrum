namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents the mod library page of the mod manager.
/// </summary>
public interface IModLibrary
{
    /// <summary>
    /// Attempts to launch the game if it is not already running.
    /// </summary>
    /// <param name="isDebug">Whether to set debug mode on the injected DLL when the game is launched.</param>
    Task LaunchGameAsync(bool isDebug);
}