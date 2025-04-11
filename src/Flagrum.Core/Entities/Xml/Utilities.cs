using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flagrum.Core.Entities.Xml;

public static class Utilities
{
    private static readonly List<byte> _buffer = new(0x100);

    public static string ReadCString(this Stream stream)
    {
        return ReadCString(stream, Encoding.UTF8);
    }

    public static string ReadCString(this Stream stream, Encoding encoding)
    {
        _buffer.Clear();

        byte c;
        while ((c = (byte)stream.ReadByte()) != 0)
        {
            _buffer.Add(c);
        }

        return encoding.GetString(_buffer.ToArray());
    }

    public static int WriteCString(this Stream stream, string str)
    {
        return WriteCString(stream, str, Encoding.UTF8);
    }

    public static int WriteCString(this Stream stream, string str, Encoding encoding)
    {
        var buffer = encoding.GetBytes(str);
        stream.Write(buffer, 0, buffer.Length);
        stream.WriteByte(0);

        return buffer.Length + 1;
    }
}