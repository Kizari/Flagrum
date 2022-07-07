using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flagrum.Core.Vfx;

public class Vfx
{
    public static IEnumerable<string> GetDependencies(byte[] vfx)
    {
        var vfxString = Encoding.UTF8.GetString(vfx);
        var matches = Regex.Matches(vfxString, @"(data:\/\/.+?\..+?) ");
        return matches.SelectMany(m => m.Groups.Values.Select(g => g.Value));
    }
}