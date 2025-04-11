using System.Collections.Generic;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class VertexStream : IMessagePackItem
{
    public VertexStreamSlot Slot { get; set; }
    public VertexStreamType Type { get; set; }
    public uint Stride { get; set; }
    public uint Offset { get; set; }
    public IList<VertexElement> Elements { get; set; }

    public void Read(MessagePackReader reader)
    {
        Slot = (VertexStreamSlot)reader.Read<int>();
        Type = (VertexStreamType)reader.Read<int>();
        Stride = reader.Read<uint>();
        Offset = reader.Read<uint>();
        Elements = reader.Read<List<VertexElement>>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write((int)Slot);
        writer.Write((int)Type);
        writer.Write(Stride);
        writer.Write(Offset);
        writer.Write(Elements);
    }
}