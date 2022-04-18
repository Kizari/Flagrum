using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Web.Persistence.Entities;

public class ModelReplacementFavourite : IEntityTypeConfiguration<ModelReplacementFavourite>
{
    public int Id { get; set; }
    public bool IsDefault { get; set; }

    public void Configure(EntityTypeBuilder<ModelReplacementFavourite> builder)
    {
        builder.HasKey(e => new {e.Id, e.IsDefault});
    }
}