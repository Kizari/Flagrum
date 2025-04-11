using System;
using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Serialization;

public class StringBuffer : IDisposable
{
    private readonly BinaryReader _reader;
    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;
    private long _offset;
    private readonly Dictionary<string, long> _offsets = new();

    public StringBuffer()
    {
        _stream = new MemoryStream();
        _reader = new BinaryReader(_stream);
        _writer = new BinaryWriter(_stream);
    }

    public StringBuffer(byte[] buffer)
    {
        _stream = new MemoryStream(buffer);
        _reader = new BinaryReader(_stream);
        _writer = new BinaryWriter(_stream);
    }

    public long Length => _stream.Length;

    public void Dispose()
    {
        _stream?.Dispose();
        _reader?.Dispose();
        _writer?.Dispose();
    }

    public string Get(ulong offset)
    {
        _stream.Seek((long)offset, SeekOrigin.Begin);
        return _reader.ReadNullTerminatedString();
    }

    public ulong Put(string value)
    {
        var offset = 0L;
        // if (!_offsets.TryGetValue(value, out var offset))
        {
            offset = _stream.Position;
            _offsets[value] = offset;
            _writer.WriteNullTerminatedString(value);
        }

        return (ulong)offset;
    }

    public byte[] ToArray()
    {
        return _stream.ToArray();
    }
}