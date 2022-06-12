using System.Collections.Generic;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalSubdependency
{
    public int Id { get; set; }

    public int ParentId { get; set; }
    public FestivalDependency Parent { get; set; }

    public string Uri { get; set; }

    public ICollection<FestivalModelDependency> Children { get; set; }
}