using System.IO;
using Flagrum.Core.Graphics.Textures.Luminous.Builder;

namespace Flagrum.Core.Graphics.Textures.Luminous.DataSources;

/// <summary>
/// Represents a source of data for a single surface in a <see cref="BlackTextureBuilder" />.
/// </summary>
public interface IBlackTextureDataSource
{
    /// <summary>
    /// Width of the image, in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Height of the image, in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Format of the pixel data contained in the data source.
    /// </summary>
    BlackTexturePixelFormat Format { get; }

    /// <summary>
    /// Opens a stream to the underlying data. Caller is responsible for disposing the stream.
    /// </summary>
    Stream OpenStream();
}