using System.Numerics;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Mathematics;

public class Matrix3x4
{
    public Matrix3x4() { }

    public Matrix3x4(Vector3 row1, Vector3 row2, Vector3 row3, Vector3 row4)
    {
        Rows = new[] {row1, row2, row3, row4};
    }

    public Vector3[] Rows { get; set; }

    public void Read(MessagePackReader reader)
    {
        Rows = new[]
        {
            reader.ReadVector3(),
            reader.ReadVector3(),
            reader.ReadVector3(),
            reader.ReadVector3()
        };
    }

    public void Write(MessagePackWriter writer)
    {
        foreach (var row in Rows)
        {
            row.Write(writer);
        }
    }
}