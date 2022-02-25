using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Web.Persistence.Entities;

public class ModelReplacementPath : IEntityTypeConfiguration<ModelReplacementPath>
{
    public int ModelReplacementPresetId { get; set; }
    public string Path { get; set; }

    public void Configure(EntityTypeBuilder<ModelReplacementPath> builder)
    {
        builder.HasKey(e => new {e.ModelReplacementPresetId, e.Path});
    }
}