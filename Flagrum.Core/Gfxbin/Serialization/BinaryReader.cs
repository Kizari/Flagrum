using System;
using System.Text;
using Flagrum.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Gfxbin.Serialization;

public class BinaryReader
{
    private readonly byte[] _buffer;

    private int _position;

    public BinaryReader(byte[] data)
    {
        _buffer = data;
    }

    public bool UnpackBlob(out byte[] data, out uint size)
    {
        if (UnpackNil())
        {
            data = null;
            size = 0;
            return true;
        }

        if (UnpackBin8(TypeFormat.Bin8, out data, out size))
        {
            return true;
        }

        if (UnpackBin16(TypeFormat.Bin16, out data, out size))
        {
            return true;
        }

        return UnpackString(TypeFormat.Bin32, out data, out size);
    }

    private bool UnpackBin8(TypeFormat format, out byte[] data, out uint size)
    {
        if (_buffer[_position] != (byte)format)
        {
            data = default;
            size = 0;
            return false;
        }

        _position++;
        size = _buffer[_position];
        _position++;

        data = new byte[(int)size];
        Array.Copy(_buffer, _position, data, 0, (int)size);
        _position += (int)size;

        return true;
    }

    private bool UnpackBin16(TypeFormat format, out byte[] data, out uint size)
    {
        if (_buffer[_position] != (byte)format)
        {
            data = default;
            size = 0;
            return false;
        }

        _position++;
        size = BitConverter.ToUInt16(_buffer, _position);
        _position += 2;

        data = new byte[(int)size];
        Array.Copy(_buffer, _position, data, 0, (int)size);
        _position += (int)size;

        return true;
    }

    private bool UnpackString(TypeFormat format, out byte[] data, out uint size)
    {
        if (_buffer[_position] != (byte)format)
        {
            data = default;
            size = 0;
            return false;
        }

        _position++;
        size = BitConverter.ToUInt32(_buffer, _position);
        _position += 4;

        data = new byte[(int)size];
        Array.Copy(_buffer, _position, data, 0, (int)size);
        _position += (int)size;

        return true;
    }

    public bool UnpackFloat32(out float value)
    {
        if (_buffer[_position] != (byte)TypeFormat.Float32)
        {
            value = default;
            return false;
        }

        _position++;
        value = BitConverter.ToSingle(_buffer, _position);
        _position += sizeof(float);

        return true;
    }

    public bool UnpackUInt64(out ulong value)
    {
        if (UnpackUInt16(out var ushortValue))
        {
            value = ushortValue;
            return true;
        }

        if (_buffer[_position] != (byte)TypeFormat.Uint32)
        {
            return UnpackPrimitiveUInt64(TypeFormat.Uint64, out value);
        }

        _position++;
        value = BitConverter.ToUInt32(_buffer, _position);
        _position += 4;
        return true;
    }

    public bool UnpackUInt32(out uint value)
    {
        if (UnpackUInt16(out var ushortValue))
        {
            value = ushortValue;
            return true;
        }

        return UnpackContainerUInt32(TypeFormat.Uint32, out value);
    }

    public bool UnpackUInt16(out ushort value)
    {
        var val = _buffer[_position];
        if (_buffer[_position] > (byte)TypeFormat.PositiveFixIntEnd)
        {
            if (UnpackPrimitiveUInt8(TypeFormat.Uint8, out var byteValue))
            {
                value = byteValue;
                return true;
            }

            return UnpackContainerUInt16(TypeFormat.Uint16, out value);
        }

        value = _buffer[_position];
        _position++;
        return true;
    }

    public Half UnpackHalf()
    {
        // Skip format byte since we know this is a half
        _position++;

        var value = BitConverter.ToHalf(_buffer, _position);
        _position += 2; // 2 bytes in a half

        return value;
    }

    public bool UnpackInt8(out sbyte value)
    {
        if (UnpackNegativeFixNum(out value))
        {
            return true;
        }

        if (_buffer[_position] > (byte)TypeFormat.PositiveFixIntEnd)
        {
            return UnpackPrimitiveInt8(TypeFormat.Int8, out value);
        }

        value = unchecked((sbyte)_buffer[_position]);
        _position++;
        return true;
    }

    private bool UnpackContainerUInt32(TypeFormat format, out uint value)
    {
        if (_buffer[_position] != (byte)format)
        {
            value = default;
            return false;
        }

        _position++;
        value = BitConverter.ToUInt32(_buffer, _position);
        _position += sizeof(uint);

        return true;
    }

    private bool UnpackContainerUInt16(TypeFormat format, out ushort value)
    {
        if (_buffer[_position] != (byte)format)
        {
            value = default;
            return false;
        }

        _position++;
        value = BitConverter.ToUInt16(_buffer, _position);
        _position += sizeof(ushort);

        return true;
    }

    private bool UnpackPrimitiveUInt64(TypeFormat format, out ulong value)
    {
        if (_buffer[_position] != (byte)format)
        {
            value = default;
            return false;
        }

        _position++;
        value = BitConverter.ToUInt64(_buffer, _position);
        _position += 8;

        return true;
    }

    private bool UnpackPrimitiveUInt8(TypeFormat format, out byte value)
    {
        if (_buffer[_position] != (byte)format)
        {
            value = default;
            return false;
        }

        _position++;
        value = _buffer[_position];
        _position++;

        return true;
    }

    private bool UnpackPrimitiveInt8(TypeFormat format, out sbyte value)
    {
        if (_buffer[_position] != (byte)format)
        {
            value = default;
            return false;
        }

        _position++;
        value = unchecked((sbyte)_buffer[_position]);
        _position++;

        return true;
    }

    private bool UnpackNegativeFixNum(out sbyte value)
    {
        if (_buffer[_position] < (byte)TypeFormat.NegativeFixIntStart)
        {
            value = default;
            return false;
        }

        value = unchecked((sbyte)_buffer[_position]);
        _position++;

        return true;
    }

    private bool UnpackNil()
    {
        if (_buffer[_position] != (byte)TypeFormat.Nil)
        {
            return false;
        }

        _position++;
        return true;
    }


    public object Read()
    {
        object result = 0;
        var val = _buffer[_position];

        var size = 1;
        switch ((TypeFormat)val)
        {
            case TypeFormat.Uint64:
                result = BitConverter.ToUInt64(_buffer, _position + 1);
                size += sizeof(ulong);
                break;
            case TypeFormat.Uint32:
                result = BitConverter.ToUInt32(_buffer, _position + 1);
                size += sizeof(uint);
                break;
            case TypeFormat.Uint16:
                result = BitConverter.ToUInt16(_buffer, _position + 1);
                size += sizeof(ushort);
                break;
            case TypeFormat.Uint8:
                result = _buffer[_position + 1];
                size += sizeof(byte);
                break;
            case TypeFormat.Float32:
                result = BitConverter.ToSingle(_buffer, _position + 1);
                size += sizeof(float);
                break;
            case TypeFormat.Map16:
            case TypeFormat.Array16:
                result = BitConverter.ToUInt16(_buffer, _position + 1);
                size += sizeof(ushort);
                break;
            case TypeFormat.True:
                result = true;
                break;
            case TypeFormat.False:
                result = false;
                break;
            default:
                result = val;
                if (((byte)result & 0xF0) >> 4 == 9)
                {
                    result = (byte)result & 0xF;
                }

                break;
        }

        _position += size;
        return result;
    }

    public uint ReadUint()
    {
        var rawValue = Read();
        if (rawValue is uint)
        {
            return (uint)rawValue;
        }

        if (rawValue is int)
        {
            return (uint)(int)rawValue;
        }

        if (rawValue is ushort)
        {
            return (ushort)rawValue;
        }

        if (rawValue is short)
        {
            return (uint)(short)rawValue;
        }

        if (rawValue is byte)
        {
            return (byte)rawValue;
        }

        return (uint)rawValue;
    }

    public int ReadInt()
    {
        var rawValue = Read();
        if (rawValue is int value)
        {
            return value;
        }

        if (rawValue is uint)
        {
            return (int)(uint)rawValue;
        }

        if (rawValue is short)
        {
            return (short)rawValue;
        }

        if (rawValue is ushort)
        {
            return (short)(ushort)rawValue;
        }

        if (rawValue is byte)
        {
            return (byte)rawValue;
        }

        return (short)rawValue;
    }

    public ushort ReadUInt16()
    {
        var rawValue = Read();
        if (rawValue is short)
        {
            return (ushort)rawValue;
        }

        if (rawValue is ushort)
        {
            return (ushort)rawValue;
        }

        if (rawValue is byte)
        {
            return (byte)rawValue;
        }

        return (ushort)rawValue;
    }

    public ulong ReadUint64()
    {
        var rawValue = Read();
        if (rawValue is ulong x)
        {
            return x;
        }

        if (rawValue is long x2)
        {
            return (uint)x2;
        }

        return (ulong)rawValue;
    }

    public bool ReadBool()
    {
        var rawValue = Read();
        return (bool)rawValue;
    }

    public float ReadFloat()
    {
        var rawValue = Read();
        return (float)rawValue;
    }

    public uint ReadMapCount()
    {
        var rawValue = ReadByte();
        var result = rawValue & 111u;

        var size = 0;
        if (rawValue == (byte)TypeFormat.Map16)
        {
            result = BitConverter.ToUInt16(_buffer, _position);
            size += sizeof(ushort);
        }
        else if (rawValue == (byte)TypeFormat.Map32)
        {
            result = BitConverter.ToUInt32(_buffer, _position);
            size += sizeof(uint);
        }

        _position += size;
        return result;
    }

    public bool UnpackArraySize(out int size)
    {
        var format = ReadByte();
        if (format == (byte)TypeFormat.Array16)
        {
            size = BitConverter.ToUInt16(_buffer, _position);
            _position += sizeof(ushort);
            return true;
        }

        if (format == (byte)TypeFormat.Array32)
        {
            size = (int)BitConverter.ToUInt32(_buffer, _position);
            _position += sizeof(uint);
            return true;
        }

        size = format & 111;
        return true;
    }

    public string ReadString()
    {
        var format = _buffer[_position];
        var length = (int)format;
        if (format < 0xA0 || format > 0xBf)
        {
            if ((TypeFormat)format == TypeFormat.Str8)
            {
                _position++;
                length = _buffer[_position];
                _position++;
            }
            else if ((TypeFormat)format == TypeFormat.Str16)
            {
                _position++;
                length = _buffer[_position];
                _position++;
            }
            else if ((TypeFormat)format == TypeFormat.Str32)
            {
                _position++;
                length = _buffer[_position];
                _position++;
            }
        }
        else
        {
            length = format & 95;
            _position++;
        }

        //var length = this.ReadData[this.Position] & 95;
        if (length == 0)
        {
            //this.Position++;
            return string.Empty;
        }

        var result = Encoding.UTF8.GetString(_buffer, _position, length - 1);

        _position += length;
        return result;
    }

    public string ReadStr8()
    {
        var pathFormat = (TypeFormat)(byte)Read();
        var length = _buffer[_position];
        var result = Encoding.UTF8.GetString(_buffer, _position + 1, length - 1);

        _position += length + 1;
        return result;
    }

    public byte ReadByte()
    {
        var result = _buffer[_position];
        _position++;
        return result;
    }

    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }

    public Matrix ReadMatrix()
    {
        return new Matrix(ReadVector3(), ReadVector3(), ReadVector3(), ReadVector3());
    }
}