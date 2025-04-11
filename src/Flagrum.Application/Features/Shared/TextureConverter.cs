using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Abstractions;
using Flagrum.Core.Graphics.Terrain;
using Flagrum.Core.Graphics.Textures;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Services;
using Flagrum.Application.Utilities;

namespace Flagrum.Application.Features.Shared;

public class TextureConverter(IProfileService profile)
{
    public BlackTexture ToBtex(TextureConversionRequest request)
    {
        // Load the image data
        var pinnedData = GCHandle.Alloc(request.SourceData, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = request.SourceFormat switch
        {
            TextureSourceFormat.Wic => TexHelper.Instance.LoadFromWICMemory(pointer, request.SourceData.Length,
                WIC_FLAGS.NONE),
            TextureSourceFormat.Targa => TexHelper.Instance.LoadFromTGAMemory(pointer, request.SourceData.Length),
            TextureSourceFormat.Dds => TexHelper.Instance.LoadFromDDSMemory(pointer, request.SourceData.Length,
                DDS_FLAGS.NONE),
            _ => throw new NotImplementedException("Unsupported source format")
        };

        pinnedData.Free();

        // Generate mipmaps if required
        if (request.MipLevels != 1)
        {
            var beforeMipsImage = image;
            image = beforeMipsImage.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, 0);
            beforeMipsImage.Dispose();
        }

        // Get image metadata
        var dxgiFormat = (DXGI_FORMAT)TexturePixelFormatMap.Instance[request.PixelFormat];

        // Force BC3 if the target is BC7 due to issues with converting textures to BC7
        // See https://github.com/Kizari/Flagrum-Dev/issues/14
        if (dxgiFormat == DXGI_FORMAT.BC7_UNORM)
        {
            dxgiFormat = DXGI_FORMAT.BC3_UNORM;
        }

        var metadata = image.GetMetadata();

        // Convert the pixel data to the appropriate format if required
        if (metadata.Format != dxgiFormat && request.SourceFormat != TextureSourceFormat.Dds)
        {
            var beforeConversionImage = image;
            image = TexHelper.Instance.IsCompressed(dxgiFormat)
                ? beforeConversionImage.Compress(dxgiFormat, TEX_COMPRESS_FLAGS.SRGB | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f)
                : beforeConversionImage.Convert(dxgiFormat, TEX_FILTER_FLAGS.SRGB, 0.5f);
            beforeConversionImage.Dispose();
        }

        // Calculate the image pitch
        // TexHelper.Instance.ComputePitch(dxgiFormat, metadata.Width, metadata.Height, out var rowPitch, out _,
        //     CP_FLAGS.NONE);
        var isCompressed = TexHelper.Instance.IsCompressed(dxgiFormat);

        // Copy the DDS into managed memory
        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.FORCE_DX10_EXT | DDS_FLAGS.FORCE_DX10_EXT_MISC2);
        image.Dispose();
        ddsStream.CopyTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        // Convert the DDS to BTEX
        var dds = new DirectDrawSurface();
        dds.Read(stream);
        return dds.ToBtex(request.Game, request.Name, request.ImageFlags, isCompressed);
    }

    public byte[] ToDds(byte[] btex)
    {
        var image = BtexToScratchImage(btex);

        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
        image.Dispose();
        ddsStream.CopyTo(stream);
        return stream.ToArray();
    }

    public byte[] ToJpeg(byte[] btex) => ToJpegs(btex).First();

    public byte[] ToPng(byte[] btex) => ToPngs(btex).First();

    public byte[] ToTarga(byte[] btex) => ToTargas(btex).First();

    public IEnumerable<byte[]> ToJpegs(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        var metadata = image.GetMetadata();

        for (var i = 0; i < image.GetImageCount(); i += metadata.MipLevels)
        {
            yield return image.ToJpeg(i);
        }

        image.Dispose();
    }

    public IEnumerable<byte[]> ToPngs(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        var metadata = image.GetMetadata();

        for (var i = 0; i < image.GetImageCount(); i += metadata.MipLevels)
        {
            yield return image.ToPng(i);
        }

        image.Dispose();
    }

    public IEnumerable<byte[]> ToTargas(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        var metadata = image.GetMetadata();

        if (TexHelper.Instance.BitsPerPixel(metadata.Format) > 32)
        {
            var previousImage = image;
            image = previousImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);
            previousImage.Dispose();
        }

        for (var i = 0; i < image.GetImageCount(); i += metadata.MipLevels)
        {
            yield return image.ToTarga(i);
        }

        image.Dispose();
    }

    public byte[] WicToEarcThumbnailJpeg(byte[] data)
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

    public byte[] WicToBinmodPreviewBlackTexture(string name, byte[] data, out byte[] jpegImage)
    {
        // Load the image data
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();
        var image = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);
        pinnedData.Free();

        // Convert the image to uncompressed 32bit RGBA if not already
        var metadata = image.GetMetadata();
        if (metadata.Format != DXGI_FORMAT.R8G8B8A8_UNORM)
        {
            var filter = TexHelper.Instance.IsSRGB(metadata.Format) ? TEX_FILTER_FLAGS.SRGB : TEX_FILTER_FLAGS.DEFAULT;
            var previousImage = image;
            image = previousImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, filter, 0.5f);
            previousImage.Dispose();
        }

        // Resize the image if needed
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
                jpegImage = stream.ToArray();
                break;
            }

            quality -= 0.05f;
        }

        // Compress the image that will be used for the BTEX
        var previousImage2 = image;
        image = previousImage2.Compress(DXGI_FORMAT.BC1_UNORM,
            TEX_COMPRESS_FLAGS.SRGB_OUT | TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
        previousImage2.Dispose();

        // Save to DDS in managed memory
        using var memoryStream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.FORCE_DX10_EXT | DDS_FLAGS.FORCE_DX10_EXT_MISC2);
        image.Dispose();
        ddsStream.CopyTo(memoryStream);

        return ConvertImageToNewBtex(name, "dds", TextureType.Preview, memoryStream.ToArray(), LuminousGame.FFXV);
    }

    public byte[] WicToBinmodThumbnailBlackTexture(string name, byte[] data)
    {
        // Load the image data
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();
        var image = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);
        pinnedData.Free();

        // Resize the image if the dimensions are incorrect
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

        // Save to DDS in managed memory
        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.FORCE_DX10_EXT | DDS_FLAGS.FORCE_DX10_EXT_MISC2);
        image.Dispose();
        ddsStream.CopyTo(stream);

        return ConvertImageToNewBtex(name, "dds", TextureType.Thumbnail, stream.ToArray(), LuminousGame.FFXV);
    }

    public byte[] ConvertImageToNewBtex(string name, string extension, TextureType type, byte[] data, LuminousGame game)
    {
        return ToBtex(new TextureConversionRequest
        {
            Name = name,
            PixelFormat = type switch
            {
                TextureType.Normal => BlackTexturePixelFormat.BC5_UNORM,
                TextureType.AmbientOcclusion or TextureType.Opacity => BlackTexturePixelFormat.BC4_UNORM,
                TextureType.MenuSprites => BlackTexturePixelFormat.B8G8R8A8_UNORM,
                _ => BlackTexturePixelFormat.BC1_UNORM
            },
            // TODO: Now that these flags are known, they should be calculated based on the properties of the image
            ImageFlags = type switch
            {
                TextureType.BaseColor => BlackTextureImageFlags.FLAG_SRGB | BlackTextureImageFlags.FLAG_COMPRESS |
                                         BlackTextureImageFlags.FLAG_MIPMAP,
                TextureType.Preview or TextureType.Thumbnail => BlackTextureImageFlags.FLAG_SRGB |
                                                                BlackTextureImageFlags.FLAG_MIPMAP,
                TextureType.MenuItem => BlackTextureImageFlags.FLAG_COMPRESS,
                _ => BlackTextureImageFlags.FLAG_COMPRESS | BlackTextureImageFlags.FLAG_MIPMAP
            },
            MipLevels = type switch
            {
                TextureType.Preview or TextureType.Thumbnail => 1,
                _ => 0 // Generate all mip levels
            },
            SourceFormat = extension switch
            {
                "tga" => TextureSourceFormat.Targa,
                "dds" => TextureSourceFormat.Dds,
                _ => TextureSourceFormat.Wic
            },
            SourceData = data,
            Game = game
        }).Write();
    }

    public bool ExportTerrainTexture(string absoluteFilePath, ImageFormat targetFormat, byte[] hebData)
    {
        var outputPathNoExtension = absoluteFilePath[..absoluteFilePath.LastIndexOf('.')];

        // ReSharper disable twice PossibleUnintendedReferenceComparison
        if (targetFormat == ImageFormat.Heb)
        {
            var finalPath = $"{outputPathNoExtension}.{targetFormat}";
            IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
            File.WriteAllBytes(finalPath, hebData);
        }
        else
        {
            var heightEntity = HeightEntityBinary.FromData(hebData);

            List<byte[]> images;
            if (targetFormat == ImageFormat.Png)
            {
                images = heightEntity.ImageHeaders.Select(h => DdsToScratchImage(h.ToDds()).ToPng()).ToList();
            }
            else if (targetFormat == ImageFormat.Targa)
            {
                images = heightEntity.ImageHeaders.Select(h => DdsToScratchImage(h.ToDds()).ToTarga()).ToList();
            }
            else if (targetFormat == ImageFormat.Dds)
            {
                images = heightEntity.ImageHeaders.Select(h => h.ToDds().Write()).ToList();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(targetFormat), targetFormat,
                    @"Unsupported texture format");
            }

            if (images.Count > 1)
            {
                for (var i = 0; i < images.Count; i++)
                {
                    var type = heightEntity.ImageHeaders[i].Type;
                    var finalPath = Path.Combine(outputPathNoExtension, $"{i}_{type}.{targetFormat}");
                    IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
                    File.WriteAllBytes(finalPath, images[i]);
                }
            }
            else if (images.Count > 0)
            {
                var finalPath = $"{outputPathNoExtension}.{targetFormat}";
                IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
                File.WriteAllBytes(finalPath, images[0]);
            }
            else
            {
                return false;
            }
        }

        return true;
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
            File.WriteAllBytes(finalPath, ToDds(btexData));
        }
        else
        {
            List<byte[]> images;
            if (targetFormat == ImageFormat.Png)
            {
                images = ToPngs(btexData).ToList();
            }
            else if (targetFormat == ImageFormat.Targa)
            {
                images = ToTargas(btexData).ToList();
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
                    var finalPath = $@"{outputPathNoExtension}\{name}.1{(i + 1).WithLeadingZeroes(3)}.{targetFormat}";
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

    private ScratchImage BtexToScratchImage(byte[] btex)
    {
        var texture = new BlackTexture(profile.Current.Type);
        texture.Read(btex);
        var dds = texture.ToDds();
        return DdsToScratchImage(dds);
    }

    public ScratchImage DdsToScratchImage(DirectDrawSurface dds)
    {
        var buffer = dds.Write();
        var pinnedData = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromDDSMemory(pointer, buffer.Length, DDS_FLAGS.NONE);

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
}