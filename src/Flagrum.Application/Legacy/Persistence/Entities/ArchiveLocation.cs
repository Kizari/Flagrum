using System.Collections.Generic;

namespace Flagrum.Application.Persistence.Entities;

public class ArchiveLocation
{
    public int Id { get; set; }
    public string Path { get; set; }
    
    public ICollection<AssetUri> AssetUris { get; set; }
}