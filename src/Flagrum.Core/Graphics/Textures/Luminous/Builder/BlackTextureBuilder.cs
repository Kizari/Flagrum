using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Flagrum.Core.Data.Binary;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous.DataSources;
using Flagrum.Core.Graphics.Textures.NvidiaTextureTools;
using Flagrum.Core.Graphics.Textures.OpenExtendedRange;
using Flagrum.Core.Graphics.Textures.Shared;
using Flagrum.Core.Graphics.Textures.Shared.PixelFormats;
using Flagrum.Core.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Flagrum.Core.Graphics.Textures.Luminous.Builder;

/// <inheritdoc cref="IBlackTextureBuilder" />
public struct BlackTextureBuilder : IBlackTextureBuilder
{
    /// <summary>
    /// Alignment size for PC version of the game. Need to make dynamic to support other platforms.
    /// </summary>
    private const int BlockSize = 128;

    private readonly Configuration _configuration;
    private readonly BlackTexturePixelFormat _targetFormat;
    private readonly int _height;
    private readonly int _mipmapCount;
    private readonly string _name;
    private readonly int _width;
    private readonly List<List<IBlackTextureDataSource>> _images;
    private readonly IPixelFormatStrategy _outputStrategy;

    private readonly bool _isCubeMap;
    private readonly bool _isVolumeTexture;
    private readonly bool _isSrgb;
    private long _dataStart;

    // TODO: Sanity checks on width/height being powers of 2 when mips are enabled

    /// <summary>
    /// Initializes the builder with an existing texture.
    /// </summary>
    /// <param name="texture">Texture to initialize the builder with.</param>
    /// <remarks>
    /// Metadata from the input texture is used to format the builder.
    /// Existing surfaces in the texture are automatically added to the build list.
    /// </remarks>
    public BlackTextureBuilder(BlackTexture texture) : this(
        Encoding.UTF8.GetString(texture.Name),
        texture.ImageHeader.Width,
        texture.ImageHeader.Height,
        texture.ImageHeader.MipMapCount,
        texture.ImageHeader.Format,
        texture.ImageHeader.Flags.HasFlag(BlackTextureImageFlags.SRGB))
    {
        _isCubeMap = texture.ImageHeader.Flags.HasFlag(BlackTextureImageFlags.CUBE);
        _isVolumeTexture = texture.ImageHeader.Flags.HasFlag(BlackTextureImageFlags.VOLUME);
        _images = texture.GetImageDataSources();
        _outputStrategy = PixelFormatStrategyFactory.Create(_targetFormat);
    }

    /// <summary>
    /// Initializes the builder with an existing texture.
    /// </summary>
    /// <param name="texture">Texture to initialize the builder with.</param>
    /// <param name="name">Name to give to the texture.</param>
    /// <remarks>
    /// Metadata from the input texture is used to format the builder.
    /// Existing surfaces in the texture are automatically added to the build list.
    /// </remarks>
    public BlackTextureBuilder(DirectDrawSurface texture, string name) : this(
        name,
        (int)texture.DdsHeader.Width,
        (int)texture.DdsHeader.Height,
        (int)texture.DdsHeader.MipMapCount,
        PixelFormatMap.Get(texture.DX10Header.Format),
        PixelFormatMap.IsSrgb(texture.DX10Header.Format))
    {
        _isCubeMap = texture.DX10Header.MiscFlags.HasFlag(DirectX10MiscFlags.TextureCube);
        _isVolumeTexture = texture.DX10Header.ResourceDimension == DirectX10ResourceDimension.Texture3D;
        _images = texture.GetImageDataSources();
        _outputStrategy = PixelFormatStrategyFactory.Create(_targetFormat);
    }

    /// <summary>
    /// Initializes the builder with the given parameters.
    /// </summary>
    /// <param name="name">Name to give the texture.</param>
    /// <param name="width">Width of the texture, in pixels.</param>
    /// <param name="height">Height of the texture, in pixels.</param>
    /// <param name="isSrgb">Whether the texture is in the sRGB color space.</param>
    /// <param name="mipmapCount">
    /// Number of mipmaps per image in the texture.
    /// If less than 1, the appropriate number of mip levels will be computed automatically.
    /// </param>
    /// <param name="targetFormat">Pixel format to encode the texture with.</param>
    public BlackTextureBuilder(
        string name,
        int width,
        int height,
        int mipmapCount,
        BlackTexturePixelFormat targetFormat,
        bool isSrgb)
    {
        _name = name;
        _width = width;
        _height = height;
        _targetFormat = targetFormat;
        _isSrgb = isSrgb;
        _outputStrategy = PixelFormatStrategyFactory.Create(_targetFormat);
        _configuration = Configuration.Default.Clone();
        _configuration.PreferContiguousImageBuffers = true;
        _images = [];
        _mipmapCount = mipmapCount > 0
            ? mipmapCount
            : 1 + (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
    }

    /// <inheritdoc />
    public IBlackTextureBuilder AddImage(List<IBlackTextureDataSource> dataSources)
    {
        _images.Add(dataSources);
        return this;
    }

    /// <inheritdoc />
    public IBlackTextureBuilder AddRasterImage(RasterImageDataSource imageSource)
    {
        _images.Add([imageSource]);
        return this;
    }

    /// <inheritdoc />
    public byte[] Build()
    {
        using var stream = new MemoryStream();
        Build(stream);
        return stream.ToArray();
    }

    /// <inheritdoc />
    public void Build(Stream destination)
    {
        // Allocate memory for the metadata
        var metadataSize = BlackTexture.GetMetadataSizePC(_name, _images.Count, _mipmapCount);
        using var metadataBuffer = MemoryOwner<byte>.Allocate(metadataSize, AllocationMode.Clear);
        var metadata = metadataBuffer.Span;
        ref var start = ref MemoryMarshal.GetReference(metadata);

        // Skip to the image data section
        var destinationStart = destination.Position;
        destination.SetLength(destination.Length + metadataSize);
        destination.Seek(metadataSize, SeekOrigin.Current);
        _dataStart = destination.Position;

        // Get span over the memory region for the surface headers
        var pSurfaceHeaders = 128 + BlackTextureHeader.StructSize + BlackTextureImageHeader.StructSize;
        var surfaceCount = _images.Count * _mipmapCount;
        var surfaceHeadersSize = BlackTextureSurfaceHeader.StructSize * surfaceCount;
        var surfaceSpan = MemoryMarshal.Cast<byte, BlackTextureSurfaceHeader>(metadata
            .Slice(pSurfaceHeaders, surfaceHeadersSize));

        // Write each image
        for (var i = 0; i < _images.Count; i++)
        {
            WriteImage(destination, i, surfaceSpan);
        }
        
        // PC BTEX format is aligned to 128 at the end
        destination.Align(128);

        // Write SEDB header to the buffer
        ref var sedbHeader = ref Unsafe.As<byte, SectionDataBinaryHeader>(ref start);
        sedbHeader.Type = SectionDataBinaryHeader.TypeValue;
        sedbHeader.Subtype = BlackTextureHeader.SedbType;
        sedbHeader.Offset = 128;
        sedbHeader.Size = (ulong)(destination.Position - destinationStart);

        // Write BTEX header to the buffer
        ref var btexHeader = ref Unsafe.As<byte, BlackTextureHeader>(ref Unsafe.Add(ref start, sedbHeader.Offset));
        btexHeader.Magic = BlackTextureHeader.MagicValue;
        btexHeader.HeaderSize = (uint)(metadataSize - sedbHeader.Offset);
        btexHeader.ImageDataSize = (uint)(sedbHeader.Size - (ulong)metadataSize);
        btexHeader.Version = BlackTextureVersion.VERSION_LATEST;
        btexHeader.Platform = BlackTexturePlatform.PLATFORM_WIIU; // Used for PC for whatever reason
        btexHeader.Flags = BlackTextureFlags.FLAG_COMPOSITED_IMAGE;
        btexHeader.ImageCount = 1;
        btexHeader.ImageHeaderStride = (ushort)BlackTextureImageHeader.StructSize;
        btexHeader.ImageHeaderOffset = (ushort)BlackTextureHeader.StructSize; // Starts immediately after this header

        // Write image header to the buffer
        var pImageHeader = sedbHeader.Offset + btexHeader.ImageHeaderOffset;
        ref var imageHeader = ref Unsafe.As<byte, BlackTextureImageHeader>(ref Unsafe.Add(ref start, pImageHeader));
        imageHeader.Width = (ushort)_width;
        imageHeader.Height = (ushort)_height;
        imageHeader.Pitch = (ushort)_outputStrategy.GetRowPitch(_width);
        imageHeader.Format = _targetFormat;
        imageHeader.MipMapCount = (byte)_mipmapCount;
        imageHeader.Depth = 1;
        imageHeader.DimensionCount = 2;
        imageHeader.Flags = BlackTextureImageFlags.NONE;
        imageHeader.Flags |= _mipmapCount > 1 ? BlackTextureImageFlags.MIPMAP : 0;
        imageHeader.Flags |= _outputStrategy.IsCompressed ? BlackTextureImageFlags.COMPRESS : 0;
        imageHeader.Flags |= _images.Count > 1 ? BlackTextureImageFlags.ARRAY : 0;
        imageHeader.Flags |= _isCubeMap ? BlackTextureImageFlags.CUBE : 0;
        imageHeader.Flags |= _isVolumeTexture ? BlackTextureImageFlags.VOLUME : 0;
        imageHeader.Flags |= _isSrgb ? BlackTextureImageFlags.SRGB : 0;
        imageHeader.ArrayCount = (ushort)_images.Count;
        imageHeader.SurfaceCount = (ushort)surfaceCount;
        imageHeader.SurfaceHeaderStride = (ushort)BlackTextureSurfaceHeader.StructSize;
        imageHeader.SurfaceHeaderOffset = (uint)BlackTextureImageHeader.StructSize; // Immediately after this header
        imageHeader.NameOffset = (uint)(imageHeader.SurfaceHeaderOffset + surfaceHeadersSize); // After surfaces
        // Textures seem to work fine without these, it's unclear if they're not used
        // or if defining them would improve performance (such as if the engine has to recalculate at runtime)
        imageHeader.HighResolutionMipCount = 0; // TODO: Calculate this properly (does it matter?)
        imageHeader.HighResolutionBtexSize = 0; // TODO: Get this from the high res texture (does it matter?)

        // Write image name
        var nameSlice = metadata[(int)(pImageHeader + imageHeader.NameOffset)..];
        Encoding.UTF8.GetBytes(_name).CopyTo(nameSlice);
    }

    /// <summary>
    /// Writes one image (including all of its mip maps) to the output stream.
    /// </summary>
    /// <param name="destination">Stream to write the image to.</param>
    /// <param name="imageIndex">Index of the image in the texture array.</param>
    /// <param name="surfaceHeaders">View of the surface header memory.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if any images have mip maps already, but fewer than needed to fill all mip levels.
    /// </exception>
    /// <exception cref="NotSupportedException">Thrown if mip generation isn't available for the input.</exception>
    private void WriteImage(Stream destination, int imageIndex, Span<BlackTextureSurfaceHeader> surfaceHeaders)
    {
        // Validate input
        var mipSources = _images[imageIndex];
        if (mipSources.Count != 1 && mipSources.Count != _mipmapCount)
        {
            throw new InvalidOperationException("Cannot process image with partial mips. " +
                                                "Should either have one image for automatic mip generation, " +
                                                "or all mip levels prepopulated.");
        }

        // Handle raster image files
        if (mipSources.Count == 1)
        {
            var source = mipSources[0];
            if (source is not RasterImageDataSource)
            {
                throw new NotSupportedException("Mip generation not supported for non-raster image types.");
            }

            WriteRasterImage(imageIndex, destination, source, surfaceHeaders);
            return;
        }

        // Handle container texture files
        foreach (var mipSource in mipSources)
        {
            WriteRaw(destination, mipSource);
        }
    }

    /// <summary>
    /// Writes raw pixel information for a surface directly to the output stream.
    /// </summary>
    /// <param name="destination">Stream to write the surface data to.</param>
    /// <param name="source">Source of the surface data.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the source data is not in the expected pixel format.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteRaw(Stream destination, IBlackTextureDataSource source)
    {
        if (source.Format != _targetFormat)
        {
            throw new InvalidOperationException("Pixel format does not match.");
        }

        using var sourceStream = source.OpenStream();
        sourceStream.CopyTo(destination);
    }

    /// <summary>
    /// Writes an image from a standard raster image format to the output stream.
    /// </summary>
    /// <param name="imageIndex">Index of this image in the texture array.</param>
    /// <param name="destination">Stream to write the image data to.</param>
    /// <param name="source">Source of the raster image data.</param>
    /// <param name="surfaceHeaders">View of the surface headers in memory.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the source image dimensions do not match the dimensions configured on the builder.
    /// </exception>
    /// <exception cref="BlackTextureBuilderInvalidSourceException">
    /// Thrown if an HDR target pixel format is set on the builder, but the source data is not HDR.
    /// </exception>
    /// <remarks>
    /// Mip maps will be generated automatically if applicable.
    /// Data will be converted to the target pixel format automatically.
    /// </remarks>
    private void WriteRasterImage(
        int imageIndex,
        Stream destination,
        IBlackTextureDataSource source,
        Span<BlackTextureSurfaceHeader> surfaceHeaders)
    {
        // Validate image dimensions
        if (source.Width != _width || source.Height != _height)
        {
            throw new InvalidOperationException($"Image was incorrectly sized at {source.Width}x{source.Height}." +
                                                $" Expected {_width}x{_height}.");
        }

        // Compute attributes relating to this operation
        var attributes = BlackTextureImageBuildFlags.None;
        attributes |= PixelFormatMap.IsCompressed(_targetFormat) ? BlackTextureImageBuildFlags.OutputCompressed : 0;
        attributes |= PixelFormatMap.IsHighDynamicRange(source.Format) ? BlackTextureImageBuildFlags.InputHDR : 0;
        attributes |= PixelFormatMap.IsHighDynamicRange(_targetFormat) ? BlackTextureImageBuildFlags.OutputHDR : 0;

        // Validate source against target format
        if (attributes.HasFlag(BlackTextureImageBuildFlags.OutputHDR)
            && !attributes.HasFlag(BlackTextureImageBuildFlags.InputHDR))
        {
            throw new BlackTextureBuilderInvalidSourceException(
                $"Target pixel format {_targetFormat} requires an HDR image.");
        }

        // Read the image from the data source
        using var sourceStream = source.OpenStream();
        using var image = attributes.HasFlag(BlackTextureImageBuildFlags.InputHDR)
            ? OpenExr.ReadPlanarPixelData(sourceStream)
            : Image.Load<Rgba32>(new DecoderOptions {Configuration = _configuration}, sourceStream);

        // Iterate each mip level
        for (var mip = 0; mip < _mipmapCount; mip++)
        {
            // Write the offset for the current mip map
            var offset = (uint)(destination.Position - _dataStart);
            surfaceHeaders[imageIndex * _mipmapCount + mip].Offset = offset;

            // Write the image data for the current mip map
            var width = Math.Max(1, _width >> mip);
            var height = Math.Max(1, _height >> mip);
            var mipImage = mip == 0
                ? image
                : image.Clone(context => context.Resize(width, height, new LanczosResampler()));

            _ = source.Format switch
            {
                BlackTexturePixelFormat.A8R8G8B8 => WriteSurface<Argb32>(destination, mipImage, attributes),
                BlackTexturePixelFormat.B8 => WriteSurface<B8>(destination, mipImage, attributes),
                BlackTexturePixelFormat.G8R8_UNORM => WriteSurface<GR16>(destination, mipImage, attributes),
                BlackTexturePixelFormat.R16G16B16A16_FLOAT => WriteSurface<RgbaHalfVector>(destination, mipImage,
                    attributes),
                BlackTexturePixelFormat.R16G16B16A16_UNORM => WriteSurface<Rgba64>(destination, mipImage, attributes),
                BlackTexturePixelFormat.R32_FLOAT => WriteSurface<R32F>(destination, mipImage, attributes),
                BlackTexturePixelFormat.R32G32B32A32_FLOAT => WriteSurface<RgbaVector>(destination, mipImage,
                    attributes),
                BlackTexturePixelFormat.R8G8B8A8_UNORM => WriteSurface<Rgba32>(destination, mipImage, attributes),
                _ => throw new NotSupportedException($"Unsupported input pixel format {source.Format}.")
            };

            mipImage.Dispose();

            // Write the size of the current mip map, then align to the required block size
            surfaceHeaders[imageIndex * _mipmapCount + mip].Size = (uint)(destination.Position - offset);
            destination.Align(BlockSize);
        }
    }

    /// <summary>
    /// Writes a single surface to the output stream.
    /// </summary>
    /// <param name="destination">Stream to write the surface to.</param>
    /// <param name="image">Image data for the surface.</param>
    /// <param name="attributes">Attributes that apply to this operation.</param>
    /// <typeparam name="TSource">Type of the pixel data in the source image.</typeparam>
    /// <returns>Only returns a value so it can be used in a switch expression. Sue me.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if an unsupported pixel format conversion is required.
    /// </exception>
    private bool WriteSurface<TSource>(
        Stream destination,
        Image image,
        BlackTextureImageBuildFlags attributes)
        where TSource : unmanaged, IPixel<TSource>
    {
        var sourceFormat = PixelFormatMap.Get(image);

        // Write pixel data directly if already in correct format
        if (sourceFormat == _targetFormat)
        {
            var typedImage = (Image<TSource>)image;
            typedImage.DangerousTryGetSinglePixelMemory(out var memory);
            destination.Write(MemoryMarshal.Cast<TSource, byte>(memory.Span));
            return true;
        }

        // Stream block-compressed data if target format is compressed
        if (attributes.HasFlag(BlackTextureImageBuildFlags.OutputCompressed))
        {
            using var uncompressed = image.CloneAs<RgbaVector>(); // NVTT requires data in this format
            using var surface = new NvttSurface(uncompressed);
            surface.Compress(destination, _targetFormat);
            return true;
        }

        // Conversion is required, convert to the target format
        return _targetFormat switch
        {
            BlackTexturePixelFormat.A8R8G8B8 => WriteSurfaceConverted<Argb32>(destination, image),
            BlackTexturePixelFormat.B8 => WriteSurfaceConverted<B8>(destination, image),
            BlackTexturePixelFormat.G8R8_UNORM => WriteSurfaceConverted<GR16>(destination, image),
            BlackTexturePixelFormat.R8G8B8A8_UNORM => WriteSurfaceConverted<Rgba32>(destination, image),
            _ => throw new NotSupportedException(
                $"Cannot convert raster image to Unsupported pixel format {_targetFormat}.")
        };
    }

    /// <summary>
    /// Converts a single surface to the target pixel format and writes it to the output stream.
    /// </summary>
    /// <param name="destination">Stream to write the converted surface to.</param>
    /// <param name="source">Unconverted image.</param>
    /// <typeparam name="TTarget">Pixel format to convert to.</typeparam>
    /// <returns>Only returns a value so it can be used in a switch expression. Sue me.</returns>
    private static bool WriteSurfaceConverted<TTarget>(
        Stream destination,
        Image source)
        where TTarget : unmanaged, IPixel<TTarget>
    {
        using var converted = source.CloneAs<TTarget>();
        converted.DangerousTryGetSinglePixelMemory(out var memory);
        destination.Write(MemoryMarshal.Cast<TTarget, byte>(memory.Span));
        return true;
    }
}