using System.IO;

namespace Flagrum.Core.Physics.Collision;

public class MeshCollisionAttributeSet
{
    public uint Count { get; set; }
    public uint[] Attributes { get; set; }

    public void Read(BinaryReader reader)
    {
        Count = reader.ReadUInt32();

        Attributes = new uint[Count];
        for (var i = 0; i < Count; i++)
        {
            Attributes[i] = reader.ReadUInt32();
        }
    }

    public void Write(BinaryWriter writer)
    {
        Count = (uint)Attributes.Length;
        writer.Write(Count);
        foreach (var attribute in Attributes)
        {
            writer.Write(attribute);
        }
    }
}