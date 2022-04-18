using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Flagrum.Core.Ebex.Xmb2;

public class Xmb2Document
{
    private const int RootElementOffsetOffset = 12;

    public uint Identifier { get; }
    public uint FileSize { get; }
    public ushort Flags { get; }
    public ushort Version { get; }
    public int RootElementOffset { get; }

    public static Xmb2Element GetRootElement(Stream stream)
    {
        var xmb2 = new byte[stream.Length];
        stream.Read(xmb2);

        var rootElementRelativeOffset = BitConverter.ToInt32(xmb2, RootElementOffsetOffset);
        return Xmb2Element.FromByteArray(xmb2, RootElementOffsetOffset + rootElementRelativeOffset);
    }

    public static void Dump(byte[] xmb2, StringBuilder output)
    {
        var rootElementRelativeOffset = BitConverter.ToInt32(xmb2, RootElementOffsetOffset);
        var rootElement = Xmb2Element.FromByteArray(xmb2, RootElementOffsetOffset + rootElementRelativeOffset);
        Debug.Assert(rootElement != null);
        rootElement.Dump(output, 0);
    }
}