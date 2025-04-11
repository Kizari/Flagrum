using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterialUniform
{
    public ulong NameOffset { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public ulong Unknown { get; set; }
    public uint NameHash { get; set; }
    public uint ShaderGenNameHash { get; set; }
    public uint Unknown2 { get; set; }
    public uint Offset { get; set; }
    public ushort Size { get; set; }
    public ushort BufferCount { get; set; }
    public uint Flags { get; set; }

    public string Name { get; set; }
    public string ShaderGenName { get; set; }

    public void Read(MessagePackReader reader)
    {
        NameOffset = reader.Read<ulong>();
        ShaderGenNameOffset = reader.Read<ulong>();
        Unknown = reader.Read<ulong>();
        NameHash = reader.Read<uint>();
        ShaderGenNameHash = reader.Read<uint>();
        Unknown2 = reader.Read<uint>();
        Offset = reader.Read<uint>();
        Size = reader.Read<ushort>();
        BufferCount = reader.Read<ushort>();
        Flags = reader.Read<uint>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(NameOffset);
        writer.Write(ShaderGenNameOffset);
        writer.Write(Unknown);
        writer.Write(NameHash);
        writer.Write(ShaderGenNameHash);
        writer.Write(Unknown2);
        writer.Write(Offset);
        writer.Write(Size);
        writer.Write(BufferCount);
        writer.Write(Flags);
    }
}