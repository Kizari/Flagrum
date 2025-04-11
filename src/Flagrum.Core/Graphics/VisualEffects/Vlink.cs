using System.IO;

namespace Flagrum.Core.Graphics.VisualEffects;

/// <summary>
/// This format isn't yet reversed, so this class is here purely for crudely pulling dependency URIs from the
/// binary data at this stage. This class should be updated when more is known about the format.
/// </summary>
public class Vlink
{
    public static string GetVfxUriFromData(byte[] vlink)
    {
        using var stream = new MemoryStream(vlink);
        using var reader = new BinaryReader(stream);
        stream.Seek(4, SeekOrigin.Begin);
        var count = reader.ReadByte();
        var chars = reader.ReadChars(count);
        return new string(chars);
    }
}