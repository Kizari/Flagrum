using System.Collections.Generic;
using Flagrum.Core.Gfxbin.Gmdl.Components;

namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinVertexStream : IMessagePackItem
{
    public VertexStreamSlot Slot { get; set; }
    public VertexStreamType Type { get; set; }
    public uint Stride { get; set; }
    public uint Offset { get; set; }
    public IList<GfxbinVertexElement> Elements { get; set; }

    public void Read(MessagePackReader reader)
    {
        Slot = (VertexStreamSlot)reader.Read<int>();
        Type = (VertexStreamType)reader.Read<int>();
        Stride = reader.Read<uint>();
        Offset = reader.Read<uint>();
        Elements = reader.Read<List<GfxbinVertexElement>>();
    }
}