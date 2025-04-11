using Flagrum.Core.Mathematics;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class GameModelSubgeometry : IMessagePackItem
{
    public AxisAlignedBoundingBox AABB { get; set; }
    public uint StartIndex { get; set; }
    public uint PrimitiveCount { get; set; }
    public uint ClusterIndexBitFlag { get; set; }
    public uint DrawOrder { get; set; }

    public void Read(MessagePackReader reader)
    {
        AABB = new AxisAlignedBoundingBox();
        AABB.Read(reader);
        StartIndex = reader.Read<uint>();
        PrimitiveCount = reader.Read<uint>();
        ClusterIndexBitFlag = reader.Read<uint>();
        DrawOrder = reader.Read<uint>();
    }

    public void Write(MessagePackWriter writer)
    {
        AABB.Write(writer);
        writer.Write(StartIndex);
        writer.Write(PrimitiveCount);
        writer.Write(ClusterIndexBitFlag);
        writer.Write(DrawOrder);
    }
}