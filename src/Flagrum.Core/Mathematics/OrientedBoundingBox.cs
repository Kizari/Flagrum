using System.Numerics;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Mathematics;

public class OrientedBoundingBox
{
    public OrientedBoundingBox() { }

    public OrientedBoundingBox(Vector3 center, Vector3 xHalfExtent, Vector3 yHalfExtent, Vector3 zHalfExtent)
    {
        Center = center;
        XHalfExtent = xHalfExtent;
        YHalfExtent = yHalfExtent;
        ZHalfExtent = zHalfExtent;
    }

    public Vector3 Center { get; set; }
    public Vector3 XHalfExtent { get; set; }
    public Vector3 YHalfExtent { get; set; }
    public Vector3 ZHalfExtent { get; set; }

    public void Read(MessagePackReader reader)
    {
        Center = reader.ReadVector3();
        XHalfExtent = reader.ReadVector3();
        YHalfExtent = reader.ReadVector3();
        ZHalfExtent = reader.ReadVector3();
    }

    public void Write(MessagePackWriter writer)
    {
        Center.Write(writer);
        XHalfExtent.Write(writer);
        YHalfExtent.Write(writer);
        ZHalfExtent.Write(writer);
    }
}