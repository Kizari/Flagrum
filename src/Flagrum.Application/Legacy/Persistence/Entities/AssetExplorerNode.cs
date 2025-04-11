using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flagrum.Application.Persistence.Entities;

public class AssetExplorerNode
{
    public int Id { get; set; }
    public string Name { get; set; }

    [ForeignKey(nameof(ParentNode))] public int? ParentId { get; set; }

    public AssetExplorerNode ParentNode { get; set; }
    public ICollection<AssetExplorerNode> ChildNodes { get; set; } = new List<AssetExplorerNode>();
}