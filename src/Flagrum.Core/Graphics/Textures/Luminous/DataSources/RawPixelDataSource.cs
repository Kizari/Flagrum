using System;
using System.IO;
using CommunityToolkit.HighPerformance;

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
    BlackTexturePixelFormat format) : IBlackTextureDataSource
{
    /// <inheritdoc />
    public int Width { get; } = width;

    /// <inheritdoc />
    public int Height { get; } = height;

    /// <inheritdoc />
    public BlackTexturePixelFormat Format { get; } = format;

    /// <inheritdoc />
    public Stream OpenStream() => memory.AsStream();
}