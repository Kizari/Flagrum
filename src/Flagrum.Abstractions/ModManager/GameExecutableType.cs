namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents the game executable build type.
/// </summary>
public enum GameExecutableType : byte
{
    /// <summary>
    /// Executable type could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// Official debug executable, was only briefly available on Steam.
    /// </summary>
    Debug,

    /// <summary>
    /// Final Steam release, not expected to be updated ever again, but could be.
    /// </summary>
    Release
}