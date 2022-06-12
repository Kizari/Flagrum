using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Web.Persistence.Entities;

public class Ps4ArchiveAsset : IEntityTypeConfiguration<Ps4ArchiveAsset>
{
    public int Ps4ArchiveLocationId { get; set; }
    public Ps4ArchiveLocation Ps4ArchiveLocation { get; set; }

    public int Ps4AssetUriId { get; set; }
    public Ps4AssetUri Ps4AssetUri { get; set; }

    public void Configure(EntityTypeBuilder<Ps4ArchiveAsset> builder)
    {
        builder.HasKey(e => new {e.Ps4ArchiveLocationId, e.Ps4AssetUriId});
    }
}