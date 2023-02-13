namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinModelPart : IMessagePackItem
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
}