using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Web.Features.AssetExplorer.Data;

public abstract class IAssetExplorerNode
{
    private string _displayName;

    private string _icon;

    private ExplorerItemType _type;
    public abstract string Name { get; set; }
    public abstract string Path { get; }
    public abstract IAssetExplorerNode Parent { get; set; }
    public abstract bool HasChildren { get; }
    public abstract ICollection<IAssetExplorerNode> Children { get; }

    public abstract byte[] Data { get; }

    [NotMapped] public bool IsExpanded { get; set; }

    [NotMapped] public string ElementId { get; } = Guid.NewGuid().ToString();

    public string DisplayName
    {
        get
        {
            if (_displayName == null)
            {
                if (Type == ExplorerItemType.Directory || Name.StartsWith("CRAF"))
                {
                    _displayName = Name;
                }
                else
                {
                    var extension = Name[Name.LastIndexOf('.')..].ToLower();
                    var trueExtension = extension switch
                    {
                        ".tif" or ".tga" or ".png" or ".dds" or ".exr" => ".btex",
                        ".gmtl" => ".gmtl.gfxbin",
                        ".gmdl" => ".gmdl.gfxbin",
                        ".prefab" or ".ebex" => ".exml",
                        ".autoext" => ".txt",
                        _ => extension
                    };

                    return Name[..Name.LastIndexOf('.')] + trueExtension;
                }
            }

            return _displayName;
        }
    }

    public ExplorerItemType Type
    {
        get
        {
            if (_type == ExplorerItemType.Unspecified)
            {
                // var fileName = Path.Split('/', '\\').Last();
                // if (!fileName.Contains('.') || fileName.StartsWith('.'))
                // {
                //     return ExplorerItemType.Directory;
                // }

                if (HasChildren)
                {
                    return ExplorerItemType.Directory;
                }

                var fileName = Path.Split('/', '\\').Last();
                var tokens = fileName.Split('.');
                var extension = tokens[^1];
                if (extension == "gfxbin")
                {
                    extension = tokens[^2] + "." + extension;
                }

                _type = GetTypeFromExtension(extension);
            }

            return _type;
        }
    }

    public string Icon => _icon ??= Type switch
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

    protected abstract ExplorerItemType GetTypeFromExtension(string extension);

    public void Traverse(Action<IAssetExplorerNode> visitor)
    {
        visitor(this);

        if (Children != null)
        {
            foreach (var child in Children)
            {
                child.Traverse(visitor);
            }
        }
    }

    public IAssetExplorerNode GetRoot()
    {
        return Parent == null ? this : Parent.GetRoot();
    }

    public void SetChildrenDirectoriesOnly()
    {
        if (this is AssetExplorerNode node)
        {
            node.ChildNodes = node.Children.Where(n => n.HasChildren).Cast<AssetExplorerNode>().ToList();
        }
    }
}