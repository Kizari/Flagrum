using System.IO;
using Flagrum.Core.Archive;
using Flagrum.Web.Persistence.Entities.ModManager;

namespace Flagrum.Web.Features.ModManager.Data;

public class FmodEarc
{
    public EarcChangeType Type { get; set; }
    public EbonyArchiveFlags Flags { get; set; }
    public uint RelativePathOffset { get; set; }
    public uint FileCount { get; set; }
    public FmodEarcFile[] Files { get; set; }

    public string RelativePath { get; set; }

    public void Read(BinaryReader reader)
    {
        Type = (EarcChangeType)reader.ReadInt32();
        Flags = (EbonyArchiveFlags)reader.ReadUInt32();
        RelativePathOffset = reader.ReadUInt32();
        FileCount = reader.ReadUInt32();

        Files = new FmodEarcFile[FileCount];
        for (var i = 0; i < FileCount; i++)
        {
            Files[i] = new FmodEarcFile();
            Files[i].Read(reader);
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)Type);
        writer.Write((uint)Flags);
        writer.Write(RelativePathOffset);
        writer.Write(FileCount);

        foreach (var file in Files)
        {
            file.Write(writer);
        }
    }
}