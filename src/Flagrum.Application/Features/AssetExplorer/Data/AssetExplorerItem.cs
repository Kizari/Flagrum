using System.Linq;
using Flagrum.Abstractions.AssetExplorer;

namespace Flagrum.Application.Features.AssetExplorer.Data;

public static class AssetExplorerItem
{
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
            "heb" => ExplorerItemType.TerrainTexture,
            "gmtl" => ExplorerItemType.Material,
            "gmdl" => ExplorerItemType.Model,
            "ebex" or "prefab" => ExplorerItemType.Xml,
            "autoext" => ExplorerItemType.Text,
            "pka" => ExplorerItemType.AnimationPackage,
            _ => ExplorerItemType.Unsupported
        };
    }
}