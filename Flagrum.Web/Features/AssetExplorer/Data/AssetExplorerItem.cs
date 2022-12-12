using System;
using System.Linq;

namespace Flagrum.Web.Features.AssetExplorer.Data;

public static class AssetExplorerItem
{
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
}