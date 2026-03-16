using Flagrum.Platform.Abstractions;
using Injectio.Attributes;

namespace Flagrum.Platform.Linux;

/// <inheritdoc />
[RegisterTransient<IPlatformManager>]
public class PlatformManager : IPlatformManager
{
    /// <inheritdoc />
    public bool IsVersionSupported => true;

    /// <inheritdoc />
    public void EnableTaskbarStacking()
    {
        // Enabled by default on Linux
    }

    /// <inheritdoc />
    public void SetFileTypeAssociation()
    {
        // TODO: Implement this
    }
}