namespace Flagrum.Core.Graphics.Textures.Luminous;

/// <summary>
/// Determines which version of the <see cref="BlackTexture"/> format a texture uses.
/// It seems to use a date-based versioning system <c>yyyyMMdd</c>.
/// </summary>
public enum BlackTextureVersion : ushort
{
    VERSION_20080801 = 1,
    VERSION_20140909 = 2,
    VERSION_20141022 = 3,
    VERSION_20151102 = 4,
    
    /// <summary>
    /// The latest version of the format.
    /// </summary>
    VERSION_LATEST = VERSION_20151102,
    
    /// <summary>
    /// The oldest version of the format that Luminous supports.
    /// </summary>
    /// <remarks>
    /// This is an assumption, it has not been confirmed.
    /// </remarks>
    VERSION_SUPPORT = VERSION_20080801
}