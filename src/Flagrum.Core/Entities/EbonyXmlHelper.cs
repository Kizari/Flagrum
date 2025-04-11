using System;
using Flagrum.Core.Entities.Xml;
using Flagrum.Core.Entities.Xml2;

namespace Flagrum.Core.Entities;

public static class EbonyXmlHelper
{
    /// <summary>
    /// Converts XMB or XMB2 files to XML
    /// </summary>
    /// <param name="data">The XMB or XMB2 data</param>
    /// <returns>The resulting XML in a byte buffer</returns>
    /// <exception cref="Exception">Will throw if magic is not either XMB or XMB2</exception>
    public static byte[] ToXml(byte[] data)
    {
        return BitConverter.ToUInt32(data, 0) switch
        {
            0x32424D58 => XmlBinary2Document.ToXml(data),
            0x00424D58 => XmlBinary.ToXml(data),
            _ => throw new Exception("Invalid XMB type")
        };
    }
}