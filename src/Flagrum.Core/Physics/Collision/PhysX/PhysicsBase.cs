using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsBase
{
    public PhysicsConcreteType Type { get; set; }
    public ushort BaseFlags { get; set; }

    public virtual void Read(BinaryReader reader)
    {
        Type = (PhysicsConcreteType)reader.ReadUInt16();
        BaseFlags = reader.ReadUInt16();
        reader.Align(16);
    }

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write((ushort)Type);
        writer.Write(BaseFlags);
        writer.Align(16, 0xCD);
    }
}