using System.IO;
using System.Numerics;
using Flagrum.Core.Serialization.MessagePack;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Mathematics;

public class AxisAlignedBoundingBox
{
    public AxisAlignedBoundingBox() { }

    public AxisAlignedBoundingBox(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public Vector3 Start { get; set; }
    public Vector3 End { get; set; }

    public void Read(MessagePackReader reader)
    {
        Start = reader.ReadVector3();
        End = reader.ReadVector3();
    }

    public void Write(MessagePackWriter writer)
    {
        Start.Write(writer);
        End.Write(writer);
    }

    public void Read(BinaryReader reader)
    {
        Start = reader.ReadVector3();
        End = reader.ReadVector3();
    }

    public void Write(BinaryWriter writer)
    {
        writer.WriteVector3(Start);
        writer.WriteVector3(End);
    }
}