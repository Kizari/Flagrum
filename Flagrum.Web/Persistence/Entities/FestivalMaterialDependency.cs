using System.ComponentModel.DataAnnotations;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalMaterialDependency
{
    [Key] public int Id { get; set; }

    public int ParentId { get; set; }
    public FestivalModelDependency Parent { get; set; }

    public string Uri { get; set; }
}