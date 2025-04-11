using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Flagrum.Abstractions.Archive;

namespace Flagrum.Core.Archive;

public partial class EbonyArchive : IEbonyArchive
{
    private const uint PointerSize = 8;

    private readonly string _sourcePath;
    private readonly Stream _stream;

    public EbonyArchive(bool isProtectedArchive)
    {
        Header = new EbonyArchiveHeader
        {
            Version = EbonyArchiveHeader.ArchiveVersion
        };

        if (isProtectedArchive)
        {
            Header.Version |= EbonyArchiveHeader.ProtectedArchiveVersion;
        }

        Files = new ConcurrentDictionary<AssetId, EbonyArchiveFile>();
    }

    public EbonyArchive(byte[] archive)
    {
        _stream = new MemoryStream(archive);
        Header = ReadHeader();
        Files = new ConcurrentDictionary<AssetId, EbonyArchiveFile>(ReadFileHeaders());
    }

    public EbonyArchive(string archivePath)
    {
        _sourcePath = archivePath;
        _stream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
        Header = ReadHeader();
        Files = new ConcurrentDictionary<AssetId, EbonyArchiveFile>(ReadFileHeaders());
    }

    public EbonyArchiveHeader Header { get; }

    public ConcurrentDictionary<AssetId, EbonyArchiveFile> Files { get; }
    public bool IsProtectedArchive => Header.IsProtectedArchive;

    public EbonyArchiveFile this[string uri]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Files[new AssetId(uri)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set => Files[new AssetId(uri)] = value;
    }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        _stream?.Dispose();
    }

    public bool HasFile(string uri) => Files.ContainsKey(new AssetId(uri));

    public void AddProcessedFile(string uri, EbonyArchiveFileFlags flags, byte[] data, uint size, ushort key,
        string relativePathOverride)
    {
        if (string.IsNullOrWhiteSpace(relativePathOverride))
        {
            relativePathOverride = null;
        }

        var file = new EbonyArchiveFile(uri, relativePathOverride)
        {
            Flags = flags,
            Size = size,
            Key = key
        };

        if (data != null)
        {
            file.ProcessedSize = (uint)data.Length;
            file.SetDataByFlags(data);
        }

        Files.TryAdd(new AssetId(uri), file);
    }

    public void AddFile(string uri, EbonyArchiveFileFlags flags, byte[] data)
    {
        var file = new EbonyArchiveFile(uri)
        {
            Flags = flags
        };

        if (data != null)
        {
            file.SetRawData(data);
        }

        Files.TryAdd(new AssetId(uri), file);
    }

    public void RemoveFile(string uri)
    {
        Files.Remove(new AssetId(uri), out _);
    }

    public bool HasFlag(EbonyArchiveFlags flag) => Header.Flags.HasFlag(flag);

    public void SetFlags(EbonyArchiveFlags flags)
    {
        Header.Flags = flags;
    }

    public static byte[] GetFileByLocation(string earcLocation, string uri)
    {
        using var unpacker = new EbonyArchive(earcLocation);
        return unpacker[uri].GetReadableData();
    }

    public void AddFile(byte[] data, string uri)
    {
        var file = new EbonyArchiveFile(uri);
        file.SetRawData(data);
        Files.TryAdd(new AssetId(uri), file);
    }

    public void UpdateFile(string uri, byte[] data)
    {
        this[uri].SetRawData(data);
    }

    public void UpdateFileWithProcessedData(string uri, uint originalSize, byte[] data)
    {
        this[uri].SetProcessedData(originalSize, data);
    }

    public void AddFileFromBackup(string uri, string relativePath, uint size, EbonyArchiveFileFlags flags, ushort key,
        byte[] data)
    {
        var file = new EbonyArchiveFile(uri, relativePath, size, (uint)data.Length, flags, 0, 0, key);
        file.SetDataByFlags(data);
        this[uri] = file;
    }
}