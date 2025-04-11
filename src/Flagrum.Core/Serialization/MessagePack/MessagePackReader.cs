using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Text;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Serialization.MessagePack;

public class MessagePackReader : IDisposable
{
    private readonly BinaryReader _reader;

    public MessagePackReader(Stream stream, bool leaveOpen = false)
    {
        _reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen);
    }

    public uint DataVersion { get; set; }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        _reader?.Dispose();
    }

    public TValue Read<TValue>()
    {
        var type = (MessagePackType)_reader.ReadByte();

        if ((MessagePackType)((byte)type & 0xE0) is >= MessagePackType.FixStrStart and <= MessagePackType.FixStrEnd)
        {
            var lengthFix = (byte)type & 0x1F;
            return lengthFix == 0
                ? (TValue)(object)""
                : (TValue)(object)new string(_reader.ReadChars(lengthFix)[..^1]);
        }

        if ((MessagePackType)((byte)type & 0xF0) is >= MessagePackType.FixArrayStart and <= MessagePackType.FixArrayEnd)
        {
            var array8Type = typeof(TValue).GetGenericArguments()[0];
            return (TValue)ReadArray(array8Type, (uint)((byte)type & 0xF));
        }

        if ((MessagePackType)((byte)type & 0xF0) is >= MessagePackType.FixMapStart and <= MessagePackType.FixMapEnd)
        {
            var fixMapTypes = typeof(TValue).GetGenericArguments();
            return (TValue)ReadMap(fixMapTypes[0], fixMapTypes[1], (uint)((byte)type & 0xF));
        }

        switch (type)
        {
            case <= MessagePackType.PositiveFixIntEnd:
                return (TValue)Convert.ChangeType((byte)type & 0x7F, typeof(TValue));
            case MessagePackType.Nil:
                return (TValue)(object)null;
            case MessagePackType.Unused:
                throw new NotImplementedException("Why does your file even have this!?");
            case MessagePackType.False:
                return (TValue)(object)false;
            case MessagePackType.True:
                return (TValue)(object)true;
            case MessagePackType.Bin8:
                var size8 = _reader.ReadByte();
                return (TValue)(object)_reader.ReadBytes(size8);
            case MessagePackType.Bin16:
                var size16 = _reader.ReadUInt16();
                return (TValue)(object)_reader.ReadBytes(size16);
            case MessagePackType.Bin32:
                var size32 = _reader.ReadUInt32();
                return (TValue)(object)_reader.ReadBytes((int)size32);
            case MessagePackType.Ext8:
                throw new NotImplementedException();
            case MessagePackType.Ext16:
                throw new NotImplementedException();
            case MessagePackType.Ext32:
                throw new NotImplementedException();
            case MessagePackType.Float32:
                return (TValue)(object)_reader.ReadSingle();
            case MessagePackType.Float64:
                return (TValue)(object)_reader.ReadDouble();
            case MessagePackType.Uint8:
                return (TValue)Convert.ChangeType(_reader.ReadByte(), typeof(TValue));
            case MessagePackType.Uint16:
                return (TValue)Convert.ChangeType(_reader.ReadUInt16(), typeof(TValue));
            case MessagePackType.Uint32:
                return (TValue)Convert.ChangeType(_reader.ReadUInt32(), typeof(TValue));
            case MessagePackType.Uint64:
                return (TValue)(object)_reader.ReadUInt64();
            case MessagePackType.Int8:
                return (TValue)(object)_reader.ReadSByte();
            case MessagePackType.Int16:
                return (TValue)Convert.ChangeType(_reader.ReadInt16(), typeof(TValue));
            case MessagePackType.Int32:
                return (TValue)(object)_reader.ReadInt32();
            case MessagePackType.Int64:
                return (TValue)(object)_reader.ReadInt64();
            case MessagePackType.FixExt1:
                throw new NotImplementedException();
            case MessagePackType.FixExt2:
                throw new NotImplementedException();
            case MessagePackType.FixExt4:
                throw new NotImplementedException();
            case MessagePackType.FixExt8:
                throw new NotImplementedException();
            case MessagePackType.FixExt16:
                throw new NotImplementedException();
            case MessagePackType.Str8:
                var length8 = _reader.ReadByte();
                return (TValue)(object)new string(_reader.ReadChars(length8)[..^1]);
            case MessagePackType.Str16:
                var length16 = _reader.ReadUInt16();
                return (TValue)(object)new string(_reader.ReadChars(length16)[..^1]);
            case MessagePackType.Str32:
                var length32 = _reader.ReadUInt32();
                return (TValue)(object)new string(_reader.ReadChars((int)length32)[..^1]);
            case MessagePackType.Array16:
                var array16Size = _reader.ReadUInt16();
                var array16Type = typeof(TValue).GetGenericArguments()[0];
                return (TValue)ReadArray(array16Type, array16Size);
            case MessagePackType.Array32:
                var array32Size = _reader.ReadUInt32();
                var array32Type = typeof(TValue).GetGenericArguments()[0];
                return (TValue)ReadArray(array32Type, array32Size);
            case MessagePackType.Map16:
                var map16Size = _reader.ReadUInt16();
                var map16Types = typeof(TValue).GetGenericArguments();
                return (TValue)ReadMap(map16Types[0], map16Types[1], map16Size);
            case MessagePackType.Map32:
                var map32Size = _reader.ReadUInt32();
                var map32Types = typeof(TValue).GetGenericArguments();
                return (TValue)ReadMap(map32Types[0], map32Types[1], map32Size);
            case >= MessagePackType.NegativeFixIntStart:
                return (TValue)(object)-((byte)type & 0x1F);
            default:
                throw new Exception("How?");
        }
    }

    private object ReadMap(Type key, Type value, uint size)
    {
        var type = typeof(Dictionary<,>).MakeGenericType(key, value);
        var dictionary = (IDictionary)Activator.CreateInstance(type);

        for (var i = 0; i < size; i++)
        {
            var method = typeof(MessagePackReader).GetMethod(nameof(Read))!;
            var keyMethod = method.MakeGenericMethod(key);
            var valueMethod = method.MakeGenericMethod(value);

            dictionary!.Add(
                keyMethod.Invoke(this, Array.Empty<object>())!,
                valueMethod.Invoke(this, Array.Empty<object>())
            );
        }

        return dictionary;
    }

    private object ReadArray(Type type, uint size)
    {
        var listType = typeof(List<>).MakeGenericType(type);
        var list = (IList)Activator.CreateInstance(listType)!;

        for (var i = 0; i < size; i++)
        {
            if (type.IsAssignableTo(typeof(IMessagePackItem)))
            {
                var item = (IMessagePackItem)Activator.CreateInstance(type)!;

                if (type.IsAssignableTo(typeof(IMessagePackDifferentFirstItem)))
                {
                    ((IMessagePackDifferentFirstItem)item).IsFirst = i == 0;
                }

                item.Read(this);
                list.Add(item);
            }
            else
            {
                var method = typeof(MessagePackReader).GetMethod(nameof(Read))!.MakeGenericMethod(type);
                list.Add(method.Invoke(this, Array.Empty<object>()));
            }
        }

        return list;
    }

    private string GetTypeString(MessagePackType type)
    {
        return type switch
        {
            >= MessagePackType.PositiveFixIntStart and <= MessagePackType.PositiveFixIntEnd => "PositiveFixInt",
            >= MessagePackType.FixMapStart and <= MessagePackType.FixMapEnd => "FixMap",
            >= MessagePackType.FixArrayStart and <= MessagePackType.FixArrayEnd => "FixArray",
            >= MessagePackType.FixStrStart and <= MessagePackType.FixStrEnd => "FixString",
            >= MessagePackType.NegativeFixIntStart or <= MessagePackType.NegativeFixIntEnd => "NegativeFixInt"
        };
    }

    public void Seek(long offset, SeekOrigin origin)
    {
        _reader.BaseStream.Seek(offset, origin);
    }

    public Vector3 ReadVector3() => new(Read<float>(), Read<float>(), Read<float>());
}