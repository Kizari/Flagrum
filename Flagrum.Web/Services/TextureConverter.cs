﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Web.Features.Settings.Data;

namespace Flagrum.Web.Services;

public class TextureConverter
{
    private readonly LuminousGame _profile;
    
    public TextureConverter(LuminousGame profile)
    {
        _profile = profile;
    }
    
    public byte[] ProcessEarcModThumbnail(byte[] data)
    {
        try
        {
            var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
            var pointer = pinnedData.AddrOfPinnedObject();

            var initialImage = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);
            pinnedData.Free();
            
            var metadata = initialImage.GetMetadata();
            var image = initialImage;

            if (metadata.Format != DXGI_FORMAT.R8G8B8A8_UNORM)
            {
                var filter = TexHelper.Instance.IsSRGB(metadata.Format)
                    ? TEX_FILTER_FLAGS.SRGB
                    : TEX_FILTER_FLAGS.DEFAULT;
                
                image = initialImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, filter, 0.5f);
                initialImage.Dispose();
            }

            // Resize to 326x170 without stretching
            const int desiredWidth = 326;
            const int desiredHeight = 170;

            if (metadata.Width != desiredWidth || metadata.Height != desiredHeight)
            {
                var aspectRatioHeight = metadata.Height / (double)metadata.Width;
                var aspectRatioWidth = metadata.Width / (double)metadata.Height;

                var ratioedWidth = (int)(desiredHeight * aspectRatioWidth);
                var ratioedHeight = (int)(desiredWidth * aspectRatioHeight);

                var differenceWidth = Math.Abs(metadata.Width - ratioedWidth);
                var differenceHeight = Math.Abs(metadata.Height - ratioedHeight);

                int newWidth;
                int newHeight;

                if (differenceWidth < differenceHeight && ratioedWidth >= desiredWidth)
                {
                    newWidth = ratioedWidth;
                    newHeight = desiredHeight;
                }
                else
                {
                    newHeight = ratioedHeight;
                    newWidth = desiredWidth;
                }

                // Resize the image to the new size
                var previousImage = image;
                image = previousImage.Resize(newWidth, newHeight, TEX_FILTER_FLAGS.CUBIC);
                previousImage.Dispose();

                // Truncate the resized image to the desired frame
                metadata = image.GetMetadata();
                var destinationImage =
                    TexHelper.Instance.Initialize2D(DXGI_FORMAT.R8G8B8A8_UNORM, desiredWidth, desiredHeight, 1, 1,
                        CP_FLAGS.NONE);
                var xOffset = Math.Abs((desiredWidth - metadata.Width) / 2);
                var yOffset = Math.Abs((desiredHeight - metadata.Height) / 2);
                TexHelper.Instance.CopyRectangle(image.GetImage(0), xOffset, yOffset, desiredWidth, desiredHeight,
                    destinationImage.GetImage(0), TEX_FILTER_FLAGS.DEFAULT, 0, 0);

                previousImage = image;
                image = destinationImage;
                previousImage.Dispose();
            }

            using var jpgStream = image.SaveToJPGMemory(0, 1.0f);
            image.Dispose();
            using var stream = new MemoryStream();
            jpgStream.CopyTo(stream);
            return stream.ToArray();
        }
        catch
        {
            return data;
        }
    }

    public byte[] ConvertPreview(string name, byte[] data, out byte[] jpeg)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);
        pinnedData.Free();
        
        var metadata = image.GetMetadata();

        if (metadata.Format != DXGI_FORMAT.R8G8B8A8_UNORM)
        {
            var filter = TexHelper.Instance.IsSRGB(metadata.Format) ? TEX_FILTER_FLAGS.SRGB : TEX_FILTER_FLAGS.DEFAULT;
            var previousImage = image;
            image = previousImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, filter, 0.5f);
            previousImage.Dispose();
        }

        if (!(metadata.Width == 600 && metadata.Height == 600))
        {
            // Resize to the mandatory 600x600 without stretching
            if (metadata.Width > metadata.Height)
            {
                var aspectRatio = metadata.Height / (double)metadata.Width;
                var height = (int)(600 * aspectRatio);

                var previousImage = image;
                image = previousImage.Resize(600, height, TEX_FILTER_FLAGS.CUBIC);
                previousImage.Dispose();
            }
            else if (metadata.Height > metadata.Width)
            {
                var aspectRatio = metadata.Width / (double)metadata.Height;
                var width = (int)(600 * aspectRatio);
                
                var previousImage = image;
                image = previousImage.Resize(width, 600, TEX_FILTER_FLAGS.CUBIC);
                previousImage.Dispose();
            }
            else
            {
                var previousImage = image;
                image = previousImage.Resize(600, 600, TEX_FILTER_FLAGS.CUBIC);
                previousImage.Dispose();
            }

            metadata = image.GetMetadata();

            // Add black bars if image isn't square
            if (metadata.Width != metadata.Height)
            {
                var destinationImage =
                    TexHelper.Instance.Initialize2D(DXGI_FORMAT.R8G8B8A8_UNORM, 600, 600, 1, 1, CP_FLAGS.NONE);
                var xOffset = (600 - metadata.Width) / 2;
                var yOffset = (600 - metadata.Height) / 2;
                TexHelper.Instance.CopyRectangle(image.GetImage(0), 0, 0, metadata.Width, metadata.Height,
                    destinationImage.GetImage(0), TEX_FILTER_FLAGS.DEFAULT, xOffset, yOffset);

                var previousImage = image;
                image = destinationImage;
                previousImage.Dispose();
            }
        }

        // Ensure image is within the size limit for the Steam upload
        var quality = 1.0f;

        while (true)
        {
            using var jpgStream = image.SaveToJPGMemory(0, quality);
            if (jpgStream.Length <= 953673)
            {
                using var stream = new MemoryStream();
                jpgStream.CopyTo(stream);
                jpeg = stream.ToArray();
                break;
            }

            quality -= 0.05f;
        }

        var previousImage2 = image;
        image = previousImage2.Compress(DXGI_FORMAT.BC1_UNORM, TEX_COMPRESS_FLAGS.SRGB_OUT | TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
        previousImage2.Dispose();

        return BuildBtex(TextureType.Preview, name, image);
    }

    public byte[] ConvertThumbnail(string name, byte[] data)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);
        pinnedData.Free();

        var metadata = image.GetMetadata();
        if (!(metadata.Width == 168 && metadata.Height == 242))
        {
            var beforeResizeImage = image;
            image = beforeResizeImage.Resize(168, 242, TEX_FILTER_FLAGS.CUBIC);
            beforeResizeImage.Dispose();
        }

        // Have to do some kind of conversion to force the janky SRGB out conversion
        // Which bleaches the image to compensate for SE's jank
        // DirectXTex throws a fit if converting to the same format, so arbitrarily picked R16G16B16A16_UNORM
        var previousImage = image;
        image = previousImage.Convert(DXGI_FORMAT.R16G16B16A16_UNORM, TEX_FILTER_FLAGS.SRGB_OUT, 0.5f);
        previousImage.Dispose();

        // Then need to convert back to the desired format
        previousImage = image;
        image = previousImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);
        previousImage.Dispose();

        var result = BuildBtex(TextureType.Thumbnail, name, image);
        return result;
    }

    public byte[] ToBtex(string name, string extension, TextureType type, byte[] data)
    {
        return extension.ToLower() switch
        {
            "tga" => ConvertTarga(type, name, data),
            "dds" => ConvertDds(type, name, data),
            "btex" => data,
            _ => ConvertWic(type, name, data)
        };
    }

    public byte[] ToBtex(BtexBuildRequest request)
    {
        var pinnedData = GCHandle.Alloc(request.SourceData, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = request.SourceFormat switch
        {
            BtexSourceFormat.Wic => TexHelper.Instance.LoadFromWICMemory(pointer, request.SourceData.Length,
                WIC_FLAGS.NONE),
            BtexSourceFormat.Targa => TexHelper.Instance.LoadFromTGAMemory(pointer, request.SourceData.Length),
            BtexSourceFormat.Dds => TexHelper.Instance.LoadFromDDSMemory(pointer, request.SourceData.Length, DDS_FLAGS.NONE),
            _ => throw new NotImplementedException("Unsupported source format")
        };
        
        pinnedData.Free();

        if (request.MipLevels != 1)
        {
            var beforeMipsImage = image;
            image = beforeMipsImage.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, 0);
            beforeMipsImage.Dispose();
        }

        var dxgiFormat = (DXGI_FORMAT)BtexConverter.FormatMap[request.PixelFormat];
        var metadata = image.GetMetadata();

        // If the image is already DDS, let the user commit their own sins
        if (metadata.Format != dxgiFormat && request.SourceFormat != BtexSourceFormat.Dds)
        {
            var beforeConversionImage = image;
            image = TexHelper.Instance.IsCompressed(dxgiFormat)
                ? beforeConversionImage.Compress(dxgiFormat, TEX_COMPRESS_FLAGS.SRGB | TEX_COMPRESS_FLAGS.PARALLEL, 0.5f)
                : beforeConversionImage.Convert(dxgiFormat, TEX_FILTER_FLAGS.SRGB, 0.5f);
            beforeConversionImage.Dispose();
        }
        
        TexHelper.Instance.ComputePitch(dxgiFormat, metadata.Width, metadata.Height, out var rowPitch, out _,
            CP_FLAGS.NONE);

        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.FORCE_DX10_EXT | DDS_FLAGS.FORCE_DX10_EXT_MISC2);
        image.Dispose();
        ddsStream.CopyTo(stream);

        return BtexConverter.DdsToBtex(request.Name, stream.ToArray(), request.ImageFlags, rowPitch,
            (DxgiFormat)dxgiFormat, request.AddSedbHeader);
    }

    public byte[] BtexToDds(byte[] btex)
    {
        var dds = BtexConverter.BtexToDds(btex, _profile, (format, width, height) =>
        {
            TexHelper.Instance.ComputePitch((DXGI_FORMAT)format, width, height, out var rowPitch, out var slicePitch, CP_FLAGS.NONE);
            return (rowPitch, slicePitch);
        });

        var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);

        pinnedData.Free();

        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
        image.Dispose();
        ddsStream.CopyTo(stream);
        return stream.ToArray();
    }

    public byte[] BtexToJpg(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        using var stream = new MemoryStream();
        using var jpgStream = image.SaveToJPGMemory(0, 1.0f);
        image.Dispose();
        jpgStream.CopyTo(stream);
        return stream.ToArray();
    }

    public IEnumerable<byte[]> BtexToJpgs(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        var metadata = image.GetMetadata();

        for (var i = 0; i < image.GetImageCount(); i += metadata.MipLevels)
        {
            using var stream = new MemoryStream();
            using var jpgStream = image.SaveToJPGMemory(i, 1.0f);
            jpgStream.CopyTo(stream);
            yield return stream.ToArray();
        }
        
        image.Dispose();
    }

    public byte[] BtexToPng(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        using var stream = new MemoryStream();
        using var pngStream =
            image.SaveToWICMemory(0, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
        image.Dispose();
        pngStream.CopyTo(stream);
        return stream.ToArray();
    }
    
    public IEnumerable<byte[]> BtexToPngs(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        var metadata = image.GetMetadata();

        for (var i = 0; i < image.GetImageCount(); i += metadata.MipLevels)
        {
            using var stream = new MemoryStream();
            using var pngStream =
                image.SaveToWICMemory(i, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
            pngStream.CopyTo(stream);
            yield return stream.ToArray();
        }
        
        image.Dispose();
    }

    public byte[] BtexToTga(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        using var stream = new MemoryStream();
        using var tgaStream = image.SaveToTGAMemory(0);
        image.Dispose();
        tgaStream.CopyTo(stream);
        return stream.ToArray();
    }
    
    public IEnumerable<byte[]> BtexToTgas(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        var metadata = image.GetMetadata();

        for (var i = 0; i < image.GetImageCount(); i += metadata.MipLevels)
        {
            using var stream = new MemoryStream();
            using var tgaStream = image.SaveToTGAMemory(i);
            tgaStream.CopyTo(stream);
            yield return stream.ToArray();
        }
        
        image.Dispose();
    }

    private ScratchImage BtexToScratchImage(byte[] btex)
    {
        var dds = BtexConverter.BtexToDds(btex, _profile, (format, width, height) =>
        {
            TexHelper.Instance.ComputePitch((DXGI_FORMAT)format, width, height, out var rowPitch, out var slicePitch, CP_FLAGS.NONE);
            return (rowPitch, slicePitch);
        });

        var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);

        pinnedData.Free();

        var metadata = image.GetMetadata();
        if (TexHelper.Instance.IsCompressed(metadata.Format))
        {
            var previousImage = image;
            image = previousImage.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
            previousImage.Dispose();
        }

        return image;
    }

    public byte[] TargaToDds(byte[] wicData, int mips, DXGI_FORMAT format)
    {
        var pinnedData = GCHandle.Alloc(wicData, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromTGAMemory(pointer, wicData.Length);
        pinnedData.Free();

        if (mips != 1)
        {
            var previousImage = image;
            image = previousImage.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, mips);
            previousImage.Dispose();
        }

        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(0, DDS_FLAGS.NONE);
        image.Dispose();
        ddsStream.CopyTo(stream);
        return stream.ToArray();
    }

    private byte[] ConvertWic(TextureType type, string name, byte[] data)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);
        pinnedData.Free();
        
        image = BuildDds(type, image);
        
        return BuildBtex(type, name, image);
    }

    private byte[] ConvertTarga(TextureType type, string name, byte[] data)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromTGAMemory(pointer, data.Length);
        pinnedData.Free();
        
        image = BuildDds(type, image);

        return BuildBtex(type, name, image);
    }

    private byte[] ConvertDds(TextureType type, string name, byte[] data)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromDDSMemory(pointer, data.Length, DDS_FLAGS.NONE);
        pinnedData.Free();
        
        return BuildBtex(type, name, image);
    }

    private ScratchImage BuildDds(TextureType type, ScratchImage image)
    {
        ScratchImage previousImage;
        
        switch (type)
        {
            case TextureType.Normal:
                previousImage = image;
                image = previousImage.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, 0);
                previousImage.Dispose();

                previousImage = image;
                image = previousImage.Compress(DXGI_FORMAT.BC5_UNORM, TEX_COMPRESS_FLAGS.SRGB | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f);
                previousImage.Dispose();
                break;
            case TextureType.AmbientOcclusion or TextureType.Opacity:
                previousImage = image;
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, 0);
                previousImage.Dispose();
                
                previousImage = image;
                image = image.Compress(DXGI_FORMAT.BC4_UNORM, TEX_COMPRESS_FLAGS.SRGB | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f);
                previousImage.Dispose();
                break;
            case TextureType.MenuSprites:
                var metadata = image.GetMetadata();
                if (metadata.Format != DXGI_FORMAT.B8G8R8A8_UNORM)
                {
                    previousImage = image;
                    image = image.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, TEX_FILTER_FLAGS.SRGB, 0.5f);
                    previousImage.Dispose();
                }

                break;
            default:
                previousImage = image;
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, 0);
                previousImage.Dispose();
                
                previousImage = image;
                image = image.Compress(DXGI_FORMAT.BC1_UNORM, TEX_COMPRESS_FLAGS.SRGB | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f);
                previousImage.Dispose();
                break;
        }

        return image;
    }

    private byte[] BuildBtex(TextureType type, string name, ScratchImage image)
    {
        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.FORCE_DX10_EXT | DDS_FLAGS.FORCE_DX10_EXT_MISC2);
        image.Dispose();
        ddsStream.CopyTo(stream);

        return BtexConverter.DdsToBtex(type, name, stream.ToArray());
    }
    
    public void ExportTexture(string absoluteFilePath, ImageFormat targetFormat, byte[] btexData)
    {
        var outputPathNoExtension = absoluteFilePath[..absoluteFilePath.LastIndexOf('.')];

        // ReSharper disable twice PossibleUnintendedReferenceComparison
        if (targetFormat == ImageFormat.Btex)
        {
            var finalPath = $"{outputPathNoExtension}.{targetFormat}";
            IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
            File.WriteAllBytes(finalPath, btexData);
        }
        else if (targetFormat == ImageFormat.Dds)
        {
            var finalPath = $"{outputPathNoExtension}.{targetFormat}";
            IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
            File.WriteAllBytes(finalPath, BtexToDds(btexData));
        }
        else
        {
            List<byte[]> images;
            if (targetFormat == ImageFormat.Png)
            {
                images = BtexToPngs(btexData).ToList();
            }
            else if (targetFormat == ImageFormat.Targa)
            {
                images = BtexToTgas(btexData).ToList();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(targetFormat), targetFormat,
                    @"Unsupported texture format");
            }

            if (images.Count > 1)
            {
                var name = outputPathNoExtension.Split('\\').Last();

                for (var i = 0; i < images.Count; i++)
                {
                    var finalPath = $@"{outputPathNoExtension}\{name}.1{(i + 1).WithTrailingZeros(3)}.{targetFormat}";
                    IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
                    File.WriteAllBytes(finalPath, images[i]);
                }
            }
            else
            {
                var finalPath = $"{outputPathNoExtension}.{targetFormat}";
                IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
                File.WriteAllBytes(finalPath, images[0]);
            }
        }
    }
}

public class BtexBuildRequest
{
    public string Name { get; set; }
    public BtexFormat PixelFormat { get; set; }
    public byte ImageFlags { get; set; }
    public int MipLevels { get; set; }
    public BtexSourceFormat SourceFormat { get; set; }
    public byte[] SourceData { get; set; }
    public bool AddSedbHeader { get; set; }
}

public enum BtexSourceFormat
{
    Wic,
    Targa,
    Dds
}