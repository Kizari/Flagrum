using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision;

public class PhysicsStaticBody
{
    public int ShapeLinkOffset { get; set; } = 16;
    public int NameOffset { get; set; }
    public int UserStringOffset { get; set; }
    public PhysicsShape Shape { get; set; }

    public void Read(BinaryReader reader)
    {
        ShapeLinkOffset = reader.ReadInt32();
        NameOffset = reader.ReadInt32();
        UserStringOffset = reader.ReadInt32();
        reader.Align(16);
        Shape = new PhysicsShape();
        Shape.Read(reader);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ShapeLinkOffset);
        writer.Write(NameOffset);
        writer.Write(UserStringOffset);
        writer.Align(16, 0x00);
        Shape.Write(writer);
    }
}