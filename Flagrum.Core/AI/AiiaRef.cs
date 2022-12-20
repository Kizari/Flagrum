using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flagrum.Core.AI;

public class AiiaRef
{
    public static IEnumerable<string> GetDependencies(byte[] aiiaRef)
    {
        var aiiaRefString = Encoding.UTF8.GetString(aiiaRef);
        var matches = Regex.Matches(aiiaRefString, @"(data:\/\/.+?\..+?)");
        return matches.Select(m => m.Groups.Values.ElementAt(1).Value);
    }
}