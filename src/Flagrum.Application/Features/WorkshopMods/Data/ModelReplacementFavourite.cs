using MemoryPack;

namespace Flagrum.Application.Features.WorkshopMods.Data;

[MemoryPackable]
public partial class ModelReplacementFavourite
{
    public int Id { get; set; }
    public bool IsDefault { get; set; }
}