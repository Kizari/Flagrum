using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

public partial class EbonyArchive : IDisposable
{
    private const uint PointerSize = 8;

    private readonly EbonyArchiveHeader _header;
    private readonly string _sourcePath;
    private readonly Stream _stream;

    public EbonyArchive(bool isProtectedArchive)
    {
        _header = new EbonyArchiveHeader
        {
            Version = EbonyArchiveHeader.ArchiveVersion
        };

        if (isProtectedArchive)
        {
            _header.Version |= EbonyArchiveHeader.ProtectedArchiveVersion;
        }
        
        Files = new ConcurrentDictionary<ulong, ArchiveFile>();
    }

    public EbonyArchive(byte[] archive)
    {
        _stream = new MemoryStream(archive);
        _header = ReadHeader();
        Files = new ConcurrentDictionary<ulong, ArchiveFile>(ReadFileHeaders());
    }

    public EbonyArchive(string archivePath)
    {
        _sourcePath = archivePath;
        _stream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
        _header = ReadHeader();
        Files = new ConcurrentDictionary<ulong, ArchiveFile>(ReadFileHeaders());
    }

    public ConcurrentDictionary<ulong, ArchiveFile> Files { get; private set; }
    public bool IsProtectedArchive => _header.IsProtectedArchive;

    public ArchiveFile this[string uri]
    {
        get
        {
            var hash = Cryptography.HashFileUri64(uri);
            return Files[hash];
        }

        private set
        {
            var hash = Cryptography.HashFileUri64(uri);
            Files[hash] = value;
        }
    }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        _stream?.Dispose();
    }

    public void SetFlags(EbonyArchiveFlags flags)
    {
        _header.Flags = flags;
    }

    public bool HasFile(string uri)
    {
        var hash = Cryptography.HashFileUri64(uri);
        return Files.ContainsKey(hash);
    }

    public static byte[] GetFileByLocation(string earcLocation, string uri)
    {
        using var unpacker = new EbonyArchive(earcLocation);
        return unpacker[uri].GetReadableData();
    }

    public void AddProcessedFile(string uri, ArchiveFileFlag flags, byte[] data, uint size, ushort key,
        string relativePathOverride)
    {
        if (string.IsNullOrWhiteSpace(relativePathOverride))
        {
            relativePathOverride = null;
        }

        var file = new ArchiveFile(uri, relativePathOverride)
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

        var hash = Cryptography.HashFileUri64(uri);
        Files.TryAdd(hash, file);
    }

    public void AddFile(string uri, ArchiveFileFlag flags, byte[] data)
    {
        var file = new ArchiveFile(uri)
        {
            Flags = flags
        };

        if (data != null)
        {
            file.SetRawData(data);
        }

        var hash = Cryptography.HashFileUri64(uri);
        Files.TryAdd(hash, file);
    }

    public void AddFile(byte[] data, string uri)
    {
        var file = new ArchiveFile(uri);
        file.SetRawData(data);

        var hash = Cryptography.HashFileUri64(uri);
        Files.TryAdd(hash, file);
    }

    public void UpdateFile(string uri, byte[] data)
    {
        this[uri].SetRawData(data);
    }

    public void RemoveFile(string uri)
    {
        var hash = Cryptography.HashFileUri64(uri);
        Files.Remove(hash, out _);
    }

    public void UpdateFileWithProcessedData(string uri, uint originalSize, byte[] data)
    {
        this[uri].SetProcessedData(originalSize, data);
    }

    public void AddFileFromBackup(string uri, string relativePath, uint size, ArchiveFileFlag flags, ushort key,
        byte[] data)
    {
        var file = new ArchiveFile(uri, relativePath, size, (uint)data.Length, flags, 0, 0, key);
        file.SetDataByFlags(data);
        this[uri] = file;
    }
}