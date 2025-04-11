namespace Flagrum.Abstractions.AssetExplorer;

public interface IAssetExplorerNode
{
    string Name { get; set; }
    string Path { get; }
    IAssetExplorerNode Parent { get; set; }
    bool HasChildren { get; }
    ICollection<IAssetExplorerNode> Children { get; set; }

    byte[] Data { get; }

    bool IsExpanded { get; set; }

    string ElementId { get; }

    string _displayName { get; set; }

    string DisplayName { get; }

    ExplorerItemType _type { get; set; }

    ExplorerItemType Type { get; }

    string _icon { get; set; }

    string Icon { get; }

    bool HasPropertyView { get; }

    ExplorerItemType GetTypeFromExtension(string extension);

    void Traverse(Action<IAssetExplorerNode> visitor);
    IAssetExplorerNode GetRoot();
    object ToObject();
}