using System.IO;
using Flagrum.Core.Archive;
using Flagrum.Web.Persistence.Entities.ModManager;

namespace Flagrum.Web.Features.EarcMods.Data;

public class FmodEarcFile : IFmodFile
{
    public EarcFileChangeType Type { get; set; }
    public ArchiveFileFlag Flags { get; set; }
    public uint UriOffset { get; set; }
    public string Uri { get; set; }
    public string DataSource { get; set; }
    public ulong DataOffset { get; set; }
    public uint Size { get; set; }

    public void Read(BinaryReader reader)
    {
        Type = (EarcFileChangeType)reader.ReadInt32();
        Flags = (ArchiveFileFlag)reader.ReadUInt32();
        UriOffset = reader.ReadUInt32();
        DataOffset = reader.ReadUInt64();
        Size = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)Type);
        writer.Write((uint)Flags);
        writer.Write(UriOffset);
        writer.Write(DataOffset);
        writer.Write(Size);
    }
}