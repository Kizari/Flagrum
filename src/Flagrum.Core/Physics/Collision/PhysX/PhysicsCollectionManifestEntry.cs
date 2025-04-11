using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsCollectionManifestEntry
{
    public uint Offset { get; set; }
    public PhysicsConcreteType Type { get; set; }

    public void Read(BinaryReader reader)
    {
        Offset = reader.ReadUInt32();
        Type = (PhysicsConcreteType)reader.ReadUInt16();
        reader.Align(4);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Offset);
        writer.Write((ushort)Type);
        writer.Align(4, 0xCD);
    }
}