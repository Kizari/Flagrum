using System;
using System.Collections;
using System.Collections.Generic;
using Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Graphics.Textures.Shared;

/// <summary>
/// Represents an enumerator for iterating over the surfaces of a texture.
/// </summary>
public interface ITextureSurfaceEnumerator : IDisposable
{
    /// <summary>
    /// Retrieves a surface based on the desired image index and mip level.
    /// </summary>
    /// <param name="arrayIndex">Index of the image in the texture array.</param>
    /// <param name="mipLevel">Mip level of the surface to retrieve.</param>
    TextureSurface ElementAt(int arrayIndex, int mipLevel);
}

/// <summary>
/// Configuration for a <see cref="TextureSurfaceEnumerator{TPixel}" />.
/// </summary>
/// <param name="ArrayCount">Total number of images in the texture.</param>
/// <param name="MipMapCount">Number of mip maps per image in the texture.</param>
/// <param name="BaseWidth">Width of the highest resolution mip map.</param>
/// <param name="BaseHeight">Height of the highest resolution mip map.</param>
/// <param name="Data">View of the memory where the raw pixel data is stored for the texture.</param>
/// <param name="Alignment">
/// Optional block size that each surface is aligned to.
/// Leave null if there is no padding between surfaces.
/// </param>
public record TextureSurfaceEnumeratorConfiguration(
    uint ArrayCount,
    int MipMapCount,
    int BaseWidth,
    int BaseHeight,
    ReadOnlyMemory<byte> Data,
    int? Alignment = null);

/// <summary>
/// Enumerator for enumerating the surfaces of a texture.
/// </summary>
public sealed class TextureSurfaceEnumerator<TPixel>(TextureSurfaceEnumeratorConfiguration configuration)
    : IEnumerator<TextureSurface>, ITextureSurfaceEnumerator
    where TPixel : unmanaged
{
    private int _currentHeight;
    private int _currentImage;
    private int _currentMip = -1;
    private int _currentOffset;
    private int _currentSize;
    private int _currentWidth;

    /// <inheritdoc />
    public TextureSurface Current => _currentSize == 0
        ? throw new InvalidOperationException("Cannot get current value before moving at least once.")
        : new TextureSurface(
            _currentImage,
            _currentMip,
            _currentWidth,
            _currentHeight,
            _currentOffset,
            _currentSize,
            configuration.Data.Slice(_currentOffset, _currentSize));

    /// <inheritdoc />
    object IEnumerator.Current => Current;

    /// <inheritdoc />
    public bool MoveNext()
    {
        _currentMip++;

        // Move to the next image if already at the last mip for the current image
        if (_currentMip >= configuration.MipMapCount)
        {
            _currentMip = 0;
            _currentImage++;
        }

        // Terminate if already at the end of all surfaces
        if (_currentImage >= configuration.ArrayCount)
        {
            return false;
        }

        // Compute values for next memory slice
        _currentWidth = Math.Max(1, configuration.BaseWidth >> _currentMip);
        _currentHeight = Math.Max(1, configuration.BaseHeight >> _currentMip);
        _currentOffset = configuration.Alignment == null
            ? _currentOffset + _currentSize
            : (_currentOffset + _currentSize).AlignTo(configuration.Alignment.Value);
        _currentSize = CompressedBlock.GetSizeBytes<TPixel>(_currentWidth, _currentHeight);

        return true;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _currentImage = 0;
        _currentMip = -1;
        _currentOffset = 0;
        _currentSize = 0;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // No resources to dispose, just implemented to fulfil the interface contract
    }

    /// <inheritdoc />
    public TextureSurface ElementAt(int arrayIndex, int mipLevel)
    {
        if (_currentImage > arrayIndex || (_currentImage == arrayIndex && _currentMip > mipLevel))
        {
            Reset();
        }

        while (!(_currentImage == arrayIndex && _currentMip == mipLevel))
        {
            MoveNext();
        }

        return Current;
    }
}

/// <summary>
/// Extension methods relating to <see cref="TextureSurfaceEnumerator{TPixel}" />.
/// </summary>
public static class TextureSurfaceEnumerableExtensions
{
    /// <summary>
    /// Retrieves a surface element by its 2D index.
    /// </summary>
    /// <param name="enumerable">Enumerable of surfaces.</param>
    /// <param name="arrayIndex">Index of the image to retrieve the surface from.</param>
    /// <param name="mipLevel">Mip level of the surface to retrieve.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if enumerable does not return a <see cref="TextureSurfaceEnumerator" />.
    /// </exception>
    public static TextureSurface ElementAt(this IEnumerable<TextureSurface> enumerable, int arrayIndex, int mipLevel)
    {
        using var enumerator = enumerable.GetEnumerator();
        if (enumerator is not ITextureSurfaceEnumerator surfaceEnumerator)
        {
            throw new InvalidOperationException(
                $"Can only retrieve element by array index and mip level " +
                $"when enumerator is of type {nameof(ITextureSurfaceEnumerator)}.");
        }

        return surfaceEnumerator.ElementAt(arrayIndex, mipLevel);
    }
}