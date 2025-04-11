using System.Collections.Generic;

namespace Flagrum.Core.Animation.Model;

public class PartsData
{
    public uint[] PartsLayerIds { get; set; }
    public List<ulong[]> PartsIds { get; set; } = new();
}