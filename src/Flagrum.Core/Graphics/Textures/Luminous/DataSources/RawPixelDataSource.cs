using System;
using System.IO;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace Flagrum.Core.Graphics.Textures.Luminous.DataSources;

/// <summary>
/// Data source that streams pixel data from memory.
/// </summary>
/// <param name="memory">Memory that contains the pixel data.</param>
/// <param name="format">Format of the pixel data in the memory buffer.</param>
public class RawPixelDataSource(
    ReadOnlyMemory<byte> memory,
    int width,
    int height,
    BlackTexturePixelFormat format,
    BlackTextureImageFlags flags) : IBlackTextureDataSource
{
    /// <inheritdoc />
    public int Width { get; } = width;

    /// <inheritdoc />
    public int Height { get; } = height;

    /// <inheritdoc />
    public BlackTexturePixelFormat Format { get; } = format;

    /// <inheritdoc />
    public Stream OpenStream()
    {
        // Return memory as-is if the texture is not swizzled
        if (!flags.HasFlag(BlackTextureImageFlags.SWIZZLE))
        {
            return memory.AsStream();
        }

        // Deswizzle the surface into a new buffer
        var strategy = PixelFormatStrategyFactory.Create(Format);
        var buffer = MemoryOwner<byte>.Allocate(memory.Length);
        strategy.DeswizzleSurface(memory.Span, buffer.Span, Width, Height);

        // Return the buffer as a stream
        // The buffer will be disposed when the stream is, so there is no need to track its lifetime
        return buffer.AsStream();
    }
}