using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Core.Archive;
using Flagrum.Core.Persistence;
using Flagrum.Application.Legacy.Migration;
using Injectio.Attributes;
using MemoryPack;
using ZstdSharp;

namespace Flagrum.Application.Features.AssetExplorer.Indexing;

/// <inheritdoc cref="IFileIndex" />
[MemoryPackable]
[RegisterSingleton<IFileIndex>]
public partial class FileIndex : IFileIndex
{
    private readonly IProfileService _profile;

    [MemoryPackConstructor]
    public FileIndex() { }

    public FileIndex(IProfileService profile)
    {
        _profile = profile;

        FileIndexMigration.Run(profile);

        if (File.Exists(profile.FileIndexPath))
        {
            var decompressor = new Decompressor();
            var self = this;
            MemoryPackSerializer.Deserialize(
                decompressor.Unwrap(File.ReadAllBytes(profile.FileIndexPath)), ref self);
        }
    }

    /// <summary>
    /// Simple mapping for <see cref="RootNode" /> so MemoryPack can handle serializing this properly.
    /// </summary>
    [MemoryPackInclude]
    private FileIndexNode ConcreteRootNode
    {
        get => (FileIndexNode)RootNode;
        set => RootNode = value;
    }

    /// <inheritdoc />
    public Dictionary<ulong, FileIndexArchive> Archives { get; set; } = [];

    public Dictionary<AssetId, FileIndexFile> Files { get; set; } = [];

    public FileIndexFile this[AssetId id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Files.TryGetValue(id, out var file) ? file : null;
    }

    public FileIndexFile this[string uri]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Files.TryGetValue(new AssetId(uri), out var file) ? file : null;
    }

    /// <inheritdoc />
    [MemoryPackIgnore]
    public IAssetExplorerNode RootNode { get; set; }

    /// <inheritdoc />
    [MemoryPackIgnore]
    public bool IsRegenerating { get; private set; }

    /// <inheritdoc />
    [MemoryPackIgnore]
    public bool IsEmpty => Files.Count == 0;

    /// <inheritdoc />
    public event Action<bool>? OnIsRegeneratingChanged;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(string uri) => Files.ContainsKey(new AssetId(uri));

    /// <inheritdoc />
    public string GetArchiveRelativePathByUri(string uri) => this[new AssetId(uri)]?.Archive?.RelativePath;

    /// <inheritdoc />
    public void Save(string path)
    {
        Repository.Save(this, path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFile(AssetId id, FileIndexFile file)
    {
        Files.Add(id, file);
    }

    public class Formatter : MemoryPackFormatter<IFileIndex>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, 
            scoped ref IFileIndex value) => writer.WritePackable((FileIndex)value);

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IFileIndex value) =>
            value = reader.ReadPackable<FileIndex>();
    }
}