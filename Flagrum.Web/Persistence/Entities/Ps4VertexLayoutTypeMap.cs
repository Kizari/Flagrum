using System.ComponentModel.DataAnnotations;
using Flagrum.Core.Gfxbin.Gmdl.Components;

namespace Flagrum.Web.Persistence.Entities;

public class Ps4VertexLayoutTypeMap
{
    [Key] public string Uri { get; set; }
    
    public VertexLayoutType VertexLayoutType { get; set; }
}