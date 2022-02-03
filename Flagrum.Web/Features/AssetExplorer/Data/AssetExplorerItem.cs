using System;
using System.IO;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;

namespace Flagrum.Web.Features.AssetExplorer.Data;

public class AssetExplorerItem
{
    public string Name { get; set; }
    public ExplorerItemType Type { get; set; }
    public Func<byte[]> Data { get; set; }

    public string DisplayName
    {
        get
        {
            if (Type == ExplorerItemType.Directory || Name.StartsWith("CRAF"))
            {
                return Name;
            }

            var extension = Name[Name.LastIndexOf('.')..].ToLower();
            var trueExtension = extension switch
            {
                ".tif" or ".tga" or ".png" or ".dds" or ".exr" => ".btex",
                ".gmtl" => ".gmtl.gfxbin",
                ".gmdl" => ".gmdl.gfxbin",
                _ => extension
            };

            return Name[..Name.LastIndexOf('.')] + trueExtension;
        }
    }

    public static AssetExplorerItem FromNode(AssetExplorerNode node, FlagrumDbContext context)
    {
        var uri = node.GetUri(context);
        return new AssetExplorerItem
        {
            Name = node.Name,
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

    public static ExplorerItemType GetType(string uri)
    {
        var extension = uri.Split('/').Last().Split('.').Last();
        return extension switch
        {
            "btex" or "dds" or "png" or "tif" or "tga" or "exr" => ExplorerItemType.Texture,
            "gmtl" => ExplorerItemType.Material,
            "gmdl" => ExplorerItemType.Model,
            _ => ExplorerItemType.Unsupported
        };
    }
}