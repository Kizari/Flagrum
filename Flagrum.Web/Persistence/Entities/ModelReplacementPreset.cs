using System.Collections.Generic;

namespace Flagrum.Web.Persistence.Entities;

public class ModelReplacementPreset
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<ModelReplacementPath> ReplacementPaths { get; set; }
}