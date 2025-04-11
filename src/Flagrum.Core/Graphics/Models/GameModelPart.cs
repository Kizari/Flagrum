using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class GameModelPart : IMessagePackItem
{
    public string Name { get; set; }
    public uint Id { get; set; }
    public string Unknown { get; set; }
    public bool Flags { get; set; }

    public void Read(MessagePackReader reader)
    {
        Name = reader.Read<string>();
        Id = reader.Read<uint>();
        Unknown = reader.Read<string>();
        Flags = reader.Read<bool>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(Name);
        writer.Write(Id);
        writer.Write(Unknown);
        writer.Write(Flags);
    }
}