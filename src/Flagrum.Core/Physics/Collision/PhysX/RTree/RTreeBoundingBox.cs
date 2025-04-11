using System.Numerics;

namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTreeBoundingBox
{
    public RTreeBoundingBox()
    {
        Min = Vector3.Zero;
        Max = Vector3.Zero;
    }
    
    public RTreeBoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }
    
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    public Vector3 Extents => Max - Min;

    public void Include(RTreeBoundingBox other)
    {
        Min = Vector3.Min(Min, other.Min);
        Max = Vector3.Max(Max, other.Max);
    }

    public void SetEmpty()
    {
        Min = new Vector3(float.MaxValue * 0.25f, float.MaxValue * 0.25f, float.MaxValue * 0.25f);
        Max = new Vector3(float.MinValue * 0.25f, float.MinValue * 0.25f, float.MinValue * 0.25f);
    }
}