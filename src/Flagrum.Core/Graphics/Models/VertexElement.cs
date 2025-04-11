using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class VertexElement : IMessagePackItem
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

    public void Write(MessagePackWriter writer)
    {
        writer.Write(Offset);
        writer.Write(Semantic.Value);
        writer.Write((int)Format);
    }
}