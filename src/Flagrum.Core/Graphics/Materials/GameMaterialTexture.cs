using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterialTexture
{
    public ulong UriHash { get; set; }
    public ulong NameOffset { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public ulong Unknown { get; set; }
    public ulong UriOffset { get; set; }
    public uint NameHash { get; set; }
    public uint ShaderGenNameHash { get; set; }
    public uint Unknown2 { get; set; }
    public uint UriHash32 { get; set; }
    public uint Flags { get; set; }
    public int HighTextureStreamingLevels { get; set; }

    public string Name { get; set; }
    public string ShaderGenName { get; set; }
    public string Uri { get; set; }

    public void Read(MessagePackReader reader)
    {
        UriHash = reader.Read<ulong>();
        NameOffset = reader.Read<ulong>();
        ShaderGenNameOffset = reader.Read<ulong>();
        Unknown = reader.Read<ulong>();
        UriOffset = reader.Read<ulong>();
        NameHash = reader.Read<uint>();
        ShaderGenNameHash = reader.Read<uint>();
        Unknown2 = reader.Read<uint>();
        UriHash32 = reader.Read<uint>();
        Flags = reader.Read<uint>();

        if (reader.DataVersion > 20150508)
        {
            HighTextureStreamingLevels = reader.Read<int>();
        }
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(UriHash);
        writer.Write(NameOffset);
        writer.Write(ShaderGenNameOffset);
        writer.Write(Unknown);
        writer.Write(UriOffset);
        writer.Write(NameHash);
        writer.Write(ShaderGenNameHash);
        writer.Write(Unknown2);
        writer.Write(UriHash32);
        writer.Write(Flags);

        if (writer.DataVersion > 20150508)
        {
            writer.Write(HighTextureStreamingLevels);
        }
    }
}