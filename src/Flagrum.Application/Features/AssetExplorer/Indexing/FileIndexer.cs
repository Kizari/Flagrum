using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flagrum.Abstractions.Archive;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Quickenshtein;

namespace Flagrum.Application.Features.AssetExplorer.Indexing;

public partial class FileIndex
{
    private readonly HashSet<string> _rootDirectories = [];
    private HashSet<string>? _patchDirectories;
    
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
    
    /// <summary>
    /// Returns a regex that matches on names of versioned patch directories. Case-insensitive.
    /// </summary>
    /// <remarks>
    /// <c>Groups[1]</c> is presumably the patch ID, while <c>Groups[2]</c> is the patch version, without periods
    /// (e.g. 115 for 1.15).
    /// </remarks>
    [GeneratedRegex("CUSA(.+?)-patch_(.+?)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex GetVersionedPatchDirectoryRegex();

    /// <summary>
    /// Returns a regex that matches on names of patch index directories. Case-insensitive.
    /// </summary>
    /// <remarks>
    /// <c>Groups[1]</c> is the index of patch (e.g. "1" for patch1 and patch1_initial).
    /// </remarks>
    [GeneratedRegex(@"patch(\d+)(_initial)?", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex GetPatchIndexDirectoryNameRegex();

    /// <inheritdoc />
    public void Regenerate()
    {
        // Update UI to show indexing notice
        IsRegenerating = true;
        OnIsRegeneratingChanged?.Invoke(IsRegenerating);
        
        // Index base files
        _patchDirectories = InferPatchDirectories();
        var records = new ConcurrentDictionary<AssetId, FileIndexerRecord>();
        Parallel.Invoke(
            () => IndexLooseFiles(records), 
            () => IndexPackedFiles(records));
        
        // Get a list of virtual root directories from the indexed URIs
        foreach (var withoutScheme in records.Values.Select(record => record.Uri[7..])
                     .Where(w => w.Contains('/')))
        {
            _rootDirectories.Add(withoutScheme[..withoutScheme.IndexOf('/')]);
        }
        
        // Index patch files
        IndexPatchFiles(records);
        
        // Reset the file index
        RootNode = new FileIndexNode {Name = "", ChildNodes = []};
        Archives = [];
        Files = [];
        
        // Map packed assets to their respective archives
        foreach (var (id, record) in records)
        {
            if (record.Score <= -1) // Loose files
            {
                Files[id] = new FileIndexFile {Uri = record.Uri};
            }
            else // Packed files
            {
                // Get or create the archive entry associated with this file
                var hash = Cryptography.Hash64(record.FilePath);
                if (!Archives.TryGetValue(hash, out var archive))
                {
                    archive = new FileIndexArchive
                    {
                        RelativePath = record.FilePath,
                        Files = []
                    };

                    Archives[hash] = archive;
                }

                // Create the file entry, link it to the archive entry, and add to the file index
                var file = new FileIndexFile
                {
                    Archive = archive,
                    Uri = record.Uri
                };

                Files[id] = file;
                archive.Files.Add(file);
            }
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

                    currentParent.ChildNodes!.Add(newNode);
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

                        currentParent.ChildNodes!.Add(node);
                        nodeData[key] = node;
                    }

                    currentParent = node;
                }
            }
        }

        // Sort the nodes and save the file index to disk
        SortRecursively((FileIndexNode)RootNode);
        Save(_profile.FileIndexPath);
        
        // Clear records out of memory
        _patchDirectories = null;
        _rootDirectories.Clear();
        
        // Update UI to remove file indexing notice
        IsRegenerating = false;
        OnIsRegeneratingChanged?.Invoke(IsRegenerating);
    }

    /// <summary>
    /// Indexes all non-archived files.
    /// </summary>
    /// <param name="records">WIP file index dictionary.</param>
    private void IndexLooseFiles(ConcurrentDictionary<AssetId, FileIndexerRecord> records)
    {
        foreach (var path in Directory.GetFiles(_profile.GameDataDirectory, "*.*", SearchOption.AllDirectories)
                     .Select(p => Path.GetRelativePath(_profile.GameDataDirectory, p))
                     .Where(FilterPatchFiles))
        {
            var extension = Path.GetExtension(path).ToLower();
            if (_allowedLooseFileExtensions.Contains(extension))
            {
                var normalized = NormalizeRelativePath(path);
                var uri = $"data://{normalized.ToLower()}";
                records.TryAdd(new AssetId(uri), new FileIndexerRecord(uri, normalized, -1));
            }
        }
    }

    /// <summary>
    /// Indexes all archived files, prioritising those whose archive paths are most similar to the asset URI
    /// when handling duplicate assets with the same URI.
    /// </summary>
    /// <param name="records">WIP file index dictionary.</param>
    private void IndexPackedFiles(ConcurrentDictionary<AssetId, FileIndexerRecord> records)
    {
        // Use parallelism here, since each archive will need to be read to index the entries
        Parallel.ForEach(Directory.GetFiles(_profile.GameDataDirectory, "*.earc", SearchOption.AllDirectories)
                .Select(p => Path.GetRelativePath(_profile.GameDataDirectory, p))
                .Where(FilterPatchFiles),
            file =>
            {
                using var archive = new EbonyArchive(Path.Combine(_profile.GameDataDirectory, file));

                // Only archive original archives
                if (!archive.HasFlag(EbonyArchiveFlags.FlagrumModArchive)
                    && archive.Files.All(f => ((uint)f.Value.Flags & 256) == 0))
                {
                    foreach (var entry in archive.Files.Values
                                 .Where(e => !e.Flags.HasFlag(EbonyArchiveFileFlags.Reference)
                                    && e.Size > 0))
                    {
                        // Score the entry on the similarity of the URI to the archive file path
                        var comparisonPath = entry.Uri[7..]; // Truncate "data://"
                        var normalized = NormalizeRelativePath(file);
                        var newRecord = new FileIndexerRecord(entry.Uri, normalized, 
                            Levenshtein.GetDistance(comparisonPath, normalized.ToLower()));

                        // Add the record; if it already exists, only replace it if this record has a better score
                        records.AddOrUpdate(entry.Id, newRecord,
                            (_, current) => newRecord.Score < current.Score 
                                ? newRecord 
                                : current);
                    }
                }
            });
    }

    /// <summary>
    /// Indexes all patch files (both loose and archived) in order of which the patches are applied to the game.
    /// Any patch files that match files already in the file index will overwrite them.
    /// </summary>
    /// <param name="records">Current WIP file index containing the indexed base game files.</param>
    private void IndexPatchFiles(ConcurrentDictionary<AssetId, FileIndexerRecord> records)
    {
        // Get relative paths for all files from patch directories
        var patchRoots = InferLoosePatchRoots().ToArray();
        var priorityOrder = InferPatchArchiveDirectoriesRelative();
        var patchFiles = Directory.GetFiles(_profile.GameDataDirectory, "*.*", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(_profile.GameDataDirectory, p))
            .Where(p => patchRoots.Any(r => p.StartsWith(r, StringComparison.OrdinalIgnoreCase))
                && !p.Contains($"{Path.DirectorySeparatorChar}china{Path.DirectorySeparatorChar}"))
            .OrderByDescending(p => p.StartsWith(priorityOrder[0], StringComparison.OrdinalIgnoreCase));
        
        // Order patch files by priority
        patchFiles = priorityOrder.Skip(1)
            .Aggregate(patchFiles, (current, directory) => current
                .ThenByDescending(p => p.StartsWith(directory, StringComparison.OrdinalIgnoreCase)));
        
        // Iterate the patch files
        foreach (var file in patchFiles)
        {
            if (file.EndsWith(".earc", StringComparison.OrdinalIgnoreCase))
            {
                // File is an archive, process its entries
                using var archive = new EbonyArchive(Path.Combine(_profile.GameDataDirectory, file));
                foreach (var entry in archive.Files.Values.Where(e => e.Size > 0))
                {
                    if (entry.Flags.HasFlag(EbonyArchiveFileFlags.PatchedDeleted))
                    {
                        // Patch deleted the asset, remove it from the file index
                        records.TryRemove(entry.Id, out _);
                    }
                    else
                    {
                        // Asset is either new or overwrites an existing asset, index it
                        records[entry.Id] = new FileIndexerRecord(entry.Uri, NormalizeRelativePath(file), 0);
                    }
                }
            }
            else
            {
                // Loose file, index if supported type
                var extension = Path.GetExtension(file).ToLower();
                if (_allowedLooseFileExtensions.Contains(extension))
                {
                    var patchRoot = patchRoots
                        .FirstOrDefault(r => file.StartsWith(r, StringComparison.OrdinalIgnoreCase));
                    if (patchRoot == null)
                    {
                        throw new InvalidOperationException($"Could not determine patch root for {file}.");
                    }

                    var rebased = Path.GetRelativePath(patchRoot, file);
                    var normalized = NormalizeRelativePath(rebased);
                    var uri = $"data://{normalized.ToLower()}";
                    records[new AssetId(uri)] = new FileIndexerRecord(uri, normalized, -1);
                }
            }
        }
    }

    /// <summary>
    /// Predicate that filters out relative paths of files that are contained within a patch directory.
    /// </summary>
    /// <param name="relativePath">Relative path to filter.</param>
    /// <returns><c>false</c> if the path should be filtered out, otherwise <c>true</c>.</returns>
    private bool FilterPatchFiles(string relativePath) =>
        !relativePath.StartsWith(_profile.PatchDirectory + '/', StringComparison.OrdinalIgnoreCase)
        && !_patchDirectories!.Any(d => relativePath.StartsWith(d + '/'));

    /// <summary>
    /// Infers a set of top-level directories within the game data root that contain patch files.
    /// </summary>
    private HashSet<string> InferPatchDirectories() => Directory.GetDirectories(_profile.GameDataDirectory)
        .Select(path => Path.GetRelativePath(_profile.GameDataDirectory, path))
        .Where(directoryName =>
            directoryName.Equals("ACFestPackage", StringComparison.OrdinalIgnoreCase)
            || directoryName.Equals("FFXV_Patch", StringComparison.OrdinalIgnoreCase)
            || GetVersionedPatchDirectoryRegex().IsMatch(directoryName))
        .ToHashSet();

    /// <summary>
    /// Infers relative paths of directories that contain loose file directories within the patch directories.
    /// </summary>
    private IEnumerable<string> InferLoosePatchRoots()
    {
        foreach (var subdirectory in _patchDirectories!)
        {
            var path = Path.Combine(_profile.GameDataDirectory, subdirectory);
            foreach (var subdirectory2 in Directory.EnumerateDirectories(path))
            {
                var name = Path.GetRelativePath(path, subdirectory2);
                if (_rootDirectories.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    yield return subdirectory;
                }

                foreach (var subdirectory3 in Directory.EnumerateDirectories(subdirectory2))
                {
                    var name2 = Path.GetRelativePath(subdirectory2, subdirectory3);
                    if (_rootDirectories.Contains(name2, StringComparer.OrdinalIgnoreCase))
                    {
                        yield return Path.Combine(subdirectory, name);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Infers a list of relative paths of all patch directories that contain archives.
    /// </summary>
    /// <returns>Relative path list, in order of which the patches are applied to the game.</returns>
    private List<string> InferPatchArchiveDirectoriesRelative()
    {
        var directories = InferPatchArchiveDirectoriesRelativeUnordered().ToList();
        directories.Sort(new PatchPathComparer());
        return directories;
    }

    /// <summary>
    /// Infers relative paths of all patch directories that contain archives.
    /// </summary>
    /// <returns>Relative paths, not ordered by priority.</returns>
    private IEnumerable<string> InferPatchArchiveDirectoriesRelativeUnordered()
    {
        foreach (var patchRoot in _patchDirectories!)
        {
            var patchIndexDirectories = new List<string>();
            var path = Path.Combine(_profile.GameDataDirectory, patchRoot);
            InferPatchIndexDirectoriesUnordered(path, Directory.GetDirectories(path), patchIndexDirectories);
            
            foreach (var directory in patchIndexDirectories.DefaultIfEmpty(patchRoot))
            {
                yield return directory;
            }
        }
    }

    /// <summary>
    /// Infers all patch index directories from a root patch directory, recursively.
    /// </summary>
    /// <param name="parentDirectory">Directory whose subdirectories are being iterated in the current call.</param>
    /// <param name="directories">Directories to iterate in the current call.</param>
    /// <param name="results">Result list to add inferred patch index directories to.</param>
    /// <remarks>
    /// Would it be better to find these by locating "patchindex.earc" in patch folders
    /// instead of relying on convention? Would that still produce the same end result though?
    /// </remarks>
    private void InferPatchIndexDirectoriesUnordered(
        string parentDirectory, 
        IEnumerable<string> directories,
        List<string> results)
    {
        var regex = GetPatchIndexDirectoryNameRegex();
        foreach (var directory in directories)
        {
            var name = Path.GetRelativePath(parentDirectory, directory);
            if (regex.IsMatch(name))
            {
                results.Add(Path.GetRelativePath(_profile.GameDataDirectory, directory));
            }
            else
            {
                InferPatchIndexDirectoriesUnordered(directory, Directory.GetDirectories(directory), results);
            }
        }
    }

    /// <summary>
    /// Normalizes a relative path to use forward slashes and avoid leading slashes.
    /// </summary>
    /// <param name="path">Path to normalize.</param>
    /// <returns>Normalized relative path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string NormalizeRelativePath(string path) => path.Replace('\\', '/').TrimStart('/');
    
    /// <summary>
    /// Sorts nodes at the same level of the node tree first by whether it's a directory, then alphabetically.
    /// </summary>
    /// <param name="node">The node whose children are to be sorted.</param>
    private static void SortRecursively(FileIndexNode node)
    {
        node.ChildNodes?.Sort((first, second) =>
        {
            var typeDifference = (first.ChildNodes!.Count != 0).CompareTo(second.ChildNodes!.Count != 0) * -1;
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
    /// Record for temporarily holding information about a file so that <see cref="FileIndex"/> can build the index from it.
    /// </summary>
    /// <param name="Uri">URI of the file.</param>
    /// <param name="FilePath">
    /// Normalized relative path of the file on disk (will be the path of the containing archive for packed files).
    /// </param>
    /// <param name="Score">
    /// Levenschtein distance of the <paramref name="FilePath"/> from the schemeless <paramref name="Uri"/>,
    /// or <c>-1</c> for non-archived files. This is used to prioritise best matches when duplicates are encountered.
    /// </param>
    private record FileIndexerRecord(string Uri, string FilePath, int Score);

    /// <summary>
    /// Compares relative patch directory paths for sorting patches in order of which they're applied to the game.
    /// </summary>
    private class PatchPathComparer : IComparer<string>
    {
        /// <inheritdoc />
        public int Compare(string? x, string? y)
        {
            if (x == null || y == null)
            {
                throw new InvalidOperationException($"{nameof(PatchPathComparer)} does not support null strings.");
            }

            var xTokens = x.Split(Path.DirectorySeparatorChar);
            var yTokens = y.Split(Path.DirectorySeparatorChar);

            // Order by root directory, then by patch index directory
            var rootOrder = CompareRoot(xTokens[0], yTokens[0]);
            return rootOrder != 0 ? rootOrder : ComparePatchIndexDirectory(xTokens[^1], yTokens[^1]);
        }

        /// <summary>
        /// Compares the root directory of the relative paths, such that FFXV_Patch comes first,
        /// followed by CUSA{i}-patch_{n} ordered by 'n', followed finally by ACFestPackage.
        /// </summary>
        private int CompareRoot(string x, string y)
        {
            // Compare direct equality
            if (x.Equals(y, StringComparison.OrdinalIgnoreCase)) return 0;
            
            // Ensure that FFXV_Patch always comes first, and ACFestPackage always comes last
            if (x.Equals("FFXV_Patch", StringComparison.OrdinalIgnoreCase)) return -1;
            if (y.Equals("FFXV_Patch", StringComparison.OrdinalIgnoreCase)) return 1;
            if (x.Equals("ACFestPackage", StringComparison.OrdinalIgnoreCase)) return 1;
            if (y.Equals("ACFestPackage", StringComparison.OrdinalIgnoreCase)) return -1;
            
            // Finally, order by patch number for the directories not covered above
            var (xMatch, yMatch) = MatchRegex(x, y, GetVersionedPatchDirectoryRegex());
            var xNumber = int.Parse(xMatch.Groups[2].Value);
            var yNumber = int.Parse(yMatch.Groups[2].Value);
            return xNumber.CompareTo(yNumber);
        }

        /// <summary>
        /// Compares the last directory in the relative paths, such that they are ordered first by
        /// 'n' in "patch{n}" and "patch{n}_initial", then by "_initial" suffix first.
        /// Directories without "patch{n}" in them come last.
        /// </summary>
        private int ComparePatchIndexDirectory(string x, string y)
        {
            var (xMatch, yMatch) = MatchRegex(x, y, GetPatchIndexDirectoryNameRegex());
            
            // Ensure "patchN" and "patchN_initial" directories are ordered by "N"
            var xNumber = int.Parse(xMatch.Groups[1].Value);
            var yNumber = int.Parse(xMatch.Groups[1].Value);
            var numberCompare = xNumber.CompareTo(yNumber);
            if (numberCompare != 0)
            {
                return numberCompare;
            }

            // Ensure "patchN_initial" precedes "patchN"
            var xInitial = xMatch.Groups[2].Success;
            var yInitial = yMatch.Groups[2].Success;
            return xInitial == yInitial ? 0 : xInitial ? -1 : 1;
        }

        /// <summary>
        /// Matches two strings on a given regex, throwing an exception if they did not match.
        /// </summary>
        private (Match x, Match y) MatchRegex(string x, string y, Regex regex)
        {
            var xMatch = regex.Match(x);
            var yMatch = regex.Match(y);
            
            if (!xMatch.Success)
            {
                throw new InvalidOperationException($"Could not sort patch directories, unexpected directory {x}");
            }
            
            if (!yMatch.Success)
            {
                throw new InvalidOperationException($"Could not sort patch directories, unexpected directory {y}");
            }

            return (xMatch, yMatch);
        }
    }
}