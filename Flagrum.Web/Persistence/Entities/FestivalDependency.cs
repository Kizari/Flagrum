using System.ComponentModel.DataAnnotations;

namespace Flagrum.Web.Persistence.Entities;

public class FestivalDependency
{
    [Key] public int Id { get; set; }

    public string Uri { get; set; }
}