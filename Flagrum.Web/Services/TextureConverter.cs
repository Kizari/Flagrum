using System.IO;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Core.Gfxbin.Btex;

namespace Flagrum.Web.Services;

public class TextureConverter
{
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

    public byte[] BtexToJpg(byte[] btex)
    {
        var dds = BtexConverter.BtexToDds(btex);

        var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
        var pointer = pinnedData.AddrOfPinnedObject();

        var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);

        pinnedData.Free();

        using var stream = new MemoryStream();
        using var jpgStream = image.SaveToJPGMemory(0, 1.0f);
        jpgStream.CopyTo(stream);
        return stream.ToArray();
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
        // Run necessary conversion methods on image
        switch (type)
        {
            case TextureType.Normal:
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.LINEAR, 0);
                image = image.Compress(DXGI_FORMAT.BC5_UNORM, TEX_COMPRESS_FLAGS.DEFAULT | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f);
                break;
            case TextureType.Greyscale:
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.LINEAR, 0);
                image = image.Compress(DXGI_FORMAT.BC4_UNORM, TEX_COMPRESS_FLAGS.DEFAULT | TEX_COMPRESS_FLAGS.PARALLEL,
                    0.5f);
                break;
            case TextureType.Preview or TextureType.Thumbnail:
                var metadata = image.GetMetadata();
                if (type == TextureType.Preview && !(metadata.Width == 600 && metadata.Height == 600))
                {
                    image = image.Resize(600, 600, TEX_FILTER_FLAGS.DEFAULT);
                }
                else if (type == TextureType.Thumbnail && !(metadata.Width == 168 && metadata.Height == 242))
                {
                    image = image.Resize(168, 242, TEX_FILTER_FLAGS.DEFAULT);
                }

                image = image.Compress(DXGI_FORMAT.BC1_UNORM,
                    TEX_COMPRESS_FLAGS.SRGB_OUT | TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
                break;
            default:
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.SRGB, 0);
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