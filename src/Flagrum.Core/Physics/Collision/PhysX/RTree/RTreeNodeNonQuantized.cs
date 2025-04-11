using Flagrum.Core.Mathematics;

namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTreeNodeNonQuantized
{
    public RTreeBoundingBox Bounds { get; set; } = new();
    public int ChildPageFirstNodeIndex { get; set; }
    public int LeafCount { get; set; }
}