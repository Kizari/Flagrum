using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

public class Unpacker : IDisposable
{
    private readonly ArchiveHeader _header;
    private readonly object _lock = new();

    private readonly Stream _stream;
    private List<ArchiveFile> _files;

    public ArchiveHeader Header => _header;

    public Unpacker(byte[] archive)
    {
        _stream = new MemoryStream(archive);
        _header = ReadHeader();
    }

    public Unpacker(string archivePath)
    {
        _stream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
        _header = ReadHeader();
    }

    public List<ArchiveFile> Files => _files ??= ReadFileHeaders().ToList();
    public bool IsProtectedArchive => _header.IsProtectedArchive;

    public void Dispose()
    {
        _stream?.Dispose();
    }

    public bool HasFile(string uri)
    {
        _files ??= ReadFileHeaders().ToList();
        return _files.Any(f => f.Uri == uri);
    }

    public string GetUriByQuery(string query)
    {
        _files ??= ReadFileHeaders().ToList();
        return _files.FirstOrDefault(f => f.Uri.EndsWith(query))?.Uri;
    }

    /// <summary>
    /// Retrieves the data for one file in the archive
    /// </summary>
    /// <param name="query">A string that must be contained in the URI</param>
    /// <returns>Buffer containing the file data</returns>
    public byte[] UnpackFileByQuery(string query, out string uri)
    {
        _files ??= ReadFileHeaders().ToList();

        var match = _files.FirstOrDefault(f => f.Uri.Contains(query, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            uri = match.Uri;
            if (!match.HasData)
            {
                ReadFileData(match);
            }

            return match.GetReadableData();
        }

        uri = null;
        return Array.Empty<byte>();
    }

    public byte[] UnpackRawByUri(string uri)
    {
        _files ??= ReadFileHeaders().ToList();

        var match = _files.FirstOrDefault(f => f.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            if (!match.HasData)
            {
                ReadFileData(match);
            }

            return match.GetRawData();
        }

        throw new FileNotFoundException("The file at the given URI was not found", uri);
    }

    public byte[] UnpackReadableByUri(string uri)
    {
        _files ??= ReadFileHeaders().ToList();

        var match = _files.FirstOrDefault(f => f.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            if (!match.HasData)
            {
                ReadFileData(match);
            }

            return match.GetReadableData();
        }

        throw new FileNotFoundException("The file at the given URI was not found", uri);
    }

    public Dictionary<string, byte[]> UnpackFilesByQuery(string query)
    {
        _files ??= ReadFileHeaders().ToList();

        var result = new Dictionary<string, byte[]>();
        var matches = _files.Where(f => f.Uri.Contains(query));
        foreach (var match in matches)
        {
            if (!match.HasData)
            {
                ReadFileData(match);
            }

            result.Add(match.Uri, match.GetReadableData());
        }

        return result;
    }

    public Packer ToPacker()
    {
        ReadDataForAllFiles();
        var packer = Packer.FromUnpacker(_header, _files);
        Dispose();
        return packer;
    }

    public void ReadDataForAllFiles()
    {
        _files ??= ReadFileHeaders().ToList();
        foreach (var file in _files.Where(file => !file.HasData))
        {
            ReadFileData(file);
        }
    }

    public void ReadFileData(ArchiveFile file)
    {
        byte[] buffer;

        lock (_stream)
        {
            _stream.Seek((long)file.DataOffset, SeekOrigin.Begin);
            buffer = new byte[file.ProcessedSize];
            _ = _stream.Read(buffer, 0, (int)file.ProcessedSize);
        }

        if (file.Flags.HasFlag(ArchiveFileFlag.Compressed))
        {
            file.SetCompressedData(buffer);
        }
        else if (file.Flags.HasFlag(ArchiveFileFlag.Encrypted))
        {
            file.SetEncryptedData(buffer);
        }
        else
        {
            file.SetRawData(buffer);
        }
    }

    private IEnumerable<ArchiveFile> ReadFileHeaders()
    {
        var hash = ArchiveFile.HeaderHash ^ _header.Hash;

        if (_header.Flags.HasFlag(ArchiveHeaderFlags.Copyguard))
        {
            hash ^= ArchiveHeader.CopyguardHash;
        }

        for (var i = 0; i < _header.FileCount; i++)
        {
            _stream.Seek(_header.FileHeadersOffset + i * ArchiveFile.HeaderSize, SeekOrigin.Begin);

            var file = new ArchiveFile
            {
                UriAndTypeHash = ReadUint64(),
                Size = ReadUint32(),
                ProcessedSize = ReadUint32(),
                Flags = (ArchiveFileFlag)ReadInt32(),
                UriOffset = ReadUint32(),
                DataOffset = ReadUint64(),
                RelativePathOffset = ReadUint32(),
                LocalizationType = ReadByte(),
                Locale = ReadByte(),
                Key = ReadUint16()
            };

            if (!file.Flags.HasFlag(ArchiveFileFlag.MaskProtected) && _header.IsProtectedArchive)
            {
                var subhash = Cryptography.MergeHashes(hash, file.UriAndTypeHash);
                file.Size ^= (uint)(subhash >> 32);
                file.ProcessedSize ^= (uint)subhash;
                hash = Cryptography.MergeHashes(subhash, ~file.UriAndTypeHash);
                file.DataOffset ^= hash;
            }

            _stream.Seek(file.UriOffset, SeekOrigin.Begin);
            file.Uri = ReadString();

            _stream.Seek(file.RelativePathOffset, SeekOrigin.Begin);
            file.RelativePath = ReadString();

            file.DeconstructUriAndTypeHash();

            yield return file;
        }
    }

    private ArchiveHeader ReadHeader()
    {
        var header = new ArchiveHeader();

        var buffer = new byte[4];
        _stream.Read(buffer, 0, 4);
        header.Tag = buffer;

        header.Version = ReadUint32();
        header.FileCount = ReadUint32();
        header.BlockSize = ReadUint32();
        header.FileHeadersOffset = ReadUint32();
        header.UriListOffset = ReadUint32();
        header.PathListOffset = ReadUint32();
        header.DataOffset = ReadUint32();
        header.Flags = (ArchiveHeaderFlags)ReadInt32();
        header.ChunkSize = ReadUint32();
        header.Hash = ReadUint64();

        var version = header.Version & ~ArchiveHeader.ProtectVersionHash;
        header.VersionMajor = (ushort)(version >> 16);
        header.VersionMinor = (ushort)(version & ushort.MaxValue);

        return header;
    }

    public static byte[] GetFileByLocation(string earcLocation, string fileQuery)
    {
        using var unpacker = new Unpacker(earcLocation);
        return unpacker.UnpackReadableByUri(fileQuery);
    }

    private byte ReadByte()
    {
        var buffer = new byte[1];
        _stream.Read(buffer, 0, 1);
        return buffer[0];
    }

    private ushort ReadUint16()
    {
        var buffer = new byte[2];
        _stream.Read(buffer, 0, 2);
        return BitConverter.ToUInt16(buffer);
    }

    private uint ReadUint32()
    {
        var buffer = new byte[4];
        _stream.Read(buffer, 0, 4);
        return BitConverter.ToUInt32(buffer);
    }

    private int ReadInt32()
    {
        var buffer = new byte[4];
        _stream.Read(buffer, 0, 4);
        return BitConverter.ToInt32(buffer);
    }

    private ulong ReadUint64()
    {
        var buffer = new byte[8];
        _stream.Read(buffer, 0, 8);
        return BitConverter.ToUInt64(buffer);
    }

    private string ReadString()
    {
        var builder = new StringBuilder();

        var buffer = new byte[1];
        while (true)
        {
            _stream.Read(buffer, 0, 1);

            if (buffer[0] == byte.MinValue)
            {
                break;
            }

            builder.Append(Convert.ToChar(buffer[0]));
        }

        return builder.ToString();
    }
}