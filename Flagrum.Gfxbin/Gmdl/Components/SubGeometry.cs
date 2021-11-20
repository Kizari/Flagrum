using Flagrum.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Gfxbin.Gmdl.Components;

public class SubGeometry
{
    public Aabb Aabb { get; set; }
    public uint StartIndex { get; set; }
    public uint PrimitiveCount { get; set; }
    public uint ClusterIndexBitFlag { get; set; }
    public uint DrawOrder { get; set; }
}