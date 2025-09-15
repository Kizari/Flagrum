using System.IO;

namespace Flagrum.Core.Graphics.Textures.Luminous.DataSources;

/// <summary>
/// Data source that streams an image file from a standard raster format.
/// </summary>
/// <param name="buffer">Buffer that contains the image file.</param>
public class RasterImageDataSource : IBlackTextureDataSource
{
    private readonly byte[]? _buffer;
    private readonly string? _filePath;

    public RasterImageDataSource(byte[] buffer, int width, int height, BlackTexturePixelFormat format)
        : this(width, height, format) =>
        _buffer = buffer;

    public RasterImageDataSource(string filePath, int width, int height, BlackTexturePixelFormat format)
        : this(width, height, format) =>
        _filePath = filePath;

    private RasterImageDataSource(int width, int height, BlackTexturePixelFormat format)
    {
        Width = width;
        Height = height;
        Format = format;
    }

    /// <inheritdoc />
    public int Width { get; }

    /// <inheritdoc />
    public int Height { get; }

    /// <inheritdoc />
    public BlackTexturePixelFormat Format { get; }

    /// <inheritdoc />
    public Stream OpenStream() => _buffer == null
        ? new FileStream(_filePath!, FileMode.Open, FileAccess.Read, FileShare.Read)
        : new MemoryStream(_buffer);
}