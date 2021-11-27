using Luminous.Xml;
using System;
using System.Diagnostics;
using System.Text;

namespace Luminous
{
    public class Xmb2Document
    {
        public uint Identifier { get; }
        public uint FileSize { get; }
        public ushort Flags { get; }
        public ushort Version { get; }
        public int RootElementOffset { get; }

        private const int RootElementOffsetOffset = 12;
        public static void Dump(byte[] xmb2, StringBuilder output)
        {
            var rootElementRelativeOffset = BitConverter.ToInt32(xmb2, RootElementOffsetOffset);
            var rootElement = Xmb2Element.FromByteArray(xmb2, RootElementOffsetOffset + rootElementRelativeOffset);
            Debug.Assert(rootElement != null);

            rootElement.Dump(xmb2, output, 0);
        }
    }
}
