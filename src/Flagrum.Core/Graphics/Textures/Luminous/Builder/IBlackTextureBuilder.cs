using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Graphics.Textures.Luminous.DataSources;

namespace Flagrum.Core.Graphics.Textures.Luminous.Builder;

/// <summary>
/// Creates <c>.btex</c> files from standard image formats.
/// </summary>
public interface IBlackTextureBuilder
{
    /// <summary>
    /// Adds an image (including all mip levels) from raw pixel data.
    /// Data must already be in the same pixel format as the builder.
    /// </summary>
    /// <param name="dataSources">Image data source for each mip level.</param>
    IBlackTextureBuilder AddImage(List<IBlackTextureDataSource> dataSources);

    /// <summary>
    /// Adds an image to the texture.
    /// </summary>
    /// <param name="imageSource">Source of the image file.</param>
    IBlackTextureBuilder AddRasterImage(RasterImageDataSource imageSource);

    /// <summary>
    /// Writes the <see cref="BlackTexture" /> to memory based on the configuration applied to this builder.
    /// </summary>
    /// <returns>Buffer containing the BTEX file.</returns>
    byte[] Build();

    /// <summary>
    /// Writes the <see cref="BlackTexture" /> to the given stream based on the configuration applied to this builder.
    /// </summary>
    /// <param name="destination">Stream to write the <c>.btex</c> file to.</param>
    void Build(Stream destination);
}