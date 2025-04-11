namespace Flagrum.Abstractions.AssetExplorer;

/// <summary>
/// Represents an index of game files.
/// </summary>
public interface IFileIndex
{
    /// <summary>
    /// Root node of the virtual file tree.
    /// </summary>
    IAssetExplorerNode RootNode { get; set; }
    
    /// <summary>
    /// Whether the file indexer is currently generating the file index.
    /// </summary>
    bool IsRegenerating { get; }

    /// <summary>
    /// Whether the file index contains any records.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Callback to execute when the file indexer either starts or finishes indexing game files.
    /// </summary>
    event Action<bool>? OnIsRegeneratingChanged;

    /// <summary>
    /// Check if the file index contains the given URI.
    /// </summary>
    bool Contains(string uri);

    /// <summary>
    /// Gets the relative path of the archive that contains a given asset.
    /// </summary>
    /// <param name="uri">URI of the asset to find the archive for.</param>
    string? GetArchiveRelativePathByUri(string uri);

    /// <summary>
    /// Creates a fresh file index from the directory specified by <see cref="IProfileService.GameDataDirectory" />.
    /// </summary>
    /// <returns>The regenerated file index.</returns>
    void Regenerate();

    /// <summary>
    /// Writes this file index to disk.
    /// </summary>
    /// <param name="path">File path to write the file to.</param>
    void Save(string path);
}