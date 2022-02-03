using System.IO;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Core.Gfxbin.Btex;

namespace Flagrum.Web.Services;

public class TextureConverter
{
    public byte[] ConvertPreview(string name, byte[] data, out byte[] jpeg)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);

        var metadata = image.GetMetadata();
        if (!(metadata.Width == 600 && metadata.Height == 600))
        {
            // Resize to the mandatory 600x600 without stretching
            if (metadata.Width > metadata.Height)
            {
                var aspectRatio = metadata.Height / (double)metadata.Width;
                var height = (int)(600 * aspectRatio);
                image = image.Resize(600, height, TEX_FILTER_FLAGS.CUBIC);
            }
            else if (metadata.Height > metadata.Width)
            {
                var aspectRatio = metadata.Width / (double)metadata.Height;
                var width = (int)(600 * aspectRatio);
                image = image.Resize(width, 600, TEX_FILTER_FLAGS.CUBIC);
            }
            else
            {
                image = image.Resize(600, 600, TEX_FILTER_FLAGS.CUBIC);
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
                image = destinationImage;
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

        image = image.Compress(DXGI_FORMAT.BC1_UNORM, TEX_COMPRESS_FLAGS.SRGB_OUT | TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);

        pinnedData.Free();

        return BuildBtex(TextureType.Preview, name, image);
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

    public byte[] BtexToDds(byte[] btex)
    {
        var dds = BtexConverter.BtexToDds(btex);

        var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);

        pinnedData.Free();

        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
        ddsStream.CopyTo(stream);
        return stream.ToArray();
    }

    public byte[] BtexToJpg(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        using var stream = new MemoryStream();
        using var jpgStream = image.SaveToJPGMemory(0, 1.0f);
        jpgStream.CopyTo(stream);
        return stream.ToArray();
    }

    public byte[] BtexToPng(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        using var stream = new MemoryStream();
        using var pngStream =
            image.SaveToWICMemory(0, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
        pngStream.CopyTo(stream);
        return stream.ToArray();
    }

    public byte[] BtexToTga(byte[] btex)
    {
        var image = BtexToScratchImage(btex);
        using var stream = new MemoryStream();
        using var tgaStream = image.SaveToTGAMemory(0);
        tgaStream.CopyTo(stream);
        return stream.ToArray();
    }

    private ScratchImage BtexToScratchImage(byte[] btex)
    {
        var dds = BtexConverter.BtexToDds(btex);

        var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);

        pinnedData.Free();

        var metadata = image.GetMetadata();
        if (metadata.Format != DXGI_FORMAT.R8G8B8A8_UNORM)
        {
            image = image.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
        }

        return image;
    }

    private byte[] ConvertWic(TextureType type, string name, byte[] data)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromWICMemory(pointer, data.Length, WIC_FLAGS.NONE);
        image = BuildDds(type, image);

        pinnedData.Free();
        return BuildBtex(type, name, image);
    }

    private byte[] ConvertTarga(TextureType type, string name, byte[] data)
    {
        var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromTGAMemory(pointer, data.Length);
        image = BuildDds(type, image);

        pinnedData.Free();
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
        // var filterFlags = TEX_FILTER_FLAGS.CUBIC;
        // var compressFlags = TEX_COMPRESS_FLAGS.DEFAULT;
        // var metadata = image.GetMetadata();
        // if (metadata.Format is DXGI_FORMAT.B8G8R8A8_UNORM_SRGB
        //     or DXGI_FORMAT.B8G8R8X8_UNORM_SRGB
        //     or DXGI_FORMAT.R8G8B8A8_UNORM_SRGB)
        // {
        //     compressFlags = TEX_COMPRESS_FLAGS.SRGB;
        //     filterFlags |= TEX_FILTER_FLAGS.SRGB;
        // }

        switch (type)
        {
            case TextureType.AmbientOcclusion:
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, 0);
                image = image.Compress(DXGI_FORMAT.BC4_UNORM, TEX_COMPRESS_FLAGS.DEFAULT | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f);
                break;
            default:
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.CUBIC, 0);
                image = image.Compress(DXGI_FORMAT.BC1_UNORM, TEX_COMPRESS_FLAGS.SRGB | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f);
                break;
        }

        return image;
    }

    private byte[] BuildBtex(TextureType type, string name, ScratchImage image)
    {
        using var stream = new MemoryStream();
        using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.FORCE_DX10_EXT | DDS_FLAGS.FORCE_DX10_EXT_MISC2);
        ddsStream.CopyTo(stream);

        return BtexConverter.DdsToBtex(type, name, stream.ToArray());
    }
}