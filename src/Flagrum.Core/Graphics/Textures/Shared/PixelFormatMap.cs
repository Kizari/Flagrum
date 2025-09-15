using System;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Shared.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Flagrum.Core.Graphics.Textures.Shared;

/// <summary>
/// Utilities for working with texture pixel formats.
/// </summary>
/// <remarks>
/// Only handles working with pixel formats that the game is known to use.
/// The game does not use every format listed in <see cref="BlackTexturePixelFormat" />.
/// It is unclear if it is even capable of doing so, or if they were never implemented.
/// </remarks>
public static class PixelFormatMap
{
    /// <summary>
    /// Gets the equivalent <see cref="BlackTexturePixelFormat" /> from a <see cref="DxgiFormat" />.
    /// </summary>
    /// <param name="format">Format to find an equivalent for.</param>
    /// <returns>
    /// <see cref="BlackTexturePixelFormat.None" /> if no equivalent was found, otherwise the equivalent format.
    /// </returns>
    public static BlackTexturePixelFormat Get(DxgiFormat format) => format switch
    {
        DxgiFormat.BC1_UNORM or DxgiFormat.BC1_UNORM_SRGB => BlackTexturePixelFormat.BC1_UNORM,
        DxgiFormat.BC3_UNORM or DxgiFormat.BC3_UNORM_SRGB => BlackTexturePixelFormat.BC3_UNORM,
        DxgiFormat.BC4_UNORM => BlackTexturePixelFormat.BC4_UNORM,
        DxgiFormat.BC5_UNORM => BlackTexturePixelFormat.BC5_UNORM,
        DxgiFormat.BC6H_UF16 => BlackTexturePixelFormat.BC6_UFLOAT,
        DxgiFormat.BC7_UNORM or DxgiFormat.BC7_UNORM_SRGB => BlackTexturePixelFormat.BC7_UNORM,
        DxgiFormat.R16G16B16A16_FLOAT => BlackTexturePixelFormat.R16G16B16A16_FLOAT,
        DxgiFormat.R16G16B16A16_UNORM => BlackTexturePixelFormat.R16G16B16A16_UNORM,
        DxgiFormat.R32_FLOAT => BlackTexturePixelFormat.R32_FLOAT,
        DxgiFormat.R32G32B32A32_FLOAT => BlackTexturePixelFormat.R32G32B32A32_FLOAT,
        DxgiFormat.R8G8B8A8_UNORM or DxgiFormat.R8G8B8A8_UNORM_SRGB => BlackTexturePixelFormat.R8G8B8A8_UNORM,
        _ => BlackTexturePixelFormat.None
    };

    /// <summary>
    /// Gets the equivalent <see cref="DxgiFormat" /> from a <see cref="BlackTexturePixelFormat" />.
    /// </summary>
    /// <param name="format">Format to find an equivalent for.</param>
    public static DxgiFormat Get(BlackTexturePixelFormat format) => format switch
    {
        BlackTexturePixelFormat.BC1_UNORM => DxgiFormat.BC1_UNORM,
        BlackTexturePixelFormat.BC3_UNORM => DxgiFormat.BC3_UNORM,
        BlackTexturePixelFormat.BC4_UNORM => DxgiFormat.BC4_UNORM,
        BlackTexturePixelFormat.BC5_UNORM => DxgiFormat.BC5_UNORM,
        BlackTexturePixelFormat.BC6_UFLOAT => DxgiFormat.BC6H_UF16,
        BlackTexturePixelFormat.BC7_UNORM => DxgiFormat.BC7_UNORM,
        BlackTexturePixelFormat.R16G16B16A16_FLOAT => DxgiFormat.R16G16B16A16_FLOAT,
        BlackTexturePixelFormat.R16G16B16A16_UNORM => DxgiFormat.R16G16B16A16_UNORM,
        BlackTexturePixelFormat.R32_FLOAT => DxgiFormat.R32_FLOAT,
        BlackTexturePixelFormat.R32G32B32A32_FLOAT => DxgiFormat.R32G32B32A32_FLOAT,
        BlackTexturePixelFormat.R8G8B8A8_UNORM => DxgiFormat.R8G8B8A8_UNORM,
        _ => throw new NotSupportedException($"Unsupported pixel format {format}.")
    };

    /// <summary>
    /// Gets the equivalent sRGB <see cref="DxgiFormat" /> from a <see cref="BlackTexturePixelFormat" />.
    /// </summary>
    /// <param name="format">Format to find an equivalent for.</param>
    public static DxgiFormat GetSrgb(BlackTexturePixelFormat format) => format switch
    {
        BlackTexturePixelFormat.BC1_UNORM => DxgiFormat.BC1_UNORM_SRGB,
        BlackTexturePixelFormat.BC3_UNORM => DxgiFormat.BC3_UNORM_SRGB,
        BlackTexturePixelFormat.BC7_UNORM => DxgiFormat.BC7_UNORM_SRGB,
        BlackTexturePixelFormat.R8G8B8A8_UNORM => DxgiFormat.R8G8B8A8_UNORM_SRGB,
        _ => throw new NotSupportedException($"Unsupported sRGB pixel format {format}.")
    };

    /// <summary>
    /// Gets the equivalent <see cref="BlackTexturePixelFormat" /> from an <see cref="Image{TPixel}" />
    /// based on its <see cref="IPixel{TSelf}" /> type.
    /// </summary>
    /// <param name="image">Image to get the pixel format of.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if an unexpected <see cref="IPixel{TSelf}" /> type is detected.
    /// </exception>
    public static BlackTexturePixelFormat Get(Image image) => image switch
    {
        Image<Rgba32> => BlackTexturePixelFormat.R8G8B8A8_UNORM,
        Image<R32F> => BlackTexturePixelFormat.R32_FLOAT,
        Image<Rgba64> => BlackTexturePixelFormat.R16G16B16A16_UNORM,
        Image<RgbaHalfVector> => BlackTexturePixelFormat.R16G16B16A16_FLOAT,
        Image<RgbaVector> => BlackTexturePixelFormat.R32G32B32A32_FLOAT,
        _ => throw new NotSupportedException("Unexpected pixel type.")
    };

    /// <summary>
    /// Determines whether a pixel format uses the sRGB color space.
    /// </summary>
    /// <param name="format">Format to check.</param>
    /// <returns><c>true</c> if the format is in sRGB color space, otherwise <c>false</c>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if passed a pixel format the game is not known to use.
    /// </exception>
    public static bool IsSrgb(DxgiFormat format) => format switch
    {
        DxgiFormat.BC1_UNORM => false,
        DxgiFormat.BC1_UNORM_SRGB => true,
        DxgiFormat.BC3_UNORM => false,
        DxgiFormat.BC3_UNORM_SRGB => true,
        DxgiFormat.BC4_UNORM => false,
        DxgiFormat.BC5_UNORM => false,
        DxgiFormat.BC6H_UF16 => false,
        DxgiFormat.BC7_UNORM => false,
        DxgiFormat.BC7_UNORM_SRGB => true,
        DxgiFormat.R16G16B16A16_FLOAT => false,
        DxgiFormat.R16G16B16A16_UNORM => false,
        DxgiFormat.R32_FLOAT => false,
        DxgiFormat.R32G32B32A32_FLOAT => false,
        DxgiFormat.R8G8B8A8_UNORM => false,
        DxgiFormat.R8G8B8A8_UNORM_SRGB => true,
        _ => throw new NotSupportedException($"Unsupported pixel format {format}.")
    };

    /// <summary>
    /// Determines whether a pixel format uses block compression.
    /// </summary>
    /// <param name="format">Format to check.</param>
    /// <returns><c>true</c> if input is a block-compressed format, otherwise <c>false</c>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if passed a pixel format the game is not known to use.
    /// </exception>
    public static bool IsCompressed(BlackTexturePixelFormat format) => format switch
    {
        BlackTexturePixelFormat.A8R8G8B8 => false,
        BlackTexturePixelFormat.B8 => false,
        BlackTexturePixelFormat.BC3_UNORM => true,
        BlackTexturePixelFormat.BC4_UNORM => true,
        BlackTexturePixelFormat.BC5_UNORM => true,
        BlackTexturePixelFormat.BC6_UFLOAT => true,
        BlackTexturePixelFormat.BC7_UNORM => true,
        BlackTexturePixelFormat.BC1_UNORM => true,
        BlackTexturePixelFormat.G8R8_UNORM => false,
        BlackTexturePixelFormat.R16G16B16A16_FLOAT => false,
        BlackTexturePixelFormat.R16G16B16A16_UNORM => false,
        BlackTexturePixelFormat.R32_FLOAT => false,
        BlackTexturePixelFormat.R32G32B32A32_FLOAT => false,
        BlackTexturePixelFormat.R8G8B8A8_UNORM => false,
        _ => throw new NotSupportedException($"Unsupported pixel format {format}.")
    };

    /// <summary>
    /// Determines whether a pixel format uses a 16-bit or 32-bit pixel format, rather than 8-bit.
    /// </summary>
    /// <param name="format">Format to check.</param>
    /// <returns><c>true</c> if input is an HDR format, otherwise <c>false</c>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if passed a pixel format the game is not known to use.
    /// </exception>
    public static bool IsHighDynamicRange(BlackTexturePixelFormat format) => format switch
    {
        BlackTexturePixelFormat.A8R8G8B8 => false,
        BlackTexturePixelFormat.B8 => false,
        BlackTexturePixelFormat.BC3_UNORM => false,
        BlackTexturePixelFormat.BC4_UNORM => false,
        BlackTexturePixelFormat.BC5_UNORM => false,
        BlackTexturePixelFormat.BC6_UFLOAT => true,
        BlackTexturePixelFormat.BC7_UNORM => false,
        BlackTexturePixelFormat.BC1_UNORM => false,
        BlackTexturePixelFormat.G8R8_UNORM => false,
        BlackTexturePixelFormat.R16G16B16A16_FLOAT => true,
        BlackTexturePixelFormat.R16G16B16A16_UNORM => true,
        BlackTexturePixelFormat.R32_FLOAT => true,
        BlackTexturePixelFormat.R32G32B32A32_FLOAT => true,
        BlackTexturePixelFormat.R8G8B8A8_UNORM => false,
        _ => throw new NotSupportedException($"Unsupported pixel format {format}.")
    };
}