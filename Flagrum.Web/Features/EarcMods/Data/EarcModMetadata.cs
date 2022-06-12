using System.Collections.Generic;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Web.Features.EarcMods.Data;

public class EarcModMetadata
{
    public int Version { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    
    public Dictionary<string, IEnumerable<EarcModMetadataChange>> Changes { get; set; }

    // Only used for handling version 0 mods
    public Dictionary<string, IEnumerable<string>> Replacements { get; set; }
}

public class EarcModMetadataChange
{
    public EarcChangeType Type { get; set; }
    public string Uri { get; set; }
}

public class EarcConflictString
{
    public string Value { get; set; }
}