using System.Collections.Generic;

namespace Flagrum.Web.Persistence.Entities;

public class Ps4AssetUri
{
    public int Id { get; set; }
    public string Uri { get; set; }

    public ICollection<Ps4ArchiveAsset> ArchiveAssets { get; set; }
}