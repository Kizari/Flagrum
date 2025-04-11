using System.IO;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsCollectionReference<TReference>
{
    public TReference Reference { get; set; }
    public uint Kind { get; set; }
    public uint ObjectIndex { get; set; }

    public void Read(BinaryReader reader)
    {
        Reference = typeof(TReference) == typeof(ulong)
            ? (TReference)(object)reader.ReadUInt64()
            : (TReference)(object)reader.ReadUInt32();
        
        Kind = reader.ReadUInt32();
        ObjectIndex = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        if (typeof(TReference) == typeof(ulong))
        {
            writer.Write((ulong)(object)Reference);
        }
        else
        {
            writer.Write((uint)(object)Reference);
        }
        
        writer.Write(Kind);
        writer.Write(ObjectIndex);
    }
}