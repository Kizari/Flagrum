using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Flagrum.Core.Archive;

public class EbonyReplace
{
    public EbonyReplace(byte[] erep)
    {
        Replacements = new Dictionary<ulong, ulong>();

        var data = MemoryMarshal.Cast<byte, ulong>(erep.AsSpan());
        for (var i = 0; i < data.Length; i += 2)
        {
            Replacements.Add(data[i], data[i + 1]);
        }
    }

    public Dictionary<ulong, ulong> Replacements { get; set; }

    public byte[] ToArray()
    {
        using var stream = new MemoryStream();
        Write(stream, true);
        return stream.ToArray();
    }

    public void Write(Stream destination, bool leaveOpen = false)
    {
        using var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen);

        foreach (var (original, replacement) in Replacements)
        {
            writer.Write(original);
            writer.Write(replacement);
        }
    }
}