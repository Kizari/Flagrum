namespace Flagrum.Application.Features.ModManager.Launcher;

/// <summary>
/// The status of attempting to launch the game from <see cref="GameLauncher"/>.
/// </summary>
public enum GameLaunchResult
{
    /// <summary>
    /// The game was launched successfully.
    /// </summary>
    Success,
    
    /// <summary>
    /// The game was not launched as an instance of it was already running.
    /// </summary>
    GameAlreadyRunning,
    
    /// <summary>
    /// The game was not launched as the mod loader DLL does not support the user's game executable.
    /// </summary>
    UnsupportedExecutable,
    
    /// <summary>
    /// The game was not launched due to insufficient access to create the game process.
    /// </summary>
    AccessDenied
}