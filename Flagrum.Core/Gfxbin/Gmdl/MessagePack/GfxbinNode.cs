using Flagrum.Core.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinNode : IMessagePackItem, IMessagePackDifferentFirstItem
{
    public Matrix Matrix { get; set; }
    public float Unknown { get; set; }
    public string Name { get; set; }
    public int Unknown2 { get; set; }
    public int Unknown3 { get; set; }
    public int Unknown4 { get; set; }
    public bool IsFirst { get; set; }

    public void Read(MessagePackReader reader)
    {
        Matrix = new Matrix(
            reader.ReadVector3(),
            reader.ReadVector3(),
            reader.ReadVector3(),
            reader.ReadVector3()
        );

        if (IsFirst)
        {
            Unknown = 0.0f;
        }
        else if (reader.DataVersion >= 20220707)
        {
            Unknown = reader.Read<float>();
        }

        Name = reader.Read<string>();

        if (reader.DataVersion >= 20220707)
        {
            Unknown2 = reader.Read<int>();
            Unknown3 = reader.Read<int>();
            Unknown4 = reader.Read<int>();
        }
    }
}