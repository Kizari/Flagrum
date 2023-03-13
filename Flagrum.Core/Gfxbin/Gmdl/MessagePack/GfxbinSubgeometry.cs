namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinSubgeometry : IMessagePackItem
{
    public GfxbinAABB AABB { get; set; }
    public uint StartIndex { get; set; }
    public uint PrimitiveCount { get; set; }
    public uint ClusterIndexBitFlag { get; set; }
    public uint DrawOrder { get; set; }

    public void Read(MessagePackReader reader)
    {
        AABB = reader.ReadAABB();
        StartIndex = reader.Read<uint>();
        PrimitiveCount = reader.Read<uint>();
        ClusterIndexBitFlag = reader.Read<uint>();
        DrawOrder = reader.Read<uint>();
    }
}