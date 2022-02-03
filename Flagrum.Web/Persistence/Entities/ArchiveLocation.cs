using System.Collections.Generic;

namespace Flagrum.Web.Persistence.Entities;

public class ArchiveLocation
{
    public int Id { get; set; }
    public string Path { get; set; }
    
    public ICollection<AssetUri> AssetUris { get; set; }
}