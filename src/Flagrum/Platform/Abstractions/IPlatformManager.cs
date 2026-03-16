namespace Flagrum.Platform.Abstractions;

/// <summary>
/// Manages platform-specific functionality.
/// </summary>
public interface IPlatformManager
{
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
}