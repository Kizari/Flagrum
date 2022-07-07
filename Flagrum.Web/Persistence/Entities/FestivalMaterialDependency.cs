using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalMaterialDependency
{
    public int Id { get; set; }
    public string Uri { get; set; }
    
    public ICollection<FestivalModelDependencyFestivalMaterialDependency> Parents { get; set; }
}

public class FestivalModelDependencyFestivalMaterialDependency : IEntityTypeConfiguration<FestivalModelDependencyFestivalMaterialDependency>
{
    public int ModelDependencyId { get; set; }
    public FestivalModelDependency ModelDependency { get; set; }
    
    public int MaterialDependencyId { get; set; }
    public FestivalMaterialDependency MaterialDependency { get; set; }
    
    public void Configure(EntityTypeBuilder<FestivalModelDependencyFestivalMaterialDependency> builder)
    {
        builder.HasKey(e => new {e.ModelDependencyId, e.MaterialDependencyId});
    }
}