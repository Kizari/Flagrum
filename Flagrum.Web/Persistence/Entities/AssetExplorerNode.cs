using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Utilities;
using Flagrum.Web.Services;

namespace Flagrum.Web.Persistence.Entities;

public class AssetExplorerNode
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int? ParentId { get; set; }
    public AssetExplorerNode Parent { get; set; }

    public ICollection<AssetExplorerNode> Children { get; set; } = new ConcurrentCollection<AssetExplorerNode>();

    public void Traverse(FlagrumDbContext context, Action<AssetExplorerNode> visitor)
    {
        visitor(this);
        foreach (var node in context.AssetExplorerNodes.Where(n => n.ParentId == Id))
        {
            node.Traverse(context, visitor);
        }
    }

    public void TraverseDescending(FlagrumDbContext context, Action<AssetExplorerNode> visitor)
    {
        visitor(this);

        if (ParentId != null)
        {
            Parent = context.AssetExplorerNodes.Find(ParentId);
            Parent?.TraverseDescending(context, visitor);
        }
    }

    public string GetUri(FlagrumDbContext context)
    {
        var uri = "";
        TraverseDescending(context,
            node => uri = node.Name + (uri.Length > 0 && !node.Name.EndsWith('/') ? "/" : "") + uri);
        return uri;
    }

    public string GetLocation(FlagrumDbContext context, SettingsService settings)
    {
        var uri = GetUri(context);
        return settings.GameDataDirectory + "\\" + context.AssetUris
            .Where(a => a.Uri == uri)
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();
    }
}