using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalSubdependency
{
    public int Id { get; set; }
    public string Uri { get; set; }

    public ICollection<FestivalDependencyFestivalSubdependency> Parents { get; set; }
    public ICollection<FestivalSubdependencyFestivalModelDependency> Children { get; set; }
}

public class FestivalDependencyFestivalSubdependency : IEntityTypeConfiguration<FestivalDependencyFestivalSubdependency>
{
    public int DependencyId { get; set; }
    public FestivalDependency Dependency { get; set; }
    
    public int SubdependencyId { get; set; }
    public FestivalSubdependency Subdependency { get; set; }
    
    public void Configure(EntityTypeBuilder<FestivalDependencyFestivalSubdependency> builder)
    {
        builder.HasKey(e => new {e.DependencyId, e.SubdependencyId});
    }
}