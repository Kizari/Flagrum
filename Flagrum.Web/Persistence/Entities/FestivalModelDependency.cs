using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalModelDependency
{
    [Key] public int Id { get; set; }

    public int ParentId { get; set; }
    public FestivalSubdependency Parent { get; set; }

    public string Uri { get; set; }

    public ICollection<FestivalMaterialDependency> Children { get; set; }
}