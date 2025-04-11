using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class GameModelBone : IMessagePackItem
{
    public string Name { get; set; }
    public uint LodIndex { get; set; }
    public uint UniqueIndex { get; set; }

    public void Read(MessagePackReader reader)
    {
        Name = reader.Read<string>();
        LodIndex = reader.Read<uint>();

        if (reader.DataVersion >= 20220707)
        {
            UniqueIndex = reader.Read<uint>();
        }
        else
        {
            UniqueIndex = LodIndex >> 16;
        }
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(Name);
        writer.Write(LodIndex);

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(UniqueIndex);
        }
    }
}