using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Luminous.Builder;
using Flagrum.Core.Graphics.Textures.Luminous.DataSources;
using Flagrum.Core.Graphics.Textures.Shared;

namespace Flagrum.Core.Graphics.Textures.DirectX;

/// <summary>
/// Represents the Direct Draw Surface (DDS) file format.
/// </summary>
public readonly ref partial struct DirectDrawSurface : IEnumerable<TextureSurface>
{
    private readonly ReadOnlyMemory<byte> _buffer;
    private readonly IPixelFormatStrategy _strategy;

    /// <summary>
    /// Wraps a DDS file.
    /// </summary>
    /// <param name="buffer">The DDS file in memory.</param>
    public DirectDrawSurface(ReadOnlyMemory<byte> buffer)
    {
        _buffer = buffer;
        var span = buffer.Span;

        ref var start = ref MemoryMarshal.GetReference(span);
        DdsHeader = ref Unsafe.As<byte, DirectDrawSurfaceHeader>(ref start);
        DX10Header = ref Unsafe.As<byte, DirectX10Header>(
            ref Unsafe.Add(ref start, DirectDrawSurfaceHeader.StructSize));

        var pData = DirectDrawSurfaceHeader.StructSize + DirectX10Header.StructSize;
        Data = _buffer[pData..];

        _strategy = PixelFormatStrategyFactory.Create(PixelFormatMap.Get(DX10Header.Format));

        // Validate the texture
        if (DX10Header.ResourceDimension == DirectX10ResourceDimension.Texture3D
            && DdsHeader.Depth <= 1)
        {
            throw new InvalidDataException("Volume texture must have a depth greater than 1.");
        }
    }

    /// <summary>
    /// DDS header.
    /// </summary>
    public readonly ref DirectDrawSurfaceHeader DdsHeader;

    /// <summary>
    /// DirectX 10 extended header.
    /// </summary>
    public readonly ref DirectX10Header DX10Header;

    /// <summary>
    /// Raw pixel data for every surface in the texture.
    /// </summary>
    public readonly ReadOnlyMemory<byte> Data;

    /// <summary>
    /// Retrieves a collection of data sources for each surface in this texture.
    /// </summary>
    /// <returns>
    /// A list of lists. The outer list represents one image in the texture, while
    /// the inner list has a data source for each mip in that texture.
    /// </returns>
    public List<List<IBlackTextureDataSource>> GetImageDataSources()
    {
        var result = new List<List<IBlackTextureDataSource>>((int)DX10Header.ArraySize);
        for (var i = 0; i < DX10Header.ArraySize; i++)
        {
            result[i] = new List<IBlackTextureDataSource>((int)DdsHeader.MipMapCount);
        }

        var format = PixelFormatMap.Get(DX10Header.Format);
        if (format == BlackTexturePixelFormat.None)
        {
            throw new NotSupportedException($"DDS pixel format {DX10Header.Format} not supported by BTEX converter.");
        }

        foreach (var surface in this)
        {
            result[surface.ArrayIndex].Add(new RawPixelDataSource(surface.Data, surface.Width, surface.Height, format));
        }

        return result;
    }

    /// <inheritdoc />
    public IEnumerator<TextureSurface> GetEnumerator()
    {
        var configuration = new TextureSurfaceEnumeratorConfiguration(
            DX10Header.ArraySize,
            (int)DdsHeader.MipMapCount,
            (int)DdsHeader.Width,
            (int)DdsHeader.Height,
            Data);

        return _strategy.GetEnumerator(configuration);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}