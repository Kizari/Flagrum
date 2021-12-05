using Flagrum.Core.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Core.Gfxbin.Gmdl.Components;

public class SubGeometry
{
    public Aabb Aabb { get; set; }
    public uint StartIndex { get; set; }
    public uint PrimitiveCount { get; set; }
    public uint ClusterIndexBitFlag { get; set; }
    public uint DrawOrder { get; set; }
}