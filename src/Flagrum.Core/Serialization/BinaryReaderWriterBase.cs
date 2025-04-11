using System;
using System.IO;

namespace Flagrum.Core.Serialization;

public class BinaryReaderWriterBase
{
    public void Read(byte[] buffer)
    {
        using var stream = new MemoryStream(buffer);
        Read(stream);
    }

    public byte[] Write()
    {
        using var stream = new MemoryStream();
        Write(stream);
        return stream.ToArray();
    }

    public void Read(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        Read(stream);
    }

    public void Write(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        Write(stream);
    }

    public virtual void Read(Stream stream)
    {
        throw new NotImplementedException("This method should not be called from the base class");
    }

    public virtual void Write(Stream stream)
    {
        throw new NotImplementedException("This method should not be called from the base class");
    }
}