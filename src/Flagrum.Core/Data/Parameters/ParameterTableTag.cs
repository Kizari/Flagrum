using System;
using System.IO;

namespace Flagrum.Core.Data.Bins;

[Flags]
public enum ParameterTableTagFlag : ushort
{
    None = 0,
    Boolean = 1,
    Array = 2
}

public class ParameterTableTag
{
    public uint Id { get; set; }
    public ParameterTableTagFlag Flag { get; set; }
    public ushort Offset { get; set; }

    public void Read(BinaryReader reader)
    {
        Id = reader.ReadUInt32();
        Flag = (ParameterTableTagFlag)reader.ReadUInt16();
        Offset = reader.ReadUInt16();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write((ushort)Flag);
        writer.Write(Offset);
    }
}