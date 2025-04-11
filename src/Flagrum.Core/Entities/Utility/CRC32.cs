using System.Text;

namespace Flagrum.Core.Scripting.Ebex.Utility;

public static class CRC32
{
    public static uint Hash(string str)
    {
        var num1 = 24;
        var num2 = 2147483648;
        uint num3 = 79764919;
        var num4 = uint.MaxValue;
        foreach (var num5 in Encoding.UTF8.GetBytes(str))
        {
            num4 ^= (uint)num5 << num1;
            for (uint index = 8; index > 0U; --index)
            {
                if (((int)num4 & (int)num2) != 0)
                {
                    num4 = (num4 << 1) ^ num3;
                }
                else
                {
                    num4 <<= 1;
                }
            }
        }

        return ~num4;
    }
}