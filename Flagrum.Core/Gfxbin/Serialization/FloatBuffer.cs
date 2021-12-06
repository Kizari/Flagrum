using System;

namespace Flagrum.Core.Gfxbin.Serialization;

public class FloatBuffer
{
    private readonly byte[] _buffer;

    public FloatBuffer(uint length)
    {
        _buffer = new byte[length];
    }

    public void Put(int offset, float[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            var bytes = BitConverter.GetBytes(values[i]);
            for (var j = 0; j < 4; j++)
            {
                _buffer[offset + i * 4 + j] = bytes[j];
            }
        }
    }

    public byte[] ToArray()
    {
        return _buffer;
    }
}