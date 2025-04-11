using System.IO;

namespace Flagrum.Core.Serialization;

public class ValueBuffer
{
    private readonly byte[] _buffer;

    public ValueBuffer(uint size)
    {
        _buffer = new byte[size];
    }

    public ValueBuffer(byte[] buffer)
    {
        _buffer = buffer;
    }

    public float Get(long offset)
    {
        using var stream = new MemoryStream(_buffer);
        using var reader = new BinaryReader(stream);
        stream.Seek(offset, SeekOrigin.Begin);
        return reader.ReadSingle();
    }

    public void Put(long offset, float[] values)
    {
        using var stream = new MemoryStream(_buffer);
        using var writer = new BinaryWriter(stream);
        stream.Seek(offset, SeekOrigin.Begin);

        foreach (var value in values)
        {
            writer.Write(value);
        }
    }

    public byte[] ToArray()
    {
        return _buffer;
    }
}