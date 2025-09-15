using System;

namespace Flagrum.Core.Graphics.Textures.Luminous;

/// <summary>
/// Determines various attributes of a <see cref="BlackTextureImageData" /> within a <see cref="BlackTexture" />.
/// </summary>
[Flags]
public enum BlackTextureImageFlags : byte
{
    NONE = 0,

    /// <summary>
    /// The image contains mipmaps.
    /// </summary>
    MIPMAP = 1,

    /// <summary>
    /// It is not currently known what this is used for.
    /// </summary>
    VOLUME = 2,

    /// <summary>
    /// The image is a cubemap.
    /// </summary>
    CUBE = 4,

    /// <summary>
    /// The image has undergone texture swizzling. This is typically used only on PS4 versions of the game.
    /// </summary>
    SWIZZLE = 8,

    /// <summary>
    /// The texture uses a compressed pixel format.
    /// </summary>
    COMPRESS = 16,

    /// <summary>
    /// The pixel data is in the sRGB colour space.
    /// </summary>
    SRGB = 32,

    /// <summary>
    /// It is assumed this flag is set when the texture is built for a specific non-PC platform.
    /// However, it is not known for certain if this is how it is used.
    /// </summary>
    PLATFORM = 64,

    /// <summary>
    /// The texture contains multiple images.
    /// </summary>
    ARRAY = 128
}