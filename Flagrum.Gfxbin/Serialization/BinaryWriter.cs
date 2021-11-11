using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Flagrum.Gfxbin.Serialization
{
    public class BinaryWriter
    {
        private MemoryStream _stream = new MemoryStream();

        public byte[] ToArray() => _stream.ToArray();

        public void WriteMapCount(uint count)
        {
            if (count <= ushort.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Map16);
                _stream.Write(BitConverter.GetBytes((ushort)count));
            }
            else
            {
                _stream.WriteByte((byte)TypeFormat.Map32);
                _stream.Write(BitConverter.GetBytes(count));
            }
        }

        public void WriteArraySize(uint size)
        {
            if (size <= ushort.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Array16);
                _stream.Write(BitConverter.GetBytes((ushort)size));
            }
            else
            {
                _stream.WriteByte((byte)TypeFormat.Array32);
                _stream.Write(BitConverter.GetBytes(size));
            }
        }

        public void WriteFloat(float value)
        {
            _stream.WriteByte((byte)TypeFormat.Float32);
            _stream.Write(BitConverter.GetBytes(value));
        }

        public void WriteInt(ulong number)
        {
            if (number <= 0x7F)
            {
                _stream.WriteByte((byte)number);
            }
            else if (number <= byte.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Uint8);
                _stream.WriteByte((byte)number);
            }
            else if (number <= ushort.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Uint16);
                _stream.Write(BitConverter.GetBytes((ushort)number));
            }
            else if (number <= uint.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Uint32);
                _stream.Write(BitConverter.GetBytes((uint)number));
            }
            else if (number <= ulong.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Uint64);
                _stream.Write(BitConverter.GetBytes((ulong)number));
            }
        }

        public void WriteSByte(sbyte value)
        {
            _stream.WriteByte((byte)value);
        }

        public void WriteBin(byte[] bin)
        {
            if (bin.Length > ushort.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Bin32);
                _stream.Write(BitConverter.GetBytes((uint)bin.Length));
            }
            else if (bin.Length > byte.MaxValue)
            {
                _stream.WriteByte((byte)TypeFormat.Bin16);
                _stream.Write(BitConverter.GetBytes((ushort)bin.Length));
            }
            else
            {
                _stream.WriteByte((byte)bin.Length);
            }

            _stream.Write(bin);
        }

        public void Write(object data)
        {
            if (data is ulong ul)
            {
                _stream.WriteByte((byte)TypeFormat.Uint64);
                _stream.Write(BitConverter.GetBytes(ul));
            }
            else if (data is uint ui)
            {
                _stream.WriteByte((byte)TypeFormat.Uint32);
                _stream.Write(BitConverter.GetBytes(ui));
            }
            else if (data is ushort us)
            {
                _stream.WriteByte((byte)TypeFormat.Uint16);
                _stream.Write(BitConverter.GetBytes(us));
            }
            else if (data is byte by)
            {
                _stream.WriteByte((byte)TypeFormat.Uint8);
                _stream.Write(BitConverter.GetBytes(by));
            }
            else if (data is float f)
            {
                _stream.WriteByte((byte)TypeFormat.Float32);
                _stream.Write(BitConverter.GetBytes(f));
            }
            else if (data is bool b)
            {
                if (b)
                {
                    _stream.WriteByte((byte)TypeFormat.True);
                    _stream.Write(BitConverter.GetBytes(b));
                }
                else
                {
                    _stream.WriteByte((byte)TypeFormat.False);
                    _stream.Write(BitConverter.GetBytes(b));
                }
            }
            else
            {
                _stream.WriteByte((byte)TypeFormat.Uint8);
                _stream.Write(BitConverter.GetBytes((byte)data));
            }
        }

        public void WriteStringX(string value)
        {
            var byteList = Encoding.ASCII.GetBytes(value).ToList();
            byteList.Add(0x00);
            var bytes = byteList.ToArray();

            // This only works if starting at byte.MaxValue and iterating backwards
            // No idea why it works, but seems to always produce the correct result
            byte format = byte.MaxValue;
            while ((format & 95) != bytes.Length)
            {
                format--;
            }

            _stream.WriteByte(format);
            _stream.Write(bytes);
        }

        public void WriteString8(string value)
        {
            var byteList = Encoding.ASCII.GetBytes(value).ToList();
            byteList.Add(0x00);
            var bytes = byteList.ToArray();
            _stream.WriteByte((byte)TypeFormat.Str8);
            _stream.WriteByte((byte)bytes.Length);
            _stream.Write(bytes);
        }
    }
}
