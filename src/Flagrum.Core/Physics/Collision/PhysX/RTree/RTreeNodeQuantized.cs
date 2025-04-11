namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTreeNodeQuantized
{
    public float MinX { get; set; }
    public float MinY { get; set; }
    public float MinZ { get; set; }
    public float MaxX { get; set; }
    public float MaxY { get; set; }
    public float MaxZ { get; set; }
    public uint Pointer { get; set; }

    public void SetLeaf(bool isLeaf)
    {
        if (isLeaf)
        {
            Pointer |= 1u;
        }
        else
        {
            Pointer &= ~1u;
        }
    }
}