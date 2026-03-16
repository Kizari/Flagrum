using System.Diagnostics.CodeAnalysis;

namespace Flagrum.Abstractions.Application;

/// <summary>
/// Manages platform-specific functionality.
/// </summary>
public interface IPlatformManager
{
    /// <summary>
    /// Retrieves the Lucent ID for this device.
    /// </summary>
    Guid LucentClientId { get; }
    
    /// <summary>
    /// Whether the current version of Flagrum is still supported.
    /// </summary>
    bool IsVersionSupported { get; }

    /// <summary>
    /// Allows the Flagrum window icon to stack with the pinned Flagrum icon on the taskbar.
    /// </summary>
    void EnableTaskbarStacking();

    /// <summary>
    /// Associates the <c>.fmod</c> file format with Flagrum on the local system.
    /// </summary>
    void SetFileTypeAssociation();

    /// <summary>
    /// Attempts to automatically detect the path to the Steam executable.
    /// </summary>
    /// <param name="result">Steam executable path.</param>
    /// <returns><c>true</c> if the path could be located, otherwise <c>false</c>.</returns>
    bool TryGetSteamExecutablePath([NotNullWhen(true)] out string? result);
}