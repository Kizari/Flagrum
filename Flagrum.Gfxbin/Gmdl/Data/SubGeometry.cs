namespace Flagrum.Gfxbin.Gmdl.Data
{
    public class SubGeometry
    {
        public Aabb Aabb { get; set; }
        public uint StartIndex { get; set; }
        public uint PrimitiveCount { get; set; }
        public uint ClusterIndexBitFlag { get; set; }
        public uint DrawOrder { get; set; }
    }
}