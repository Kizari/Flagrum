using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Application.Persistence.Entities;

public class ModelReplacementPathEntity : IEntityTypeConfiguration<ModelReplacementPathEntity>
{
    public int ModelReplacementPresetId { get; set; }
    public string Path { get; set; }

    public void Configure(EntityTypeBuilder<ModelReplacementPathEntity> builder)
    {
        builder.HasKey(e => new {e.ModelReplacementPresetId, e.Path});
        
        builder.HasOne<ModelReplacementPresetEntity>()
            .WithMany(e => e.ReplacementPaths)
            .HasForeignKey(e => e.ModelReplacementPresetId);
    }
}