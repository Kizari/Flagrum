using System.Linq;
using System.Text;

namespace Flagrum.Core.Scripting.Ebex.Utility;

public static class Fnv1A32
{
    private const uint _Prime = 16777619;
    private const uint _Seed = 2166136261;

    public static uint Hash(string str, uint? prev = null)
    {
        return Hash(Encoding.UTF8.GetBytes(str), prev);
    }

    public static uint Hash(byte[] bytes, uint? prev = null)
    {
        var seed = (uint)((int?)prev ?? -2128831035);
        return bytes.Aggregate(seed, (current, b) => (uint)(((int)current ^ b) * 16777619));
    }
}