namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTreeLeafTriangles
{
    public uint Data { get; private set; }

    public void SetData(uint count, uint index)
    {
        Data = (index << 5) | (((count - 1) & 15) << 1) | 1;
    }
}