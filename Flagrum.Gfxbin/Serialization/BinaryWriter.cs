using System;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Services.Logging;
using Flagrum.Gfxbin.Gmdl.Data;

namespace Flagrum.Gfxbin.Serialization
{
    public class BinaryWriter
    {
        private readonly Logger _logger = new ConsoleLogger();
        private readonly MemoryStream _stream = new();

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public void WriteMapCount(uint count)
        {
            if (count <= 0x0F)
            {
                var b = (byte)(count | 0x80);
                _stream.WriteByte(b);
            }
            else if (count <= ushort.MaxValue)
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
            if (size <= 0x0F)
            {
                var b = (byte)(size | 0x90);
                _stream.WriteByte(b);
            }
            else if (size <= ushort.MaxValue)
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

        public void WriteByte(byte b)
        {
            _stream.WriteByte(b);
        }

        public void WriteFloat(float value)
        {
            _stream.WriteByte((byte)TypeFormat.Float32);
            _stream.Write(BitConverter.GetBytes(value));
        }

        public void WriteUInt(ulong number)
        {
            switch (number)
            {
                case <= 0x7F:
                    _stream.WriteByte((byte)number);
                    break;
                case <= byte.MaxValue:
                    _stream.WriteByte((byte)TypeFormat.Uint8);
                    _stream.WriteByte((byte)number);
                    break;
                case <= ushort.MaxValue:
                    _stream.WriteByte((byte)TypeFormat.Uint16);
                    _stream.Write(BitConverter.GetBytes((ushort)number));
                    break;
                case <= uint.MaxValue:
                    _stream.WriteByte((byte)TypeFormat.Uint32);
                    _stream.Write(BitConverter.GetBytes((uint)number));
                    break;
                case <= ulong.MaxValue:
                    _stream.WriteByte((byte)TypeFormat.Uint64);
                    _stream.Write(BitConverter.GetBytes(number));
                    break;
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
                }
                else
                {
                    _stream.WriteByte((byte)TypeFormat.False);
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

            // FIXME: This only works if starting at byte.MaxValue and iterating backwards
            // No idea why it works, but seems to always produce the correct result so far
            var format = byte.MaxValue;
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

        public void WriteVector3(Vector3 vector)
        {
            WriteFloat(vector.X);
            WriteFloat(vector.Y);
            WriteFloat(vector.Z);
        }

        public void WriteMatrix(Matrix matrix)
        {
            foreach (var i in Enumerable.Range(0, 4))
            {
                WriteVector3(matrix.Rows[i]);
            }
        }

        public void WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // NOTE: Unsure if this is safe in all cases
                _stream.WriteByte(0xA0);
                return;
            }

            var stringBytes = Encoding.ASCII.GetBytes(value);
            var stringBuffer = new byte[stringBytes.Length + 1];
            Array.Copy(stringBytes, stringBuffer, stringBytes.Length);

            if (stringBuffer.Length <= 0x7F)
            {
                // FIXME: This only works if starting at byte.MaxValue and iterating backwards
                // No idea why it works, but seems to always produce the correct result so far
                var format = byte.MaxValue;
                while ((format & 95) != stringBuffer.Length)
                {
                    format--;
                }

                WriteByte(format);
            }
            else if (stringBuffer.Length <= byte.MaxValue)
            {
                WriteByte((byte)stringBuffer.Length);
            }
            else if (stringBuffer.Length <= ushort.MaxValue)
            {
                WriteByte((byte)TypeFormat.Str16);
                WriteUInt((ushort)stringBuffer.Length);
            }
            else
            {
                WriteByte((byte)TypeFormat.Str32);
                WriteUInt((uint)stringBuffer.Length);
            }

            _stream.Write(stringBuffer);
        }
    }
}