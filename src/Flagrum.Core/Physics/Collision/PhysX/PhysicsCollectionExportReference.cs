using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsCollectionExportReference
{
    public ulong Id { get; set; }
    public uint ObjectIndex { get; set; }

    public void Read(BinaryReader reader)
    {
        Id = reader.ReadUInt64();
        ObjectIndex = reader.ReadUInt32();
        reader.ReadBytes(4); // Weird alignment or padding
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(ObjectIndex);
        writer.Write(new byte[] {0xCD, 0xCD, 0xCD, 0xCD});
    }
}