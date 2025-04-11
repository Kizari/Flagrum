using System.Text;
using Flagrum.Core.Entities.Xml2;

namespace Flagrum.Core.Entities.Xml;

public class Crc64 : IHashing<ulong>
{
    private static readonly Crc64 Instance = new();

    private readonly ulong[] _table;
    private ulong _value;

    public Crc64(ulong key = 0xC96C5795D7870F42UL)
    {
        _table = new ulong[256];
        for (ulong i = 0; i < 256; i++)
        {
            var r = i;
            for (var j = 0; j < 8; j++)
            {
                if ((r & 1) != 0)
                {
                    r = (r >> 1) ^ key;
                }
                else
                {
                    r >>= 1;
                }
            }

            _table[i] = r;
        }

        Init();
    }

    public Crc64(Crc64 crc)
    {
        _table = crc._table;
        Init();
    }

    public void WriteByte(byte b)
    {
        _value = _table[(byte)_value ^ b] ^ (_value >> 8);
    }

    public void Write(byte[] data, uint offset, uint size)
    {
        for (uint i = 0; i < size; i++)
        {
            _value = _table[(byte)_value ^ data[offset + i]] ^ (_value >> 8);
        }
    }

    public ulong GetDigest()
    {
        return _value ^ ulong.MaxValue;
    }

    public void Init()
    {
        _value = ulong.MaxValue;
    }

    public static ulong CalculateDigestAscii(string str)
    {
        var data = Encoding.ASCII.GetBytes(str);
        return CalculateDigest(data, 0, (uint)data.Length);
    }

    public static ulong CalculateDigest(byte[] data, uint offset, uint size)
    {
        Instance.Init();
        Instance.Write(data, offset, size);
        return Instance.GetDigest();
    }
}