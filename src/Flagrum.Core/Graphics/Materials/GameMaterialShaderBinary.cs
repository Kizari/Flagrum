using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterialShaderBinary
{
    public ulong UriHash { get; set; }
    public ulong UriOffset { get; set; }

    public string Uri { get; set; }

    public void Read(MessagePackReader reader)
    {
        UriHash = reader.Read<ulong>();
        UriOffset = reader.Read<ulong>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(UriHash);
        writer.Write(UriOffset);
    }
}