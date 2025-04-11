using Flagrum.Application.Features.ModManager.Data;

namespace Flagrum.Application.Features.WorkshopMods.Data;

public class WorkshopModTexture
{
    public string Mesh { get; set; }
    public string TextureSlot { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public TextureType Type { get; set; }
    public byte[] Data { get; set; }
}