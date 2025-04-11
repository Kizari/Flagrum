using System.Collections.Generic;

namespace Flagrum.Application.Persistence.Entities;

public class ModelReplacementPresetEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<ModelReplacementPathEntity> ReplacementPaths { get; set; }
}