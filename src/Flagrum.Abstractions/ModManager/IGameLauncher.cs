namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Handles launching the game and injecting the hook DLL.
/// </summary>
public interface IGameLauncher
{
    /// <summary>
    /// Launches the game and injects the hook DLL.
    /// </summary>
    /// <param name="isDebug">Whether debug options should be enabled in the DLL configuration before injecting.</param>
    /// <param name="command">Optional command string passed in when launching via Steam.</param>
    /// <returns>Status of the finished operation.</returns>
    GameLaunchResult TryLaunch(bool isDebug, string? command = null);
}