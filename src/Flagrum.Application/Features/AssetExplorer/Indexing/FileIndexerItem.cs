using System.Collections.Generic;
using Flagrum.Core.Archive;

namespace Flagrum.Application.Features.AssetExplorer.Indexing;

/// <summary>
/// Simple class to hold temporary file information for <see cref="FileIndexer"/>.
/// </summary>
public class FileIndexerItem
{
    /// <summary>
    /// The path of the file relative to the game's "datas" directory.
    /// </summary>
    public string FilePath { get; init; }
    
    /// <summary>
    /// A list of all URIs associated with this file.<br/>
    /// <b>Archives</b>: Will contain URIs of all entries within the archive.
    /// <b>Loose files</b>: Will contain only one <c>string</c>, the URI representing the loose file.
    /// </summary>
    public Dictionary<AssetId, string> Uris { get; init; }
}