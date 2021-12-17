using System.IO;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Core.Gfxbin.Btex;

namespace Flagrum.Web.Services;

public class TextureConverter
{
    public byte[] Convert(string name, string extension, TextureType type, byte[] data)
    {
        return extension.ToLower() switch
        {
            "tga" => ConvertTarga(type, name, data),
            "dds" => ConvertDds(type, name, data),
            "btex" => data,
            _ => ConvertWic(type, name, data)
        };
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
                image = image.Compress(DXGI_FORMAT.BC5_UNORM, TEX_COMPRESS_FLAGS.DEFAULT | TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
                break;
            case TextureType.Greyscale:
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.LINEAR, 0);
                image = image.Compress(DXGI_FORMAT.BC4_UNORM, TEX_COMPRESS_FLAGS.DEFAULT | TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
                break;
            default:
                image = image.GenerateMipMaps(TEX_FILTER_FLAGS.SRGB, 0);
                image = image.Compress(DXGI_FORMAT.BC1_UNORM, TEX_COMPRESS_FLAGS.SRGB | TEX_COMPRESS_FLAGS.PARALLEL,0.5f);
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