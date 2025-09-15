using System.IO;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Data;

[StructLayout(LayoutKind.Sequential)]
public struct ResourceId
{
    public uint Type { get; set; }
    public uint Primary { get; set; }
    public uint Secondary { get; set; }
    public uint Flags { get; set; }

    public void Read(BinaryReader reader)
    {
        Type = reader.ReadUInt32();
        Primary = reader.ReadUInt32();
        Secondary = reader.ReadUInt32();
        Flags = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Type);
        writer.Write(Primary);
        writer.Write(Secondary);
        writer.Write(Flags);
    }
}