using System.IO;

namespace Flagrum.Core.Data;

public class DataIndex
{
    public ResourceId ResourceId { get; set; }
    public uint Offset { get; set; }
    public uint Size { get; set; }

    public void Read(BinaryReader reader)
    {
        ResourceId = new ResourceId();
        ResourceId.Read(reader);
        Offset = reader.ReadUInt32();
        Size = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        ResourceId.Write(writer);
        writer.Write(Offset);
        writer.Write(Size);
    }
}