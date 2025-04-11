using Flagrum.Abstractions;
using Flagrum.Core.Graphics.Textures.Luminous;

namespace Flagrum.Application.Features.Shared;

public class TextureConversionRequest
{
    public LuminousGame Game { get; set; }
    public string Name { get; set; }
    public BlackTexturePixelFormat PixelFormat { get; set; }
    public BlackTextureImageFlags ImageFlags { get; set; }
    public uint MipLevels { get; set; }
    public TextureSourceFormat SourceFormat { get; set; }
    public byte[] SourceData { get; set; }
}