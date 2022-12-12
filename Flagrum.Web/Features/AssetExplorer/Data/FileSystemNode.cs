using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flagrum.Web.Features.AssetExplorer.Data;

public class FileSystemNode : IAssetExplorerNode
{
    private ICollection<IAssetExplorerNode> _children;

    private IAssetExplorerNode _parent;
    private string _path;

    public FileSystemNode() { }

    public FileSystemNode(string path)
    {
        _path = path;
        Name = path.Length == 3 ? $"Local Disk ({path[..2]})" : path.Split('\\').Last();
    }

    public sealed override string Name { get; set; }
    public override string Path => _path;

    public override IAssetExplorerNode Parent
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

    public override bool HasChildren => !File.Exists(Path);

    public override ICollection<IAssetExplorerNode> Children
    {
        get
        {
            if (Type != ExplorerItemType.Directory)
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
                        var node = new FileSystemNode(file);
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
    }

    public override byte[] Data => File.ReadAllBytes(Path);

    protected override ExplorerItemType GetTypeFromExtension(string extension)
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
            _ => ExplorerItemType.Unsupported
        };
    }

    public static IAssetExplorerNode CreateRootNode()
    {
        var node = new FileSystemNode
        {
            Name = "This PC",
            _path = ""
        };

        node._children = DriveInfo.GetDrives().Select(drive => new FileSystemNode
        {
            Name = $"Local Disk ({drive.Name[..^1]})",
            _parent = node,
            _path = drive.Name
        }).Cast<IAssetExplorerNode>().ToList();

        return node;
    }
}