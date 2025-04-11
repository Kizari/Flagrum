using Flagrum.Core.Mathematics;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class GameModelNode : IMessagePackItem, IMessagePackDifferentFirstItem
{
    public Matrix3x4 Matrix { get; set; }
    public float Unknown { get; set; }
    public string Name { get; set; }
    public int Unknown2 { get; set; }
    public int Unknown3 { get; set; }
    public int Unknown4 { get; set; }
    public bool IsFirst { get; set; }

    public void Read(MessagePackReader reader)
    {
        Matrix = new Matrix3x4();
        Matrix.Read(reader);

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

    public void Write(MessagePackWriter writer)
    {
        Matrix.Write(writer);

        if (!IsFirst && writer.DataVersion >= 20220707)
        {
            writer.Write(Unknown);
        }

        writer.Write(Name);

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
        }
    }
}