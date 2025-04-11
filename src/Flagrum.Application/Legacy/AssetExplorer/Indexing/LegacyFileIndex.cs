using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.AssetExplorer.Indexing;
using MemoryPack;

namespace Flagrum.Application.Legacy.AssetExplorer.Indexing;

[MemoryPackable]
public partial class LegacyFileIndex
{
    private Dictionary<ulong, FileIndexFile> _hashMap = new();

    public FileIndexNode RootNode { get; set; }
    public List<FileIndexArchive> Archives { get; set; }
    public List<FileIndexFile> Files { get; set; }

    public FileIndexFile this[ulong uriHash] => _hashMap.TryGetValue(uriHash, out var result) ? result : null;

    public void RegenerateHashMap()
    {
        _hashMap = Files.ToDictionary(f => Cryptography.HashFileUri64(f.Uri), f => f);
    }

    public void Save(string path)
    {
        Repository.Save(this, path);
    }
}