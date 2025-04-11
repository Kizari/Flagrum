using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Features.AssetExplorer.Base;

namespace Flagrum.Application.Features.AssetExplorer.Data;

public class FileSystemNode : IAssetExplorerNodeBase
{
    private ICollection<IAssetExplorerNode> _children;

    private IAssetExplorerNode _parent;

    public FileSystemNode() { }

    public FileSystemNode(string path)
    {
        Path = path;
        Name = path.Length == 3 ? $"Local Disk ({path[..2]})" : path.Split('\\').Last();
    }

    public string Name { get; set; }
    public string Path { get; private set; }

    public IAssetExplorerNode Parent
    {
        get
        {
            if (_parent == null)
            {
                var parent = string.IsNullOrEmpty(Path) ? null : Directory.GetParent(Path);
                if (parent == null && Name != "This PC")
                {
                    _parent = CreateRootNode();
                }

                if (parent != null)
                {
                    _parent = new FileSystemNode(parent.FullName);
                }
            }

            return _parent;
        }
        set => _parent = value;
    }

    public bool HasChildren => !File.Exists(Path);

    public ICollection<IAssetExplorerNode> Children
    {
        get
        {
            if ((this as IAssetExplorerNode).Type != ExplorerItemType.Directory)
            {
                return null;
            }

            if (_children == null)
            {
                _children = new List<IAssetExplorerNode>();

                try
                {
                    foreach (var directory in new DirectoryInfo(Path).GetDirectories()
                                 .Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden))
                                 .Select(d => d.FullName)
                                 .OrderBy(d => d))
                    {
                        _children.Add(new FileSystemNode(directory));
                    }

                    foreach (var file in new DirectoryInfo(Path).GetFiles()
                                 .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                                 .Select(f => f.FullName)
                                 .OrderBy(f => f))
                    {
                        var node = (IAssetExplorerNode)new FileSystemNode(file);
                        if (node.Type != ExplorerItemType.Unsupported)
                        {
                            _children.Add(node);
                        }
                    }
                }
                catch
                {
                    // Flagrum can't access this directory, so just ignore it
                }
            }

            return _children;
        }
        set => throw new NotImplementedException();
    }

    public byte[] Data => File.ReadAllBytes(Path);
    public bool IsExpanded { get; set; }
    public string ElementId { get; } = Guid.NewGuid().ToString();

    public string _displayName { get; set; }
    public ExplorerItemType _type { get; set; }
    public string _icon { get; set; }

    public ExplorerItemType GetTypeFromExtension(string extension)
    {
        return extension switch
        {
            "btex" => ExplorerItemType.Texture,
            "gmtl.gfxbin" => ExplorerItemType.Material,
            "gmdl.gfxbin" => ExplorerItemType.Model,
            "gpubin" => ExplorerItemType.Unspecified,
            "exml" or "prefab" => ExplorerItemType.Xml,
            "autoext" => ExplorerItemType.Text,
            "pka" => ExplorerItemType.AnimationPackage,
            "earc" => ExplorerItemType.Archive,
            _ => ExplorerItemType.Unsupported
        };
    }

    public static IAssetExplorerNode CreateRootNode()
    {
        var node = new FileSystemNode
        {
            Name = "This PC",
            Path = ""
        };

        node._children = DriveInfo.GetDrives().Select(drive => new FileSystemNode
        {
            Name = $"Local Disk ({drive.Name[..^1]})",
            _parent = node,
            Path = drive.Name
        }).Cast<IAssetExplorerNode>().ToList();

        return node;
    }
}