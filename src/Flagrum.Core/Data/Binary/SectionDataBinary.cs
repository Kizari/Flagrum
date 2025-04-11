using System;
using System.IO;
using System.Text;
using Flagrum.Core.Serialization;

namespace Flagrum.Core.Data.Binary;

public class SectionDataBinary : BinaryReaderWriterBase, IResourceBinaryItem, ISubresource
{
    public DataIndex Index { get; set; }
    
    public char[] Type { get; set; } = "SEDB".ToCharArray();
    public char[] Subtype { get; set; }
    public uint BinaryVersion { get; set; }
    public BinaryEndianType EndianType { get; set; }
    public byte AlignmentBits { get; set; }
    public ushort Offset { get; set; } = 128;
    public ulong Size { get; set; }
    public ulong DateTime { get; set; }
    public ResourceId ResourceId { get; set; } = new();

    public override void Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        
        Type = reader.ReadChars(4);
        Subtype = reader.ReadChars(4);
        BinaryVersion = reader.ReadUInt32();
        EndianType = (BinaryEndianType)reader.ReadByte();
        AlignmentBits = reader.ReadByte();
        Offset = reader.ReadUInt16();
        Size = reader.ReadUInt64();
        DateTime = reader.ReadUInt64();
        ResourceId = new ResourceId();
        ResourceId.Read(reader);
    }

    public override void Write(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        // Write the SEDB header
        writer.Write(Type);
        writer.Write(Subtype);
        writer.Write(BinaryVersion);
        writer.Write((byte)EndianType);
        writer.Write(AlignmentBits);
        writer.Write(Offset);
        writer.Write(Size);
        writer.Write(DateTime);
        ResourceId.Write(writer);
    }

    public void Read(BinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }
}