using System.ComponentModel.DataAnnotations;

namespace Flagrum.Application.Persistence.Entities;

public class AssetUri
{
    [Key] public string Uri { get; set; }

    public int ArchiveLocationId { get; set; }
    public ArchiveLocation ArchiveLocation { get; set; }
}