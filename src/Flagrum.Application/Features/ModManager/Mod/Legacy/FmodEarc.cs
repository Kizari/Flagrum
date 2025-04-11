using System.Collections.Generic;
using System.IO;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Application.Features.ModManager.Mod.Legacy;

public class FmodEarc
{
    public ModChangeType Type { get; set; }
    public EbonyArchiveFlags Flags { get; set; }
    public uint RelativePathOffset { get; set; }
    public uint FileCount { get; set; }
    public List<FmodEarcFile> Files { get; set; } = new();

    public string RelativePath { get; set; }

    public void Read(BinaryReader reader)
    {
        Type = (ModChangeType)reader.ReadInt32();
        Flags = (EbonyArchiveFlags)reader.ReadUInt32();
        RelativePathOffset = reader.ReadUInt32();
        FileCount = reader.ReadUInt32();

        for (var i = 0; i < FileCount; i++)
        {
            var file = new FmodEarcFile();
            file.Read(reader);
            Files.Add(file);
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)Type);
        writer.Write((uint)Flags);
        writer.Write(RelativePathOffset);
        FileCount = (uint)Files.Count;
        writer.Write(FileCount);

        foreach (var file in Files)
        {
            file.Write(writer);
        }
    }
}