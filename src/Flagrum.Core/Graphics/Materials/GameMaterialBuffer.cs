using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterialBuffer
{
    public ulong NameOffset { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public uint NameHash { get; set; }
    public uint ShaderGenNameHash { get; set; }
    public ushort Offset { get; set; }
    public ushort Size { get; set; }
    public ushort UniformIndex { get; set; }
    public ushort Type { get; set; }
    public uint Flags { get; set; }

    public string Name { get; set; }
    public string ShaderGenName { get; set; }

    public float[] Values { get; set; }

    public void Read(MessagePackReader reader)
    {
        NameOffset = reader.Read<ulong>();
        ShaderGenNameOffset = reader.Read<ulong>();
        NameHash = reader.Read<uint>();
        ShaderGenNameHash = reader.Read<uint>();
        Offset = reader.Read<ushort>();
        Size = reader.Read<ushort>();
        UniformIndex = reader.Read<ushort>();
        Type = reader.Read<ushort>();
        Flags = reader.Read<uint>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(NameOffset);
        writer.Write(ShaderGenNameOffset);
        writer.Write(NameHash);
        writer.Write(ShaderGenNameHash);
        writer.Write(Offset);
        writer.Write(Size);
        writer.Write(UniformIndex);
        writer.Write(Type);
        writer.Write(Flags);
    }
}