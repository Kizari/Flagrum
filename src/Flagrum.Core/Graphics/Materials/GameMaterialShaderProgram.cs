using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterialShaderProgram
{
    public ushort LowKey { get; set; }
    public ushort HighKey { get; set; }
    public ushort CsBinaryIndex { get; set; }
    public ushort VsBinaryIndex { get; set; }
    public ushort HsBinaryIndex { get; set; }
    public ushort DsBinaryIndex { get; set; }
    public ushort GsBinaryIndex { get; set; }
    public ushort PsBinaryIndex { get; set; }

    public void Read(MessagePackReader reader)
    {
        LowKey = reader.Read<ushort>();
        HighKey = reader.Read<ushort>();
        CsBinaryIndex = reader.Read<ushort>();
        VsBinaryIndex = reader.Read<ushort>();
        HsBinaryIndex = reader.Read<ushort>();
        DsBinaryIndex = reader.Read<ushort>();
        GsBinaryIndex = reader.Read<ushort>();
        PsBinaryIndex = reader.Read<ushort>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(LowKey);
        writer.Write(HighKey);
        writer.Write(CsBinaryIndex);
        writer.Write(VsBinaryIndex);
        writer.Write(HsBinaryIndex);
        writer.Write(DsBinaryIndex);
        writer.Write(GsBinaryIndex);
        writer.Write(PsBinaryIndex);
    }
}