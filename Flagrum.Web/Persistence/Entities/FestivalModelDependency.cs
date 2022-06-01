using System.ComponentModel.DataAnnotations;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalModelDependency
{
    [Key] public int Id { get; set; }

    public string Uri { get; set; }
}