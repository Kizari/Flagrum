using System;
using Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;
using Flagrum.Core.Graphics.Textures.Shared.PixelFormats;
using SixLabors.ImageSharp.PixelFormats;

namespace Flagrum.Core.Graphics.Textures.Luminous;

/// <summary>
/// Creates generically typed concrete instances of <see cref="IPixelFormatStrategy" /> at runtime.
/// </summary>
public static class PixelFormatStrategyFactory
{
    /// <summary>
    /// Creates a strategy for the given pixel format.
    /// </summary>
    /// <param name="format">Pixel format to get a strategy for.</param>
    /// <exception cref="NotSupportedException">Thrown if the given pixel format is not supported.</exception>
    public static IPixelFormatStrategy Create(BlackTexturePixelFormat format) => format switch
    {
        BlackTexturePixelFormat.A8R8G8B8 => new PixelFormatStrategy<Argb32>(),
        BlackTexturePixelFormat.B8 => new PixelFormatStrategy<B8>(),
        BlackTexturePixelFormat.BC1_UNORM => new PixelFormatStrategy<BC1Block>(),
        BlackTexturePixelFormat.BC3_UNORM => new PixelFormatStrategy<BC3Block>(),
        BlackTexturePixelFormat.BC4_UNORM => new PixelFormatStrategy<BC4Block>(),
        BlackTexturePixelFormat.BC5_UNORM => new PixelFormatStrategy<BC5Block>(),
        BlackTexturePixelFormat.BC6_UFLOAT => new PixelFormatStrategy<BC6HBlock>(),
        BlackTexturePixelFormat.BC7_UNORM => new PixelFormatStrategy<BC7Block>(),
        BlackTexturePixelFormat.G8R8_UNORM => new PixelFormatStrategy<GR16>(),
        BlackTexturePixelFormat.R16G16B16A16_FLOAT => new PixelFormatStrategy<RgbaHalfVector>(),
        BlackTexturePixelFormat.R16G16B16A16_UNORM => new PixelFormatStrategy<Rgba64>(),
        BlackTexturePixelFormat.R32_FLOAT => new PixelFormatStrategy<R32F>(),
        BlackTexturePixelFormat.R32G32B32A32_FLOAT => new PixelFormatStrategy<RgbaVector>(),
        BlackTexturePixelFormat.R8G8B8A8_UNORM => new PixelFormatStrategy<Rgba32>(),
        _ => throw new NotSupportedException($"Unsupported pixel format {format}.")
    };
}