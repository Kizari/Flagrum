using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Flagrum.Core.Utilities;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalDependency
{
    [Key] public int Id { get; set; }

    public int? ParentId { get; set; }
    public FestivalDependency Parent { get; set; }

    public string Uri { get; set; }

    public ICollection<FestivalDependency> Children { get; set; } = new ConcurrentCollection<FestivalDependency>();
    public ICollection<FestivalSubdependency> Subdependencies { get; set; }
}