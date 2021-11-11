using System;
using System.IO;
using System.Text;

namespace Flagrum.Gfxbin.Serialization
{
    public class BinaryReader
    {
        private byte[] _buffer;

        public uint Version { get; }
        public int Position { get; set; }

        public BinaryReader(string path, out uint version)
        {
            _buffer = File.ReadAllBytes(path);
            version = (uint)Read();
            Version = version;
        }

        public bool UnpackBlob(out byte[] data, out uint size)
        {
            if (UnpackNil())
            {
                data = null;
                size = 0;
                return true;
            }
            else if (UnpackBin8(TypeFormat.Bin8, out data, out size))
            {
                return true;
            }
            else if (UnpackBin16(TypeFormat.Bin16, out data, out size))
            {
                return true;
            }
            else
            {
                return UnpackString(TypeFormat.Bin32, out data, out size);
            }
        }

        private bool UnpackBin8(TypeFormat format, out byte[] data, out uint size)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                data = default;
                size = 0;
                return false;
            }

            this.Position++;
            size = this._buffer[this.Position];
            this.Position++;

            data = new byte[(int)size];
            Array.Copy(this._buffer, this.Position, data, 0, (int)size);
            this.Position += (int)size;

            return true;
        }

        private bool UnpackBin16(TypeFormat format, out byte[] data, out uint size)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                data = default;
                size = 0;
                return false;
            }

            this.Position++;
            size = BitConverter.ToUInt16(this._buffer, this.Position);
            this.Position += 2;

            data = new byte[(int)size];
            Array.Copy(this._buffer, this.Position, data, 0, (int)size);
            this.Position += (int)size;

            return true;
        }

        private bool UnpackString(TypeFormat format, out byte[] data, out uint size)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                data = default;
                size = 0;
                return false;
            }

            this.Position++;
            size = BitConverter.ToUInt32(this._buffer, this.Position);
            this.Position += 4;

            data = new byte[(int)size];
            Array.Copy(this._buffer, this.Position, data, 0, (int)size);
            this.Position += (int)size;

            return true;
        }

        public bool UnpackFloat32(out float value)
        {
            if (this._buffer[this.Position] != (byte)TypeFormat.Float32)
            {
                value = default;
                return false;
            }

            this.Position++;
            value = BitConverter.ToSingle(this._buffer, this.Position);
            this.Position += sizeof(float);

            return true;
        }

        public bool UnpackUInt64(out ulong value)
        {
            if (this.UnpackUInt16(out ushort ushortValue))
            {
                value = ushortValue;
                return true;
            }

            if (this._buffer[this.Position] != (byte)TypeFormat.Uint32)
            {
                return UnpackPrimitiveUInt64(TypeFormat.Uint64, out value);
            }

            this.Position++;
            value = BitConverter.ToUInt32(this._buffer, this.Position);
            this.Position += 4;
            return true;
        }

        public bool UnpackUInt32(out uint value)
        {
            if (this.UnpackUInt16(out ushort ushortValue))
            {
                value = (uint)ushortValue;
                return true;
            }

            return UnpackContainerUInt32(TypeFormat.Uint32, out value);
        }

        public bool UnpackUInt16(out ushort value)
        {
            var val = this._buffer[this.Position];
            if (this._buffer[this.Position] > (byte)TypeFormat.PositiveFixIntEnd)
            {
                if (UnpackPrimitiveUInt8(TypeFormat.Uint8, out byte byteValue))
                {
                    value = (ushort)byteValue;
                    return true;
                }
                else
                {
                    return UnpackContainerUInt16(TypeFormat.Uint16, out value);
                }
            }
            else
            {
                value = this._buffer[this.Position];
                this.Position++;
                return true;
            }
        }


        public bool UnpackInt8(out sbyte value)
        {
            if (this.UnpackNegativeFixNum(out value))
            {
                return true;
            }

            if (this._buffer[this.Position] > (byte)TypeFormat.PositiveFixIntEnd)
            {
                return UnpackPrimitiveInt8(TypeFormat.Int8, out value);
            }

            value = unchecked((sbyte)this._buffer[this.Position]);
            this.Position++;
            return true;
        }

        private bool UnpackContainerUInt32(TypeFormat format, out uint value)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                value = default;
                return false;
            }

            this.Position++;
            value = BitConverter.ToUInt32(this._buffer, this.Position);
            this.Position += sizeof(uint);

            return true;
        }

        private bool UnpackContainerUInt16(TypeFormat format, out ushort value)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                value = default;
                return false;
            }

            this.Position++;
            value = BitConverter.ToUInt16(this._buffer, this.Position);
            this.Position += sizeof(ushort);

            return true;
        }

        private bool UnpackPrimitiveUInt64(TypeFormat format, out ulong value)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                value = default;
                return false;
            }

            this.Position++;
            value = BitConverter.ToUInt64(this._buffer, this.Position);
            this.Position += 8;

            return true;
        }

        private bool UnpackPrimitiveUInt8(TypeFormat format, out byte value)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                value = default;
                return false;
            }

            this.Position++;
            value = this._buffer[this.Position];
            this.Position++;

            return true;
        }

        private bool UnpackPrimitiveInt8(TypeFormat format, out sbyte value)
        {
            if (this._buffer[this.Position] != (byte)format)
            {
                value = default;
                return false;
            }

            this.Position++;
            value = unchecked((sbyte)this._buffer[this.Position]);
            this.Position++;

            return true;
        }

        private bool UnpackNegativeFixNum(out sbyte value)
        {
            if (this._buffer[this.Position] < (byte)TypeFormat.NegativeFixIntStart)
            {
                value = default;
                return false;
            }

            value = unchecked((sbyte)this._buffer[this.Position]);
            this.Position++;

            return true;
        }

        private bool UnpackNil()
        {
            if (this._buffer[this.Position] != (byte)TypeFormat.Nil)
            {
                return false;
            }

            this.Position++;
            return true;
        }


        public object Read()
        {
            object result = 0;
            var val = this._buffer[this.Position];

            int size = 1;
            switch ((TypeFormat)(val))
            {
                case TypeFormat.Uint64:
                    result = BitConverter.ToUInt64(this._buffer, this.Position + 1);
                    size += sizeof(ulong);
                    break;
                case TypeFormat.Uint32:
                    result = BitConverter.ToUInt32(this._buffer, this.Position + 1);
                    size += sizeof(uint);
                    break;
                case TypeFormat.Uint16:
                    result = BitConverter.ToUInt16(this._buffer, this.Position + 1);
                    size += sizeof(ushort);
                    break;
                case TypeFormat.Uint8:
                    result = this._buffer[this.Position + 1];
                    size += sizeof(byte);
                    break;
                case TypeFormat.Float32:
                    result = BitConverter.ToSingle(this._buffer, this.Position + 1);
                    size += sizeof(float);
                    break;
                case TypeFormat.Map16:
                case TypeFormat.Array16:
                    result = BitConverter.ToUInt16(this._buffer, this.Position + 1);
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

            this.Position += size;
            return result;
        }

        public uint ReadUint()
        {
            var rawValue = this.Read();
            if (rawValue is uint)
            {
                return (uint)rawValue;
            }
            else if (rawValue is int)
            {
                return (uint)(int)rawValue;
            }
            else if (rawValue is ushort)
            {
                return (uint)(ushort)rawValue;
            }
            else if (rawValue is short)
            {
                return (uint)(short)rawValue;
            }
            else if (rawValue is byte)
            {
                return (uint)(byte)rawValue;
            }

            return (uint)rawValue;
        }

        public int ReadInt()
        {
            var rawValue = this.Read();
            if (rawValue is int)
            {
                return (int)rawValue;
            }
            else if (rawValue is uint)
            {
                return (int)(uint)rawValue;
            }
            else if (rawValue is short)
            {
                return (short)rawValue;
            }
            else if (rawValue is ushort)
            {
                return (short)(ushort)rawValue;
            }
            else if (rawValue is byte)
            {
                return (short)(byte)rawValue;
            }

            return (short)rawValue;
        }

        public ushort ReadUInt16()
        {
            var rawValue = this.Read();
            if (rawValue is short)
            {
                return (ushort)rawValue;
            }
            else if (rawValue is ushort)
            {
                return (ushort)rawValue;
            }
            else if (rawValue is byte)
            {
                return (ushort)(byte)rawValue;
            }

            return (ushort)rawValue;
        }

        public ulong ReadUint64()
        {
            var rawValue = this.Read();
            if (rawValue is ulong x)
            {
                return x;
            }
            else if (rawValue is long x2)
            {
                return (uint)x2;
            }

            return (ulong)rawValue;
        }

        public bool ReadBool()
        {
            var rawValue = this.Read();
            return (bool)rawValue;
        }

        public float ReadFloat()
        {
            var rawValue = this.Read();
            return (float)rawValue;
        }

        public uint ReadMapCount()
        {
            var rawValue = this.ReadByte();
            uint result = rawValue & 111u;

            var size = 0;
            if (rawValue == (byte)TypeFormat.Map16)
            {
                result = (uint)BitConverter.ToUInt16(this._buffer, this.Position);
                size += sizeof(ushort);
            }
            else if (rawValue == (byte)TypeFormat.Map32)
            {
                result = BitConverter.ToUInt32(this._buffer, this.Position);
                size += sizeof(uint);
            }

            this.Position += size;
            return result;
        }

        public bool UnpackArraySize(out int size)
        {
            var format = this.ReadByte();
            if (format == (byte)TypeFormat.Array16)
            {
                size = BitConverter.ToUInt16(this._buffer, this.Position);
                this.Position += sizeof(ushort);
                return true;
            }
            else if (format == (byte)TypeFormat.Array32)
            {
                size = (int)BitConverter.ToUInt32(this._buffer, this.Position);
                this.Position += sizeof(uint);
                return true;
            }

            size = format & 111;
            return true;
        }

        public string ReadString()
        {
            var format = this._buffer[this.Position];
            var length = (int)format;
            if (format < 0xA0 || format > 0xBf)
            {
                if ((TypeFormat)format == TypeFormat.Str8)
                {
                    this.Position++;
                    length = this._buffer[this.Position];
                    this.Position++;
                }
                else if ((TypeFormat)format == TypeFormat.Str16)
                {
                    this.Position++;
                    length = this._buffer[this.Position];
                    this.Position++;
                }
                else if ((TypeFormat)format == TypeFormat.Str32)
                {
                    this.Position++;
                    length = this._buffer[this.Position];
                    this.Position++;
                }
            }
            else
            {
                length = format & 95;
                this.Position++;
            }

            //var length = this.ReadData[this.Position] & 95;
            if (length == 0)
            {
                //this.Position++;
                return string.Empty;
            }

            var result = Encoding.ASCII.GetString(this._buffer, this.Position, length - 1);

            this.Position += length;
            return result;
        }

        public string ReadStr8()
        {
            var pathFormat = (TypeFormat)(byte)this.Read();
            var length = this._buffer[this.Position];
            var result = Encoding.ASCII.GetString(this._buffer, this.Position + 1, length - 1);

            this.Position += length + 1;
            return result;
        }

        public byte ReadByte()
        {
            var result = this._buffer[this.Position];
            this.Position++;
            return result;
        }

        public void Skip(int bytes)
        {
            this.Position += bytes;
        }
    }
}
