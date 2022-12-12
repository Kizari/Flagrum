using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Persistence.Entities;

public class AssetExplorerNode : IAssetExplorerNode
{
    private string _path;
    public int Id { get; set; }

    public override string Name { get; set; }

    [ForeignKey(nameof(ParentNode))] public int? ParentId { get; set; }

    public AssetExplorerNode ParentNode { get; set; }

    [NotMapped]
    public override IAssetExplorerNode Parent
    {
        get => ParentNode;
        set => ParentNode = (AssetExplorerNode)value;
    }

    public ICollection<AssetExplorerNode> ChildNodes { get; set; } = new List<AssetExplorerNode>();

    [NotMapped]
    public override ICollection<IAssetExplorerNode> Children => ChildNodes.Cast<IAssetExplorerNode>().ToList();

    public override string Path => _path ??= GetUri();

    public override bool HasChildren => ChildNodes.Count > 0;

    //using var context = new FlagrumDbContext(new SettingsService());
    //return context.AssetExplorerNodes.Any(n => n.ParentId == Id);
    public override byte[] Data
    {
        get
        {
            using var context = new FlagrumDbContext(new SettingsService());
            return context.GetFileByUri(Path);
        }
    }

    public void Traverse(FlagrumDbContext context, Action<AssetExplorerNode> visitor)
    {
        visitor(this);
        foreach (var node in context.AssetExplorerNodes.Where(n => n.ParentId == Id).AsNoTracking().ToList())
        {
            node.Traverse(context, visitor);
        }
    }

    public void TraverseDescending(Action<AssetExplorerNode> visitor)
    {
        visitor(this);

        if (Parent == null)
        {
            if (ParentId != null)
            {
                using var context = new FlagrumDbContext(new SettingsService());
                Parent = context.AssetExplorerNodes.AsNoTracking().First(n => n.Id == ParentId);
            }
        }

        ((AssetExplorerNode)Parent)?.TraverseDescending(visitor);
    }

    public string GetUri()
    {
        var uri = "";
        TraverseDescending(node => uri = node.Name + (uri.Length > 0 && !node.Name.EndsWith('/') ? "/" : "") + uri);
        return uri;
    }

    public string GetLocation()
    {
        using var context = new FlagrumDbContext(new SettingsService());
        return context.Settings.GameDataDirectory + "\\" + context.AssetUris
            .Where(a => a.Uri == Path)
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();
    }

    protected override ExplorerItemType GetTypeFromExtension(string extension)
    {
        return extension switch
        {
            "btex" or "dds" or "png" or "tif" or "tga" or "exr" => ExplorerItemType.Texture,
            "gmtl" => ExplorerItemType.Material,
            "gmdl" => ExplorerItemType.Model,
            "ebex" or "prefab" => ExplorerItemType.Xml,
            "autoext" => ExplorerItemType.Text,
            "pka" => ExplorerItemType.AnimationPackage,
            _ => ExplorerItemType.Unsupported
        };
    }
}