using System.Collections.Generic;

namespace Flagrum.Web.Persistence.Entities;

public class Ps4ArchiveLocation
{
    public int Id { get; set; }
    public string Path { get; set; }

    public ICollection<Ps4ArchiveAsset> ArchiveAssets { get; set; }
}