using System;
using System.Collections.Generic;
using Flagrum.Core.Graphics.Textures.Shared;

namespace Flagrum.Core.Graphics.Textures.Luminous;

/// <summary>
/// Handles operations relating to the pixel formats for a texture.
/// </summary>
public interface IPixelFormatStrategy
{
    /// <summary>
    /// Whether the pixel format is a block-compressed type.
    /// </summary>
    bool IsCompressed { get; }

    /// <summary>
    /// Gets a <see cref="TextureSurfaceEnumerator{TPixel}" /> that's appropriate for the pixel format.
    /// </summary>
    /// <param name="configuration">Configuration to instantiate the enumerator with.</param>
    IEnumerator<TextureSurface> GetEnumerator(TextureSurfaceEnumeratorConfiguration configuration);

    /// <summary>
    /// Calculates the total number of bytes in a single row of an image.
    /// </summary>
    /// <param name="width">Width of the image, in pixels.</param>
    int GetRowPitch(int width);

    /// <summary>
    /// Gets the total size of an image, in bytes.
    /// </summary>
    /// <param name="width">Width of the image.</param>
    /// <param name="height">Height of the image.</param>
    int GetSizeBytes(int width, int height);

    /// <summary>
    /// Deswizzles image data for one surface of a texture.
    /// </summary>
    /// <param name="sourceBuffer">Swizzled pixel data for the surface.</param>
    /// <param name="destinationBuffer">Buffer to write the deswizzled pixel data to.</param>
    /// <param name="width">Width of the surface, in pixels.</param>
    /// <param name="height">Height of the surface, in pixels.</param>
    void DeswizzleSurface(ReadOnlySpan<byte> sourceBuffer, Span<byte> destinationBuffer, int width, int height);
}