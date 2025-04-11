namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTreeInterval
{
    public RTreeInterval(uint start, uint count)
    {
        Start = start;
        Count = count;
    }
    
    public uint Start { get; set; }
    public uint Count { get; set; }
}