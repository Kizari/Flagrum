using System.IO;
using DirectXTexNet;

namespace Flagrum.Application.Utilities;

public static class DirectXTexExtensions
{
    public static byte[] ToPng(this ScratchImage image, int imageIndex = 0)
    {
        using var stream = new MemoryStream();
        using var pngStream = image.SaveToWICMemory(imageIndex, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
        pngStream.CopyTo(stream);
        return stream.ToArray();
    }

    public static byte[] ToTarga(this ScratchImage image, int imageIndex = 0)
    {
        using var stream = new MemoryStream();
        using var tgaStream = image.SaveToTGAMemory(imageIndex);
        tgaStream.CopyTo(stream);
        return stream.ToArray();
    }

    public static byte[] ToJpeg(this ScratchImage image, int imageIndex = 0)
    {
        using var stream = new MemoryStream();
        using var jpgStream = image.SaveToJPGMemory(imageIndex, 1.0f);
        jpgStream.CopyTo(stream);
        return stream.ToArray();
    }
}