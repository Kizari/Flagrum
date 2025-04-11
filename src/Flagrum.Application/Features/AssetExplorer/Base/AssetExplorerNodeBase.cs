using System;
using System.Linq;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Core.Animation.Package;
using Flagrum.Core.Archive;
using Flagrum.Core.Graphics.Materials;
using Flagrum.Core.Graphics.Models;
using Flagrum.Core.Graphics.Terrain;
using Flagrum.Core.Graphics.Textures.Luminous;

namespace Flagrum.Application.Features.AssetExplorer.Base;

public interface IAssetExplorerNodeBase : IAssetExplorerNode
{
    string IAssetExplorerNode.DisplayName
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
                    _displayName = UriHelper.Instance.ReplaceFileNameExtensionWithTrueExtension(Name);
                }
            }

            return _displayName;
        }
    }

    ExplorerItemType IAssetExplorerNode.Type
    {
        get
        {
            if (_type == ExplorerItemType.Unspecified)
            {
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

    string IAssetExplorerNode.Icon => _icon ??= Type switch
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

    bool IAssetExplorerNode.HasPropertyView => Type is ExplorerItemType.Material or ExplorerItemType.Texture
        or ExplorerItemType.Model or ExplorerItemType.AnimationPackage or ExplorerItemType.TerrainTexture;

    void IAssetExplorerNode.Traverse(Action<IAssetExplorerNode> visitor)
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

    IAssetExplorerNode IAssetExplorerNode.GetRoot() => Parent == null ? this : Parent.GetRoot();

    object IAssetExplorerNode.ToObject() => Type switch
    {
        ExplorerItemType.Material => GameMaterial.Deserialize(Data),
        ExplorerItemType.Texture => BlackTexture.Deserialize(Data),
        ExplorerItemType.Model => GameModel.Deserialize(Data),
        ExplorerItemType.AnimationPackage => AnimationPackage.FromData(Data),
        ExplorerItemType.TerrainTexture => HeightEntityBinary.FromData(Data),
        _ => null
    };
}