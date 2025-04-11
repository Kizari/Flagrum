using System.IO;

namespace Flagrum.Core.Physics.Collision;

public class MeshCollisionLayer
{
    public uint Id { get; set; }
    public uint Count { get; set; }
    public uint[] Fixids { get; set; }
    public uint Reserved { get; set; }

    public void Read(BinaryReader reader)
    {
        Id = reader.ReadUInt32();
        Count = reader.ReadUInt32();

        Fixids = new uint[Count];
        for (var i = 0; i < Count; i++)
        {
            Fixids[i] = reader.ReadUInt32();
        }

        Reserved = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);
        Count = (uint)Fixids.Length;
        writer.Write(Count);

        foreach (var fixid in Fixids)
        {
            writer.Write(fixid);
        }
        
        writer.Write(Reserved);
    }
}