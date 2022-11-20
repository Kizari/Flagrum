using System.IO;
using Flagrum.Web.Persistence.Entities.ModManager;

namespace Flagrum.Web.Features.EarcMods.Data;

public interface IFmodFile
{
    public ulong DataOffset { get; set; }
    public uint Size { get; set; }
}

public class FmodFile : IFmodFile
{
    public EarcChangeType Type { get; set; }
    public uint FileNameOffset { get; set; }
    public uint RelativePathOffset { get; set; }

    public string RelativePath { get; set; }
    public string DataSource { get; set; }
    public ulong DataOffset { get; set; }
    public uint Size { get; set; }
    public string FileName { get; set; }

    public void Read(BinaryReader reader)
    {
        Type = (EarcChangeType)reader.ReadInt32();
        FileNameOffset = reader.ReadUInt32();
        RelativePathOffset = reader.ReadUInt32();
        DataOffset = reader.ReadUInt64();
        Size = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)Type);
        writer.Write(FileNameOffset);
        writer.Write(RelativePathOffset);
        writer.Write(DataOffset);
        writer.Write(Size);
    }
}