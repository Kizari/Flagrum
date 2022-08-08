using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Flagrum.Core.Ebex.Xmb2;

public static class Xmb2Document
{
    private const int RootElementOffsetOffset = 12;

    public static Xmb2Element GetRootElement(Stream stream)
    {
        var xmb2 = new byte[stream.Length];
        _ = stream.Read(xmb2);

        var rootElementRelativeOffset = BitConverter.ToInt32(xmb2, RootElementOffsetOffset);
        return Xmb2Element.FromByteArray(xmb2, RootElementOffsetOffset + rootElementRelativeOffset);
    }

    public static void Dump(byte[] xmb2, StringBuilder output)
    {
        // Need to set to invariant culture to avoid decimal symbol issues and such
        var previousCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var rootElementRelativeOffset = BitConverter.ToInt32(xmb2, RootElementOffsetOffset);
        var rootElement = Xmb2Element.FromByteArray(xmb2, RootElementOffsetOffset + rootElementRelativeOffset);
        Debug.Assert(rootElement != null);
        rootElement.Dump(output, 0);

        Thread.CurrentThread.CurrentCulture = previousCulture;
    }
}