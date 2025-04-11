using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Data.Tags;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Data.Bins;

public class ParameterTable
{
    private readonly uint _endOfStringBuffer;

    public ParameterTable(uint endOfStringBuffer)
    {
        _endOfStringBuffer = endOfStringBuffer;
    }

    public uint Id { get; set; }
    public uint TagCount { get; set; }
    public uint ElementCount { get; set; }
    public uint ElementSize { get; set; }
    public uint BooleanOffset { get; set; }
    public uint ArrayOffset { get; set; }
    public uint StringOffset { get; set; }
    public ParameterTableTag[] Tags { get; set; }
    public List<ParameterTableElement> Elements { get; set; } = new();
    public List<object> ArrayBuffer { get; set; } = new();
    public List<byte[]> StringBuffer { get; set; } = new();
    public List<uint> StringOffsets { get; set; } = new();

    public byte[] ArrayBufferAsBytes { get; set; }

    public void Read(BinaryReader reader)
    {
        Id = reader.ReadUInt32();
        TagCount = reader.ReadUInt32();
        ElementCount = reader.ReadUInt32();
        ElementSize = reader.ReadUInt32();
        BooleanOffset = reader.ReadUInt32();
        ArrayOffset = reader.ReadUInt32();
        StringOffset = reader.ReadUInt32();

        _ = reader.ReadUInt32(); // Skip dummy

        Tags = new ParameterTableTag[TagCount];

        for (var i = 0; i < TagCount; i++)
        {
            Tags[i] = new ParameterTableTag();
            Tags[i].Read(reader);
        }

        for (var i = 0; i < ElementCount; i++)
        {
            var element = new ParameterTableElement(ElementSize / 4 - 1);
            element.Read(reader);
            Elements.Add(element);
        }

        var arrayBufferSize = ArrayOffset > StringOffset ? 0 : StringOffset - ArrayOffset;
        var returnAddress = reader.BaseStream.Position;
        ArrayBufferAsBytes = new byte[arrayBufferSize];
        _ = reader.Read(ArrayBufferAsBytes);
        reader.BaseStream.Seek(returnAddress, SeekOrigin.Begin);

        if (arrayBufferSize > 0)
        {
            var arrayBufferCount = (StringOffset - ArrayOffset) / 4;
            for (var i = 0; i < arrayBufferCount; i++)
            {
                ArrayBuffer.Add(reader.ReadUInt32());
            }
        }

        if (ElementCount > 0)
        {
            var stringBufferStart = reader.BaseStream.Position;
            
            if (reader.BaseStream.Position < _endOfStringBuffer)
            {
                _ = reader.ReadByte(); // Skip guard byte
            }

            while (reader.BaseStream.Position < _endOfStringBuffer)
            {
                StringOffsets.Add((uint)(reader.BaseStream.Position - stringBufferStart));
                StringBuffer.Add(reader.ReadNullTerminatedStringAsBytes());
                var next = reader.ReadByte();
                reader.BaseStream.Seek(-1, SeekOrigin.Current);

                if (next == 0)
                {
                    break;
                }
            }

            reader.Align(4);
        }
    }

    public void Write(BinaryWriter writer)
    {
        ElementCount = (uint)Elements.Count;

        writer.Write(Id);
        writer.Write(TagCount);
        writer.Write(ElementCount);
        writer.Write(ElementSize);

        var offsetsStart = writer.BaseStream.Position;

        writer.BaseStream.Seek(16, SeekOrigin.Current);

        var tagsStart = writer.BaseStream.Position;

        foreach (var tag in Tags)
        {
            tag.Write(writer);
        }

        foreach (var element in Elements.OrderBy(e => e.Id))
        {
            element.Write(writer);
        }

        ArrayOffset = (uint)(writer.BaseStream.Position - tagsStart);

        foreach (var value in ArrayBuffer)
        {
            writer.WriteParameterTableValue(value);
        }

        StringOffset = (uint)(writer.BaseStream.Position - tagsStart);

        writer.BaseStream.Seek(offsetsStart, SeekOrigin.Begin);
        writer.Write(BooleanOffset);
        writer.Write(ArrayOffset);
        writer.Write(StringOffset);
        writer.Write(0u); // Write dummy
        writer.BaseStream.Seek(StringOffset + tagsStart, SeekOrigin.Begin);

        if (StringBuffer.Count > 0)
        {
            writer.Write((byte)0); // Write guard byte

            foreach (var value in StringBuffer)
            {
                writer.Write(value);
            }

            writer.Align(4, 0x00);
        }
    }

    public ParameterTableElement GetElement(uint fixid)
    {
        return Elements.FirstOrDefault(e => e.Id == fixid);
    }

    public uint GetInteger(ParameterTableElement element, IntegerTag tag)
    {
        var match = Tags.First(t => t.Id == (uint)(object)tag);
        return (uint)element.Values[match.Offset];
    }

    public object Get(ParameterTableElement element, uint tag)
    {
        var match = Tags.First(t => t.Id == tag);
        return element.Values[match.Offset];
    }

    public void Set(ParameterTableElement element, uint tag, object value)
    {
        var match = Tags.First(t => t.Id == tag);
        element.Values[match.Offset] = value;
    }

    public IEnumerable<uint> GetIntegers(ParameterTableElement element, IEnumerable<IntegerTag> tags)
    {
        return tags.Select(tag => GetInteger(element, tag));
    }

    public void SetInteger(ParameterTableElement element, IntegerTag tag, uint value)
    {
        var match = Tags.First(t => t.Id == (uint)(object)tag);
        element.Values[match.Offset] = value;
    }

    public void SetIntegers(ParameterTableElement element, IEnumerable<IntegerTag> tags, IEnumerable<uint> values)
    {
        for (var i = 0; i < tags.Count(); i++)
        {
            SetInteger(element, tags.ElementAt(i), values.ElementAt(i));
        }
    }

    public string GetString(ParameterTableElement element, StringTag tag)
    {
        var tempBuffer = StringBuffer.SelectMany(s => s).ToArray();
        var size = tempBuffer.Length + 1;
        var buffer = new byte[size];
        Array.Copy(tempBuffer, 0, buffer, 1, tempBuffer.Length);

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        var match = Tags.First(t => t.Id == (uint)(object)tag);
        var offset = (uint)element.Values[match.Offset];
        stream.Seek(offset, SeekOrigin.Begin);

        return reader.ReadNullTerminatedString(Encoding.GetEncoding(65001));
    }

    public void SetString(ParameterTableElement element, StringTag tag, string value)
    {
        var stringBufferSize = StringBuffer.Sum(s => s.Length) + 1; // Extra 1 for guard byte
        SetInteger(element, (IntegerTag)tag, (uint)stringBufferSize);
        StringBuffer.Add(value.ToNullTerminatedBytes());
    }

    public object[] GetArray(ParameterTableElement element, ArrayTag tag)
    {
        var match = Tags.First(t => t.Id == (uint)(object)tag);
        var startIndex = (uint)element.Values[match.Offset];

        var count = (uint)ArrayBuffer[(int)startIndex];
        var array = new object[count];
        for (var i = 1; i <= count; i++)
        {
            array[i - 1] = ArrayBuffer[(int)startIndex + i];
        }

        return array;
    }

    public string[] GetStringArray(ParameterTableElement element, StringTag tag)
    {
        var tempBuffer = StringBuffer.SelectMany(s => s).ToArray();
        var size = tempBuffer.Length + 1;
        var buffer = new byte[size];
        Array.Copy(tempBuffer, 0, buffer, 1, tempBuffer.Length);

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        var match = Tags.First(t => t.Id == (uint)(object)tag);
        var startIndex = (uint)element.Values[match.Offset];

        var count = (uint)ArrayBuffer[(int)startIndex];
        var array = new string[count];
        for (var i = 1; i <= count; i++)
        {
            var offset = (uint)ArrayBuffer[(int)startIndex + i];
            stream.Seek(offset, SeekOrigin.Begin);
            array[i - 1] = reader.ReadNullTerminatedString();
        }

        return array;
    }

    public void SetArray(ParameterTableElement element, ArrayTag tag, object[] array)
    {
        SetInteger(element, (IntegerTag)tag, (uint)ArrayBuffer.Count);
        ArrayBuffer.Add((uint)array.Length);

        foreach (var item in array)
        {
            ArrayBuffer.Add(item);
        }
    }

    public void SetStringArray(ParameterTableElement element, StringTag tag, string[] array)
    {
        var offsets = new uint[array.Length + 1];

        for (var i = 0; i < array.Length; i++)
        {
            var stringBufferSize = StringBuffer.Sum(s => s.Length) + 1; // Extra 1 for guard byte
            offsets[i] = (uint)stringBufferSize;
            StringBuffer.Add(array[i].ToNullTerminatedBytes());
        }

        SetInteger(element, (IntegerTag)tag, (uint)ArrayBuffer.Count);
        ArrayBuffer.Add((uint)offsets.Length);

        foreach (var item in offsets)
        {
            ArrayBuffer.Add(item);
        }
    }

    public bool GetBoolean(ParameterTableElement element, BooleanTag tag)
    {
        var match = Tags.First(t => t.Id == (uint)(object)tag);
        var offset = match.Offset;
        var booleans = (uint)element.Values[(int)BooleanOffset + offset / 32];
        return ((booleans >> offset) & 0x1) > 0;
    }

    public void SetBoolean(ParameterTableElement element, BooleanTag tag, bool value)
    {
        var match = Tags.First(t => t.Id == (uint)(object)tag);
        var offset = match.Offset;
        var booleans = (uint)element.Values[(int)BooleanOffset + offset / 32];

        if (value)
        {
            booleans |= 1u << offset;
        }
        else
        {
            booleans &= ~(1u << offset);
        }

        element.Values[(int)BooleanOffset + offset / 32] = booleans;
    }
}