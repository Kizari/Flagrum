using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Flagrum.Gfxbin.Gmdl.Buffering;

public class FaceIndexPacker<TIndex> where TIndex : struct
{
    private readonly Action<IEnumerable<int>> _putValues;
    private readonly MemoryStream _stream = new();

    public FaceIndexPacker()
    {
        if (typeof(TIndex) == typeof(int))
        {
            _putValues = PutInt32;
        }
        else if (typeof(TIndex) == typeof(short))
        {
            _putValues = PutInt16;
        }
        else
        {
            throw new ConstraintException(
                $"Face index type ({nameof(TIndex)}) must be {nameof(Int32)} or {nameof(Int16)}.");
        }
    }

    public void Put(IEnumerable<int> values)
    {
        _putValues(values);
    }

    private void PutInt32(IEnumerable<int> values)
    {
        foreach (var value in values)
        {
            _stream.Write(BitConverter.GetBytes(value));
        }
    }

    private void PutInt16(IEnumerable<int> values)
    {
        foreach (var value in values)
        {
            _stream.Write(BitConverter.GetBytes(value));
        }
    }
}