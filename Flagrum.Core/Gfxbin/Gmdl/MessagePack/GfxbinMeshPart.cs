namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinMeshPart : IMessagePackItem
{
    public uint PartsId { get; set; }
    public uint StartIndex { get; set; }
    public uint IndexCount { get; set; }

    public void Read(MessagePackReader reader)
    {
        PartsId = reader.Read<uint>();
        StartIndex = reader.Read<uint>();
        IndexCount = reader.Read<uint>();
    }
}