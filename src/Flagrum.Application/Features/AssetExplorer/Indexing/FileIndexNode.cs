using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Features.AssetExplorer.Base;
using Flagrum.Application.Services;
using MemoryPack;

namespace Flagrum.Application.Features.AssetExplorer.Indexing;

[MemoryPackable(GenerateType.CircularReference, SerializeLayout.Sequential)]
public partial class FileIndexNode : IAssetExplorerNodeBase
{
    [MemoryPackIgnore] private string _path;
    public FileIndexNode ParentNode { get; set; }
    public List<FileIndexNode> ChildNodes { get; set; }

    public static AppStateService AppState { get; set; }

    public string Name { get; set; }

    [MemoryPackIgnore]
    public IAssetExplorerNode Parent
    {
        get => ParentNode;
        set => ParentNode = (FileIndexNode)value;
    }

    [MemoryPackIgnore]
    public ICollection<IAssetExplorerNode> Children
    {
        get => ChildNodes.Cast<IAssetExplorerNode>().ToList();
        set => ChildNodes = value.Cast<FileIndexNode>().ToList();
    }

    [MemoryPackIgnore] public string Path => _path ??= GetUri();
    [MemoryPackIgnore] public bool HasChildren => Children.Count > 0;
    [MemoryPackIgnore] public byte[] Data => AppState.GetFileByUri(Path);

    [MemoryPackIgnore] public bool IsExpanded { get; set; }
    [MemoryPackIgnore] public string ElementId { get; } = Guid.NewGuid().ToString();

    [MemoryPackIgnore] public string _displayName { get; set; }
    [MemoryPackIgnore] public ExplorerItemType _type { get; set; }
    [MemoryPackIgnore] public string _icon { get; set; }

    public ExplorerItemType GetTypeFromExtension(string extension)
    {
        return extension switch
        {
            "btex" or "dds" or "png" or "tif" or "tga" or "exr" => ExplorerItemType.Texture,
            "gmtl" => ExplorerItemType.Material,
            "gmdl" => ExplorerItemType.Model,
            "ebex" or "prefab" => ExplorerItemType.Xml,
            "autoext" => ExplorerItemType.Text,
            "pka" => ExplorerItemType.AnimationPackage,
            "heb" => ExplorerItemType.TerrainTexture,
            _ => ExplorerItemType.Unsupported
        };
    }

    private string GetUri()
    {
        var uri = "";

        for (IAssetExplorerNode node = this; node != null; node = node.Parent)
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
        }

        return uri;
    }
}