using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class GameModelMeshPart : IMessagePackItem
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

    public void Write(MessagePackWriter writer)
    {
        writer.Write(PartsId);
        writer.Write(StartIndex);
        writer.Write(IndexCount);
    }
}