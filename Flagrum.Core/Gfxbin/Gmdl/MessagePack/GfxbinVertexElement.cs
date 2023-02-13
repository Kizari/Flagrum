using Flagrum.Core.Gfxbin.Gmdl.Components;

namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinVertexElement : IMessagePackItem
{
    public uint Offset { get; set; }
    public VertexElementSemantic Semantic { get; set; }
    public VertexElementFormat Format { get; set; }

    public void Read(MessagePackReader reader)
    {
        Offset = reader.Read<uint>();
        Semantic = (VertexElementSemantic)reader.Read<string>();
        Format = (VertexElementFormat)reader.Read<int>();
    }
}