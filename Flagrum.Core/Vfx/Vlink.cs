using System;
using System.Collections.Generic;
using System.IO;

namespace Flagrum.Core.Vfx;

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