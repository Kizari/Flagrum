using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterialColour
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public void Read(MessagePackReader reader)
    {
        R = reader.Read<float>();
        G = reader.Read<float>();
        B = reader.Read<float>();
        A = reader.Read<float>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(R);
        writer.Write(G);
        writer.Write(B);
        writer.Write(A);
    }
}