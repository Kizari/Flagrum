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
    public static ProfileService Profile;

    private FlagrumDbContext _context;

    private string _path;

    private FlagrumDbContext Context => _context ??= new FlagrumDbContext(Profile);
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
    public override byte[] Data => Context.GetFileByUri(Path);

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
                Parent = Context.AssetExplorerNodes.AsNoTracking().First(n => n.Id == ParentId);
            }
        }

        ((AssetExplorerNode)Parent)?.TraverseDescending(visitor);
    }

    public string GetUri()
    {
        var uri = "";

        TraverseDescending(node =>
        {
            if (node.Name != "")
            {
                var currentSection = node.Name;

                if (uri.Length > 0)
                {
                    currentSection += node.Name.EndsWith(":") ? "//" : "/";
                }

                uri = currentSection + uri;
            }
        });

        return uri;
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