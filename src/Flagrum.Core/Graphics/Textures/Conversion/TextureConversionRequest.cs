using Flagrum.Core.Graphics.Textures.Luminous;

namespace Flagrum.Core.Graphics.Textures.Conversion;

public class TextureConversionRequest
{
    public string Name { get; set; }
    public BlackTexturePixelFormat PixelFormat { get; set; }
    public byte ImageFlags { get; set; }
    public uint MipLevels { get; set; }
    public TextureSourceFormat SourceFormat { get; set; }
    public byte[] SourceData { get; set; }
}