using System.IO;
using Flagrum.Abstractions.Archive;

namespace Flagrum.Application.Features.ModManager.Mod.Legacy;

public class FmodEarcFile : IFmodFile
{
    public LegacyModBuildInstruction Type { get; set; }
    public EbonyArchiveFileFlags Flags { get; set; }
    public uint UriOffset { get; set; }
    public string Uri { get; set; }
    public string DataSource { get; set; }
    public ulong DataOffset { get; set; }
    public uint Size { get; set; }

    public void Read(BinaryReader reader)
    {
        Type = (LegacyModBuildInstruction)reader.ReadInt32();
        Flags = (EbonyArchiveFileFlags)reader.ReadUInt32();
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