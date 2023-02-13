namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinBone : IMessagePackItem
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
}