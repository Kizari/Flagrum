using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Serialization.MessagePack;

public class MessagePackWriter : IDisposable
{
    private readonly BinaryWriter _writer;

    public MessagePackWriter(Stream stream, bool leaveOpen = false)
    {
        _writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen);
    }

    public uint DataVersion { get; set; }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        _writer?.Dispose();
    }

    public void Write(object value)
    {
        switch (value)
        {
            case null:
                _writer.Write((byte)MessagePackType.Nil);
                break;
            case bool b:
                _writer.Write(b ? (byte)MessagePackType.True : (byte)MessagePackType.False);
                break;
            case sbyte or short or int or long:
            {
                var l = (long)Convert.ChangeType(value, typeof(long));

                switch (l)
                {
                    case >= 0 and <= 0x7F:
                        _writer.Write((byte)l);
                        break;
                    case < 0 and >= -0x1F:
                        _writer.Write((byte)(MessagePackType.NegativeFixIntStart + (byte)(l * -1)));
                        break;
                    case >= sbyte.MinValue and <= sbyte.MaxValue:
                        _writer.Write((byte)MessagePackType.Int8);
                        _writer.Write((sbyte)l);
                        break;
                    case >= byte.MinValue and <= byte.MaxValue:
                        _writer.Write((byte)MessagePackType.Uint8);
                        _writer.Write((byte)l);
                        break;
                    case >= short.MinValue and <= short.MaxValue:
                        _writer.Write((byte)MessagePackType.Int16);
                        _writer.Write((short)l);
                        break;
                    case >= ushort.MinValue and <= ushort.MaxValue:
                        _writer.Write((byte)MessagePackType.Uint16);
                        _writer.Write((ushort)l);
                        break;
                    case >= int.MinValue and <= int.MaxValue:
                        _writer.Write((byte)MessagePackType.Int32);
                        _writer.Write((int)l);
                        break;
                    case >= uint.MinValue and <= uint.MaxValue:
                        _writer.Write((byte)MessagePackType.Uint32);
                        _writer.Write((uint)l);
                        break;
                    default:
                        _writer.Write((byte)MessagePackType.Int64);
                        _writer.Write(l);
                        break;
                }

                break;
            }
            case byte or ushort or uint or ulong:
            {
                var l = (ulong)Convert.ChangeType(value, typeof(ulong));

                switch (l)
                {
                    case <= 0x7F:
                        _writer.Write((byte)l);
                        break;
                    case <= byte.MaxValue:
                        _writer.Write((byte)MessagePackType.Uint8);
                        _writer.Write((byte)l);
                        break;
                    case <= ushort.MaxValue:
                        _writer.Write((byte)MessagePackType.Uint16);
                        _writer.Write((ushort)l);
                        break;
                    case <= uint.MaxValue:
                        _writer.Write((byte)MessagePackType.Uint32);
                        _writer.Write((uint)l);
                        break;
                    default:
                        _writer.Write((byte)MessagePackType.Uint64);
                        _writer.Write(l);
                        break;
                }

                break;
            }
            case float f:
                _writer.Write((byte)MessagePackType.Float32);
                _writer.Write(f);
                break;
            case double d:
                _writer.Write((byte)MessagePackType.Float64);
                _writer.Write(d);
                break;
            case string s:
                if (s.Length == 0)
                {
                    _writer.Write((byte)MessagePackType.FixStrStart);
                    break;
                }

                switch (s.Length + 1)
                {
                    case <= 0x1F:
                        _writer.Write((byte)(MessagePackType.FixStrStart + (byte)(s.Length + 1)));
                        break;
                    case <= byte.MaxValue:
                        _writer.Write((byte)MessagePackType.Str8);
                        _writer.Write((byte)(s.Length + 1));
                        break;
                    case <= ushort.MaxValue:
                        _writer.Write((byte)MessagePackType.Str16);
                        _writer.Write((ushort)(s.Length + 1));
                        break;
                    case <= int.MaxValue:
                        _writer.Write((byte)MessagePackType.Str32);
                        _writer.Write((uint)(s.Length + 1));
                        break;
                }

                _writer.WriteNullTerminatedString(s);
                break;
            case byte[] bin:
                switch (bin.Length)
                {
                    case <= byte.MaxValue:
                        _writer.Write((byte)MessagePackType.Bin8);
                        _writer.Write((byte)bin.Length);
                        break;
                    case <= ushort.MaxValue:
                        _writer.Write((byte)MessagePackType.Bin16);
                        _writer.Write((ushort)bin.Length);
                        break;
                    case <= int.MaxValue:
                        _writer.Write((byte)MessagePackType.Bin32);
                        _writer.Write((uint)bin.Length);
                        break;
                }

                _writer.Write(bin);
                break;
            case IDictionary map:
                switch (map.Count)
                {
                    case <= 0xF:
                        _writer.Write((byte)(MessagePackType.FixMapStart + (byte)map.Count));
                        break;
                    case <= ushort.MaxValue:
                        _writer.Write((byte)MessagePackType.Map16);
                        _writer.Write((ushort)map.Count);
                        break;
                    case <= int.MaxValue:
                        _writer.Write((byte)MessagePackType.Map32);
                        _writer.Write((uint)map.Count);
                        break;
                }

                foreach (var key in map.Keys)
                {
                    Write(key);
                    Write(map[key]);
                }

                break;
            case IEnumerable array:
                var list = array.Cast<object>().ToList();
                switch (list.Count)
                {
                    case <= 0xF:
                        _writer.Write((byte)(MessagePackType.FixArrayStart + (byte)list.Count));
                        break;
                    case <= ushort.MaxValue:
                        _writer.Write((byte)MessagePackType.Array16);
                        _writer.Write((ushort)list.Count);
                        break;
                    case <= int.MaxValue:
                        _writer.Write((byte)MessagePackType.Array32);
                        _writer.Write((uint)list.Count);
                        break;
                }

                var isFirst = true;
                foreach (var listItem in list)
                {
                    if (listItem is IMessagePackItem item)
                    {
                        if (listItem is IMessagePackDifferentFirstItem firstItem)
                        {
                            firstItem.IsFirst = isFirst;
                        }

                        item.Write(this);
                    }
                    else
                    {
                        Write(listItem);
                    }

                    isFirst = false;
                }

                break;
        }
    }
}