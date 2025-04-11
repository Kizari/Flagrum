using System.IO;
using System.Text;

namespace Flagrum.Core.Data;

public class ResourceArchive : BlackResourceBinary, ISubresource
{
    public ResourceId ArchiveResourceId { get; set; } = new();
    public int Offset { get; set; }
    public int ArchiveSize { get; set; }
    public ulong DataOffset { get; set; }

    public override void Read(Stream stream)
    {
        base.Read(stream);

        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        ArchiveResourceId.Read(reader);
        Offset = reader.ReadInt32();
        ArchiveSize = reader.ReadInt32();
        DataOffset = reader.ReadUInt64();
    }

    public override void Write(Stream stream)
    {
        base.Write(stream);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        ArchiveResourceId.Write(writer);
        writer.Write(Offset);      // Always 0
        writer.Write(ArchiveSize); // Always 256
        writer.Write(DataOffset);  // End of file before padding
    }
}