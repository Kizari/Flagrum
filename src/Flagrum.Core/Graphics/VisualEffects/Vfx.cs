using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flagrum.Core.Graphics.VisualEffects;

/// <summary>
///     This format isn't yet reversed, so this class is here purely for crudely pulling dependency URIs from the
///     binary data at this stage. This class should be updated when more is known about the format.
/// </summary>
public class Vfx
{
    public static IEnumerable<string> GetDependencies(byte[] vfx)
    {
        var vfxString = Encoding.UTF8.GetString(vfx);
        var matches = Regex.Matches(vfxString, @"(data:\/\/.+?\..+?)" + (char) 0x00);
        return matches.Select(m => m.Groups.Values.ElementAt(1).Value);
    }
}