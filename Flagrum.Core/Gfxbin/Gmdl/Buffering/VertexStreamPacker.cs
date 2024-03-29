﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Flagrum.Core.Gfxbin.Gmdl.Buffering;

public class VertexStreamPacker
{
    private readonly Dictionary<Type, Func<object, byte[]>> _getBytes = new()
    {
        {typeof(byte), value => BitConverter.GetBytes((byte)value)},
        {typeof(sbyte), value => BitConverter.GetBytes((sbyte)value)},
        {typeof(ushort), value => BitConverter.GetBytes((ushort)value)},
        {typeof(short), value => BitConverter.GetBytes((short)value)},
        {typeof(uint), value => BitConverter.GetBytes((uint)value)},
        {typeof(int), value => BitConverter.GetBytes((int)value)},
        {typeof(Half), value => BitConverter.GetBytes((Half)value)},
        {typeof(float), value => BitConverter.GetBytes((float)value)}
    };

    private readonly MemoryStream _stream = new();

    public uint Put<TValue>(IEnumerable<TValue> values)
        where TValue : struct, IComparable, IFormattable, IComparable<TValue>, IEquatable<TValue>
    {
        var offset = _stream.Position;
        foreach (var v in values)
        {
            _stream.Write(_getBytes[typeof(TValue)](v));
        }

        return (uint)offset;
    }

    public Stream ToStream()
    {
        return _stream;
    }
}