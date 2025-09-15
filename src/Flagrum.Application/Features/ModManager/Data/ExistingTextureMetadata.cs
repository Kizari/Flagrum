using Flagrum.Core.Graphics.Textures.Luminous;

namespace Flagrum.Application.Features.ModManager.Data;

public class ExistingTextureMetadata
{
    public required string Name { get; set; }
    public BlackTexturePixelFormat Format { get; set; }
    public BlackTextureImageFlags Flags { get; set; }
    public byte MipCount { get; set; }
}