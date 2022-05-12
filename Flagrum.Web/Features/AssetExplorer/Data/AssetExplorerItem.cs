using System;
using System.IO;
using System.Linq;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Web.Features.AssetExplorer.Data;

public class AssetExplorerItem
{
    public string Name { get; set; }
    public ExplorerItemType Type { get; set; }
    public Func<byte[]> Data { get; set; }
    public string Uri { get; set; }

    public static AssetExplorerItem FromNode(AssetExplorerNode node, FlagrumDbContext context)
    {
        var uri = node.GetUri(context);
        return new AssetExplorerItem
        {
            Name = node.Name,
            Uri = uri,
            Data = () => context.GetFileByUri(uri),
            Type = context.AssetExplorerNodes.Any(n => n.ParentId == node.Id)
                ? ExplorerItemType.Directory
                : GetType(uri)
        };
    }

    public static AssetExplorerItem FromExplorerItem(ExplorerItem item)
    {
        return new AssetExplorerItem
        {
            Name = item.Name,
            Type = item.Type,
            Data = () => File.ReadAllBytes(item.Path)
        };
    }

    public static string GetDisplayName(string name, ExplorerItemType type)
    {
        if (type == ExplorerItemType.Directory || name.StartsWith("CRAF"))
        {
            return name;
        }

        var extension = name[name.LastIndexOf('.')..].ToLower();
        var trueExtension = extension switch
        {
            ".tif" or ".tga" or ".png" or ".dds" or ".exr" => ".btex",
            ".gmtl" => ".gmtl.gfxbin",
            ".gmdl" => ".gmdl.gfxbin",
            ".prefab" or ".ebex" => ".exml",
            ".autoext" => ".txt",
            _ => extension
        };

        return name[..name.LastIndexOf('.')] + trueExtension;
    }

    public static ExplorerItemType GetType(string uri)
    {
        var fileName = uri.Split('/').Last();
        if (!fileName.Contains('.'))
        {
            return ExplorerItemType.Directory;
        }

        var extension = fileName.Split('.').Last();
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

    public static string GetIcon(string uri)
    {
        return GetType(uri) switch
        {
            ExplorerItemType.Directory => "folder",
            ExplorerItemType.Material => "blur_circular",
            ExplorerItemType.Texture => "gradient",
            ExplorerItemType.Model => "view_in_ar",
            ExplorerItemType.Xml => "account_tree",
            ExplorerItemType.Text => "article",
            ExplorerItemType.AnimationPackage => "widgets",
            _ => "insert_drive_file"
        };
    }
}