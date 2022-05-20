using System.Collections.Generic;

namespace Flagrum.Web.Features.EarcMods.Data;

public class EarcModMetadata
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public Dictionary<string, IEnumerable<string>> Replacements { get; set; }
}

public class EarcConflictString
{
    public string Value { get; set; }
}