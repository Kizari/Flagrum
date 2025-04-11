using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Application.Persistence.Entities;

public class ModelReplacementFavouriteEntity : IEntityTypeConfiguration<ModelReplacementFavouriteEntity>
{
    public int Id { get; set; }
    public bool IsDefault { get; set; }

    public void Configure(EntityTypeBuilder<ModelReplacementFavouriteEntity> builder)
    {
        builder.HasKey(e => new {e.Id, e.IsDefault});
    }
}