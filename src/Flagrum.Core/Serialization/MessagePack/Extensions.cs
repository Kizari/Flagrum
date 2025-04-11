using System.Numerics;

namespace Flagrum.Core.Serialization.MessagePack;

public static class Extensions
{
    public static void Write(this Vector3 vector, MessagePackWriter writer)
    {
        writer.Write(vector.X);
        writer.Write(vector.Y);
        writer.Write(vector.Z);
    }
}