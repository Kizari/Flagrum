using System;
using System.IO;
using System.Text;
using Flagrum.Abstractions.Archive;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Application.Features.ModManager.Mod;

/// <inheritdoc />
public class FmodFragment : IFlagrumModFragment
{
    public char[] Magic { get; private set; } = "FFG ".ToCharArray();
    public uint Version { get; private set; } = 1;
    public uint OriginalSize { get; set; }
    public uint ProcessedSize { get; set; }
    public EbonyArchiveFileFlags Flags { get; set; }
    public ushort Key { get; set; }

    public string RelativePath { get; set; }

    public byte[] Data { get; set; }

    /// <inheritdoc />
    public byte[] GetReadableData() =>
        EbonyArchiveFile.GetReadableDataFromFragment("", RelativePath, OriginalSize, ProcessedSize, Flags, Key,
            Data);

    public void Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        Magic = reader.ReadChars(4);

        if (Magic[0] != 'F' || Magic[1] != 'F' || Magic[2] != 'G' || Magic[3] != ' ')
        {
            throw new Exception("File was not a valid Fmod Fragment file");
        }

        Version = reader.ReadUInt32();
        OriginalSize = reader.ReadUInt32();
        ProcessedSize = reader.ReadUInt32();
        Flags = (EbonyArchiveFileFlags)reader.ReadUInt32();
        Key = reader.ReadUInt16();

        reader.Align(16);

        RelativePath = reader.ReadNullTerminatedString();

        reader.Align(128);

        Data = new byte[ProcessedSize];
        _ = reader.Read(Data);
    }

    public void Write(string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(OriginalSize);
        writer.Write(ProcessedSize);
        writer.Write((uint)Flags);
        writer.Write(Key);

        writer.Align(16, 0x00);

        writer.WriteNullTerminatedString(RelativePath);

        writer.Align(128, 0x00);

        writer.Write(Data);
    }
}