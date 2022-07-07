using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalModelDependency
{
    public int Id { get; set; }
    public string Uri { get; set; }
    public VertexLayoutType VertexLayoutType { get; set; }

    public ICollection<FestivalSubdependencyFestivalModelDependency> Parents { get; set; }
    public ICollection<FestivalModelDependencyFestivalMaterialDependency> Children { get; set; }
}

public class FestivalSubdependencyFestivalModelDependency : IEntityTypeConfiguration<FestivalSubdependencyFestivalModelDependency>
{
    public int SubdependencyId { get; set; }
    public FestivalSubdependency Subdependency { get; set; }
    
    public int ModelDependencyId { get; set; }
    public FestivalModelDependency ModelDependency { get; set; }
    
    public void Configure(EntityTypeBuilder<FestivalSubdependencyFestivalModelDependency> builder)
    {
        builder.HasKey(e => new {e.SubdependencyId, e.ModelDependencyId});
    }
}