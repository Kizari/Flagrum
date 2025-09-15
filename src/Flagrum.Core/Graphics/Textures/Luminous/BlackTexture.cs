using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Flagrum.Core.Data.Binary;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Gnf;
using Flagrum.Core.Graphics.Textures.Luminous.DataSources;
using Flagrum.Core.Graphics.Textures.NvidiaTextureTools;
using Flagrum.Core.Graphics.Textures.Shared;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;

namespace Flagrum.Core.Graphics.Textures.Luminous;

/// <summary>
/// Represents the <c>.btex</c> format.
/// </summary>
public readonly ref partial struct BlackTexture : IEnumerable<TextureSurface>
{
    private readonly ReadOnlyMemory<byte> _buffer;
    private readonly IPixelFormatStrategy _strategy;

    public unsafe BlackTexture(ReadOnlyMemory<byte> buffer)
    {
        _buffer = buffer;
        var span = buffer.Span;

        ref var start = ref MemoryMarshal.GetReference(span);
        SedbHeader = ref Unsafe.As<byte, SectionDataBinaryHeader>(ref start);

        var pBtexHeader = SedbHeader.Offset;
        BtexHeader = ref Unsafe.As<byte, BlackTextureHeader>(ref Unsafe.Add(ref start, pBtexHeader));

        if (BtexHeader.ImageCount > 1)
        {
            throw new NotSupportedException("Textures with multiple images not supported.");
        }

        var pImageHeader = pBtexHeader + BtexHeader.ImageHeaderOffset;
        ImageHeader = ref Unsafe.As<byte, BlackTextureImageHeader>(ref Unsafe.Add(ref start, pImageHeader));

        Name = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(
            (byte*)Unsafe.AsPointer(ref Unsafe.Add(ref start, pImageHeader + ImageHeader.NameOffset)));

        if (BtexHeader.Platform == BlackTexturePlatform.PLATFORM_WIIU)
        {
            var pSurfaces = pImageHeader + ImageHeader.SurfaceHeaderOffset;
            Surfaces = MemoryMarshal.Cast<byte, BlackTextureSurfaceHeader>(span.Slice((int)pSurfaces,
                Unsafe.SizeOf<BlackTextureSurfaceHeader>() * ImageHeader.SurfaceCount));

            var pData = pBtexHeader + BtexHeader.HeaderSize;
            Data = _buffer[(int)pData..];
        }
        else if (BtexHeader.Platform == BlackTexturePlatform.PLATFORM_PS4)
        {
            var pGnfHeader = pBtexHeader + BtexHeader.HeaderSize;
            GnfHeader = ref Unsafe.As<byte, GnfHeader>(ref Unsafe.Add(ref start, pGnfHeader));
            var pData = pGnfHeader + 256;
            Data = _buffer[(int)pData..];
        }

        _strategy = PixelFormatStrategyFactory.Create(ImageHeader.Format);
    }

    public readonly ref SectionDataBinaryHeader SedbHeader;
    public readonly ref BlackTextureHeader BtexHeader;
    public readonly ref BlackTextureImageHeader ImageHeader;
    public readonly ref GnfHeader GnfHeader;

    public ReadOnlySpan<byte> Name { get; }
    public ReadOnlySpan<BlackTextureSurfaceHeader> Surfaces { get; }
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// Gets the size of all data preceding the raw pixel information for a texture targeting the PC game.
    /// </summary>
    /// <param name="name">Name of the texture that is stored in the header.</param>
    /// <param name="arrayCount">Number of images in the texture.</param>
    /// <param name="mipmapCount">Number of mipmaps per image.</param>
    /// <returns>Total size of the metadata and padding, in bytes.</returns>
    public static int GetMetadataSizePC(string name, int arrayCount, int mipmapCount) =>
        (128 // SEDB header + alignment
         + Unsafe.SizeOf<BlackTextureHeader>()
         + Unsafe.SizeOf<BlackTextureImageHeader>()
         + Unsafe.SizeOf<BlackTextureSurfaceHeader>() * arrayCount * mipmapCount
         + name.Length + 1) // Null-terminator
        .AlignTo(128);

    /// <summary>
    /// Retrieves a collection of data sources for each surface in this texture.
    /// </summary>
    /// <returns>
    /// A list of lists. The outer list represents one image in the texture, while
    /// the inner list has a data source for each mip in that texture.
    /// </returns>
    public List<List<IBlackTextureDataSource>> GetImageDataSources()
    {
        var result = new List<List<IBlackTextureDataSource>>((int)ImageHeader.ArrayCount);
        for (var i = 0; i < ImageHeader.ArrayCount; i++)
        {
            result[i] = new List<IBlackTextureDataSource>(ImageHeader.MipMapCount);
        }

        foreach (var surface in this)
        {
            result[surface.ArrayIndex].Add(new RawPixelDataSource(
                surface.Data,
                surface.Width,
                surface.Height,
                ImageHeader.Format));
        }

        return result;
    }

    /// <summary>
    /// Creates an equivalent DDS file from this <see cref="BlackTexture" />.
    /// </summary>
    /// <returns>In-memory DDS file.</returns>
    public byte[] ToDds()
    {
        using var stream = new MemoryStream();
        
        // Create a properly sized buffer and write the DDS header to it
        var ddsMetadataSize =
            sizeof(uint) + Unsafe.SizeOf<DirectDrawSurfaceHeader>() + Unsafe.SizeOf<DirectX10Header>();
        var ddsBuffer = new byte[ddsMetadataSize];
        var ddsSpan = new Span<byte>(ddsBuffer);
        WriteDdsHeader(ddsSpan);
        stream.Write(ddsSpan);
        
        // Write the pixel data
        if (ImageHeader.Flags.HasFlag(BlackTextureImageFlags.SWIZZLE))
        {
            // Rent a buffer for each mip size
            var buffers = new MemoryOwner<byte>[ImageHeader.MipMapCount];
            foreach (var surface in this)
            {
                buffers[surface.MipLevel] = MemoryOwner<byte>.Allocate(surface.Size);
                if (surface.MipLevel == ImageHeader.MipMapCount)
                {
                    break;
                }
            }

            // Deswizzle each surface into the respective buffer and write it to the output stream
            foreach (var surface in this)
            {
                var buffer = buffers[surface.MipLevel];
                var span = buffer.Span;

                _strategy.DeswizzleSurface(
                    surface.Data.Span,
                    span,
                    surface.Width,
                    surface.Height);

                stream.Write(span);
            }
            
            // Dispose temporary buffers
            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
        }
        else
        {
            // Write each surface to the output stream
            foreach (var surface in this)
            {
                stream.Write(surface.Data.Span);
            }
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Saves one image from this texture to memory in the desired format.
    /// </summary>
    /// <param name="imageIndex">Index of the image to save.</param>
    /// <param name="format">File format to convert the image to.</param>
    /// <returns>Buffer containing the image file.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if image index is not in the valid range.</exception>
    public byte[] Save(int imageIndex, ImageFileFormat format)
    {
        if (imageIndex < 0 || imageIndex >= ImageHeader.ArrayCount)
        {
            throw new IndexOutOfRangeException(
                $"Expected index between 0 and {ImageHeader.ArrayCount}. Got {imageIndex}.");
        }

        using var enumerator = (ITextureSurfaceEnumerator)GetEnumerator();
        using var stream = new MemoryStream();
        WriteOther(stream, enumerator, imageIndex, format);
        return stream.ToArray();
    }

    /// <summary>
    /// Saves each image in this texture to disk.
    /// </summary>
    /// <param name="path">Absolute file path to save the file to.</param>
    /// <param name="format">File format to save the file as.</param>
    /// <param name="arrayFileNameSelector">
    /// Defines how to name each file when saving a texture array to a format that does
    /// not support texture arrays. Input parameters are image index and the file name portion
    /// of <paramref name="path" /> respectively.
    /// If null, the default scheme of "filename.1001" will be used. Extension is appended automatically.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Thrown if the given file format is not supported.
    /// </exception>
    /// <remarks>
    /// BTEX and DDS will retain original pixel format and mipmaps.
    /// Other formats will only take the highest resolution mipmap from each image.
    /// If there are multiple images in the texture and the format is not BTEX or DDS,
    /// multiple files will be saved in a folder named for <paramref name="path" />.
    /// </remarks>
    public void Save(string path, ImageFileFormat format,
        Func<int, string, string>? arrayFileNameSelector = null)
    {
        arrayFileNameSelector ??= (i, name) => $"{name}.1{(i + 1).WithLeadingZeroes(3)}";
        var pathNoExtension = path[..path.LastIndexOf('.')];

        // ReSharper disable twice PossibleUnintendedReferenceComparison
        if (format == ImageFileFormat.Btex)
        {
            var finalPath = $"{pathNoExtension}.{format}";
            IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
            using var stream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            WriteBtex(stream);
        }
        else if (format == ImageFileFormat.Dds)
        {
            var finalPath = $"{pathNoExtension}.{format}";
            IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
            using var stream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            WriteDds(stream);
        }
        else
        {
            using var enumerator = (ITextureSurfaceEnumerator)GetEnumerator();
            var name = pathNoExtension.Split(Path.DirectorySeparatorChar)[^1];

            for (var i = 0; i < ImageHeader.ArrayCount; i++)
            {
                var finalPath = ImageHeader.ArrayCount > 1
                    ? Path.Combine(pathNoExtension, $"{arrayFileNameSelector(i, name)}.{format}")
                    : $"{pathNoExtension}.{format}";
                IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
                using var stream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None);
                WriteOther(stream, enumerator, i, format);
            }
        }
    }

    /// <summary>
    /// Writes the BTEX file directly to the stream.
    /// </summary>
    /// <param name="destination">Stream to write the texture to.</param>
    /// <remarks>
    /// Will deswizzle pixels prior to writing if the texture is swizzled.
    /// </remarks>
    private void WriteBtex(Stream destination)
    {
        if (ImageHeader.Flags.HasFlag(BlackTextureImageFlags.SWIZZLE))
        {
            throw new NotImplementedException("Haven't implemented deswizzling for BTEX export yet.");
        }

        destination.Write(_buffer.Span);
    }

    /// <summary>
    /// Converts the texture to the Direct Draw Surface (DDS) format.
    /// </summary>
    /// <param name="destination">Stream to write the converted texture to.</param>
    private void WriteDds(Stream destination)
    {
        // Write a DDS header for this texture to a new buffer, then write that to the stream
        using var ddsBuffer = MemoryOwner<byte>.Allocate(
            DirectDrawSurfaceHeader.StructSize, AllocationMode.Clear);
        WriteDdsHeader(ddsBuffer.Span);
        destination.Write(ddsBuffer.Span);

        // Iterate each surface in the texture
        foreach (var surface in this)
        {
            var surfaceSpan = surface.Data.Span;

            // Deswizzle if needed
            if (ImageHeader.Flags.HasFlag(BlackTextureImageFlags.SWIZZLE))
            {
                var deswizzledBuffer = new byte[surface.Size];
                var deswizzled = new Span<byte>(deswizzledBuffer);
                _strategy.DeswizzleSurface(surfaceSpan, deswizzled, surface.Width, surface.Height);
                surfaceSpan = deswizzled;
            }

            // Write the surface to the stream
            // Deliberately not aligning surface to BlockSize like BTEX does, as this is not standard for DDS
            destination.Write(surfaceSpan);
        }
    }

    /// <summary>
    /// Converts one image in the texture to a standard image format.
    /// </summary>
    /// <param name="destination">Stream to write the converted image to.</param>
    /// <param name="enumerator">Surface enumerator.</param>
    /// <param name="index">Index of the image to convert.</param>
    /// <param name="format">Image format to convert to.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if the <paramref name="format" /> is not JPEG, PNG, or TGA.
    /// </exception>
    private void WriteOther(Stream destination, ITextureSurfaceEnumerator enumerator, int index, ImageFileFormat format)
    {
        var element = enumerator.ElementAt(index, 0);
        var surfaceSpan = element.Data.Span;

        if (ImageHeader.Flags.HasFlag(BlackTextureImageFlags.SWIZZLE))
        {
            var deswizzledBuffer = new byte[element.Size];
            var deswizzled = new Span<byte>(deswizzledBuffer);
            _strategy.DeswizzleSurface(surfaceSpan, deswizzled, element.Width, element.Height);
            surfaceSpan = deswizzled;
        }

        using var surface = new NvttSurface(surfaceSpan,
            ImageHeader.Format,
            ImageHeader.Width,
            ImageHeader.Height);
        surface.Save(destination, format == ImageFileFormat.Jpeg
            ? new JpegEncoder()
            : format == ImageFileFormat.Png
                ? new PngEncoder()
                : format == ImageFileFormat.Targa
                    ? new TgaEncoder()
                    : throw new NotSupportedException($"Unsupported image format {format}"));
    }

    /// <summary>
    /// Writes a DDS header with the correct properties for this texture.
    /// </summary>
    /// <param name="destination">Buffer to write the header to.</param>
    private void WriteDdsHeader(Span<byte> destination)
    {
        ref var start = ref MemoryMarshal.GetReference(destination);
        ref var magic = ref Unsafe.As<byte, uint>(ref start);
        magic = DirectDrawSurfaceHeader.MagicValue;

        ref var ddsHeader = ref Unsafe.As<byte, DirectDrawSurfaceHeader>(ref Unsafe.Add(ref start, sizeof(uint)));
        ddsHeader.Size = (uint)DirectDrawSurfaceHeader.StructSize;
        ddsHeader.Width = ImageHeader.Width;
        ddsHeader.Height = ImageHeader.Height;
        ddsHeader.Pitch = ImageHeader.Pitch;
        ddsHeader.Depth = ImageHeader.Depth;
        ddsHeader.MipMapCount = ImageHeader.MipMapCount;
        ddsHeader.Flags = DirectDrawSurfaceFlags.Texture
                          | DirectDrawSurfaceFlags.Pitch
                          | DirectDrawSurfaceFlags.Depth
                          | DirectDrawSurfaceFlags.MipMapCount;
        ddsHeader.PixelFormat = DirectDrawSurfacePixelFormat.Default;
        ddsHeader.Caps = DirectDrawSurfaceCaps.Texture;

        ref var dx10Header = ref Unsafe.As<byte, DirectX10Header>(ref Unsafe.Add(ref start,
            sizeof(uint) + Unsafe.SizeOf<DirectDrawSurfaceHeader>()));
        dx10Header.ArraySize = ImageHeader.ArrayCount;
        dx10Header.Format = ImageHeader.Flags.HasFlag(BlackTextureImageFlags.SRGB)
            ? PixelFormatMap.GetSrgb(ImageHeader.Format)
            : PixelFormatMap.Get(ImageHeader.Format);
        dx10Header.ResourceDimension = (DirectX10ResourceDimension)(ImageHeader.DimensionCount + 1u);
        dx10Header.MiscFlags = 0;
        dx10Header.MiscFlags2 = DirectX10MiscFlags2.Unknown;
    }

    /// <inheritdoc />
    public IEnumerator<TextureSurface> GetEnumerator()
    {
        var configuration = new TextureSurfaceEnumeratorConfiguration(
            ImageHeader.ArrayCount,
            ImageHeader.MipMapCount,
            ImageHeader.Width,
            ImageHeader.Height,
            Data,
            BtexHeader.Platform == BlackTexturePlatform.PLATFORM_PS4 ? 512 : null);
        return _strategy.GetEnumerator(configuration);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}