using System.Collections.Generic;

namespace Flagrum.Application.Features.WorkshopMods.Data;

public class ModelReplacementPresetMetadata
{
    public int Id { get; set; }
    public bool IsDefault { get; set; }
    public bool IsFavourite { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> Paths { get; set; }
}