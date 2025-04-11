using System.Collections.Generic;
using MemoryPack;

namespace Flagrum.Application.Persistence.Entities;

[MemoryPackable]
public partial class ModelReplacementPreset
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> ReplacementPaths { get; set; }
}