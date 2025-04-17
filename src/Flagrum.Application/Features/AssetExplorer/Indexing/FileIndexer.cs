using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Abstractions.Archive;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Application.Features.AssetExplorer.Indexing;

public partial class FileIndex
{
    /// <summary>
    /// File extensions of loose game files (those not contained in archives) that are allowed to be indexed.
    /// </summary>
    private readonly HashSet<string> _allowedLooseFileExtensions =
    [
        ".bk2",
        ".heb",
        ".hephysx",
        ".pfp",
        ".mab",
        ".sab"
    ];
    
    /// <inheritdoc />
    public void Regenerate()
    {
        IsRegenerating = true;
        OnIsRegeneratingChanged?.Invoke(IsRegenerating);

        // Index all earcs in the game data directory
        var (packedFiles, looseFiles) = IndexFiles(_profile.GameDataDirectory);

        // Reset the file index
        RootNode = new FileIndexNode {Name = "", ChildNodes = []};
        Archives = [];
        Files = [];

        // Map the uris to their respective archives
        var leftovers = new Dictionary<AssetId, (string, FileIndexArchive)>();

        foreach (var item in packedFiles)
        {
            var archive = new FileIndexArchive
            {
                RelativePath = item.FilePath,
                Files = []
            };

            foreach (var (hash, uri) in item.Uris)
            {
                if (!Files.ContainsKey(hash))
                {
                    if (IsUriSimilarToEarcPath(item.FilePath, uri))
                    {
                        var file = new FileIndexFile
                        {
                            Archive = archive,
                            Uri = uri
                        };

                        Files.Add(hash, file);
                        archive.Files.Add(file);
                    }
                    else
                    {
                        leftovers.TryAdd(hash, (uri, archive));
                    }
                }
            }

            Archives.Add(Cryptography.Hash64(archive.RelativePath), archive);
        }

        // Add any leftovers that didn't make it into the index
        foreach (var (hash, (uri, archive)) in leftovers)
        {
            if (!Files.ContainsKey(hash))
            {
                var file = new FileIndexFile
                {
                    Archive = archive,
                    Uri = uri
                };

                Files.Add(hash, file);
                archive.Files.Add(file);
            }
        }

        // Remove any empty archives
        Archives.RemoveWhere(a => a.Value.Files.Count == 0);

        // Merge in loose files whose URI representations aren't already present in the file index
        foreach (var (id, uri) in looseFiles
                     .Where(f => !Files.ContainsKey(f.Uris.First().Key))
                     .Select(file => file.Uris.First()))
        {
            Files.Add(id, new FileIndexFile
            {
                Uri = uri,
                Archive = null
            });
        }

        // Build the node tree
        var nodeData = new Dictionary<string, FileIndexNode>();
        foreach (var (_, file) in Files)
        {
            var currentParent = (FileIndexNode)RootNode;
            var tokens = file.Uri.Replace("://", ":/").Split('/');
            var builder = new StringBuilder();
            for (var i = 0; i < tokens.Length; i++)
            {
                // If this is the last token, no need to check the dictionary
                if (i == tokens.Length - 1)
                {
                    var newNode = new FileIndexNode
                    {
                        ParentNode = currentParent,
                        ChildNodes = [],
                        Name = tokens[i]
                    };

                    currentParent.ChildNodes.Add(newNode);
                }
                else
                {
                    // Update the key
                    builder.Append(tokens[i]);
                    builder.Append('/');
                    var key = builder.ToString();

                    // Check for matching node
                    if (!nodeData.TryGetValue(key, out var node))
                    {
                        node = new FileIndexNode
                        {
                            ParentNode = currentParent,
                            ChildNodes = [],
                            Name = tokens[i]
                        };

                        currentParent.ChildNodes.Add(node);
                        nodeData[key] = node;
                    }

                    currentParent = node;
                }
            }
        }

        // Sort the nodes and save the file index to disk
        SortRecursively((FileIndexNode)RootNode);
        Save(_profile.FileIndexPath);

        IsRegenerating = false;
        OnIsRegeneratingChanged?.Invoke(IsRegenerating);
    }

    /// <summary>
    /// Sorts nodes at the same level of the node tree first by whether or not it's a directory, then alphabetically.
    /// </summary>
    /// <param name="node">The node whose children are to be sorted.</param>
    private static void SortRecursively(FileIndexNode node)
    {
        node.ChildNodes?.Sort((first, second) =>
        {
            var typeDifference = (first.ChildNodes.Count != 0).CompareTo(second.ChildNodes.Count != 0) * -1;
            return typeDifference == 0 ? string.CompareOrdinal(first.Name, second.Name) : typeDifference;
        });

        if (node.ChildNodes != null)
        {
            foreach (var child in node.ChildNodes)
            {
                SortRecursively(child);
            }
        }
    }

    /// <summary>
    /// Checks if a file URI is similar to the path of its containing URI. This is used to find which copy of a file
    /// is most likely to be the "original" in cases where multiple copies of a file by the same URI exist.
    /// </summary>
    /// <param name="earcPath">The relative path to the archive.</param>
    /// <param name="uri">The URI of the file to compare.</param>
    /// <returns><c>true</c> if the URI is very similar to the archive path.</returns>
    private static bool IsUriSimilarToEarcPath(string earcPath, string uri)
    {
        var earcDirectory = Path.GetDirectoryName(earcPath)!;
        var earcTransformed = earcDirectory.Replace('\\', '/');
        var uriTransformed = uri[(uri.IndexOf(':') + 3)..];
        return uriTransformed.StartsWith(earcTransformed, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Indexes all files and subdirectories of a directory in parallel.
    /// </summary>
    /// <param name="archiveRoot">The directory to index.</param>
    /// <returns>Tuple containing packed file list and loose file list.</returns>
    private (ConcurrentBag<FileIndexerItem> PackedFiles, ConcurrentBag<FileIndexerItem> LooseFiles) IndexFiles(
        string archiveRoot)
    {
        var packedFiles = new ConcurrentBag<FileIndexerItem>();
        var looseFiles = new ConcurrentBag<FileIndexerItem>();

        Parallel.ForEach(Directory.GetFiles(archiveRoot, "*.*", SearchOption.AllDirectories)
            // TODO: Ensure this restriction doesn't mess up the PS4 files that actually use the patch system
            .Where(f => !IOHelper.DoesPathStartWith(f, _profile.PatchDirectory)), file =>
        {
            var relativePath = Path.GetRelativePath(_profile.GameDataDirectory, file)
                .Replace('\\', '/');
            var extension = Path.GetExtension(file).ToLower();

            if (extension == ".earc")
            {
                using var archive = new EbonyArchive(file);

                // Only archive original archives
                if (!archive.HasFlag(EbonyArchiveFlags.FlagrumModArchive)
                    && archive.Files.All(f => ((uint)f.Value.Flags & 256) == 0))
                {
                    packedFiles.Add(new FileIndexerItem
                    {
                        FilePath = relativePath,
                        Uris = archive.Files
                            .Where(f => !f.Value.Flags.HasFlag(EbonyArchiveFileFlags.Reference))
                            .ToDictionary(f => f.Key, f => f.Value.Uri)
                    });
                }
            }
            else if (_allowedLooseFileExtensions.Contains(extension))
            {
                var uri = $"data://{relativePath}".ToLower();
                looseFiles.Add(new FileIndexerItem
                {
                    FilePath = relativePath,
                    Uris = new Dictionary<AssetId, string> {{new AssetId(uri), uri}}
                });
            }
        });

        return (packedFiles, looseFiles);
    }
}