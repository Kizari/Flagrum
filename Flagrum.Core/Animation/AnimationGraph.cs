using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flagrum.Core.Animation;

public class AnimationGraph
{
    public static IEnumerable<string> GetDependencies(byte[] anmgph)
    {
        var anmgphString = Encoding.UTF8.GetString(anmgph);
        //var matches = Regex.Matches(vfxString, @"(data:\/\/.+?\..+?) ");
        //return matches.SelectMany(m => m.Groups.Values.Select(g => g.Value));
        var matches = Regex.Matches(anmgphString, @"data:\/\/.+?\..+?" + (char)0x00);
        foreach (Match match in matches)
        {
            yield return match.Value[..^1];
        }
    }
}