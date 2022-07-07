using System.Collections.Generic;
using Flagrum.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalDependency
{
    public int Id { get; set; }
    public string Uri { get; set; }

    public ICollection<FestivalDependencyFestivalDependency> Parents { get; set; } =
        new ConcurrentCollection<FestivalDependencyFestivalDependency>();

    public ICollection<FestivalDependencyFestivalDependency> Children { get; set; } =
        new ConcurrentCollection<FestivalDependencyFestivalDependency>();
    
    public ICollection<FestivalDependencyFestivalSubdependency> Subdependencies { get; set; }
}

public class FestivalDependencyFestivalDependency : IEntityTypeConfiguration<FestivalDependencyFestivalDependency>
{
    public int ParentId { get; set; }
    public FestivalDependency Parent { get; set; }
    
    public int ChildId { get; set; }
    public FestivalDependency Child { get; set; }
    
    public void Configure(EntityTypeBuilder<FestivalDependencyFestivalDependency> builder)
    {
        builder.HasKey(e => new {e.ParentId, e.ChildId});
        builder.HasOne(e => e.Child).WithMany(e => e.Children);
        builder.HasOne(e => e.Parent).WithMany(e => e.Parents);
    }
}