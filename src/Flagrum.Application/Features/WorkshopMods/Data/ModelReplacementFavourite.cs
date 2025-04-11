using MemoryPack;

namespace Flagrum.Application.Persistence.Entities;

[MemoryPackable]
public partial class ModelReplacementFavourite
{
    public int Id { get; set; }
    public bool IsDefault { get; set; }
}