using System.Collections.Generic;
using System.Text;

namespace Flagrum.Gfxbin.Serialization
{
    public class StringBuffer
    {
        private List<byte> _buffer = new();
        private Dictionary<string, int> _map = new();
        private int _offset;

        public ulong Put(string value)
        {
            if (_map.TryGetValue(value, out int offset))
            {
                return (ulong)offset;
            }

            _map.Add(value, _offset);

            var bytes = Encoding.UTF8.GetBytes(value);
            _buffer.AddRange(bytes);
            _buffer.Add(0x00);

            var returnValue = _offset;
            _offset += value.Length + 1;
            return (ulong)returnValue;
        }

        public byte[] ToArray() => _buffer.ToArray();
    }
}
