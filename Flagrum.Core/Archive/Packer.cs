using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Services.Logging;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

public class Packer
{
    private const uint PointerSize = 8;
    private const uint BlockSize = 512;
    private readonly ArchiveHeader _header;

    private readonly Logger _logger;

    public Packer()
    {
        _logger = new ConsoleLogger();
        _header = new ArchiveHeader();
        Files = new ConcurrentCollection<ArchiveFile>();
    }

    private Packer(ArchiveHeader header, List<ArchiveFile> files) : this()
    {
        _header = header;
        Files = new ConcurrentCollection<ArchiveFile>(files);
    }

    public ConcurrentCollection<ArchiveFile> Files { get; private set; }

    public bool IsProtectedArchive => _header.IsProtectedArchive;

    public static Packer FromUnpacker(ArchiveHeader header, List<ArchiveFile> files)
    {
        return new Packer(header, files);
    }

    public void SetFlags(ArchiveHeaderFlags flags)
    {
        _header.Flags = flags;
    }

    public bool HasFile(string uri)
    {
        return Files.Any(f => f.Uri == uri);
    }

    public void AddCompressedFile(string uri, byte[] data, bool autoload = false)
    {
        var file = new ArchiveFile(uri)
        {
            Flags = ArchiveFileFlag.Compressed
        };

        if (autoload)
        {
            file.Flags |= ArchiveFileFlag.Autoload;
        }

        file.SetRawData(data);
        Files.Add(file);
    }

    public void AddProcessedFile(string uri, ArchiveFileFlag flags, byte[] data, uint size, ushort key, string relativePathOverride)
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

        Files.Add(file);
    }

    public void AddFile(string uri, ArchiveFileFlag flags, byte[] data)
    {
        var file = new ArchiveFile(uri);
        file.Flags = flags;

        if (data != null)
        {
            file.SetRawData(data);
        }

        Files.Add(file);
    }

    public void AddFile(byte[] data, string uri)
    {
        var file = new ArchiveFile(uri);
        file.SetRawData(data);

        Files.Add(file);
    }

    public void AddReference(string uri, bool autoload)
    {
        var file = new ArchiveFile(uri);
        file.Flags = ArchiveFileFlag.Reference;

        if (autoload)
        {
            file.Flags |= ArchiveFileFlag.Autoload;
        }

        Files.Add(file);
    }

    public void UpdateFile(string query, byte[] data)
    {
        var match = Files.FirstOrDefault(f => f.Uri.EndsWith(query));
        if (match != null)
        {
            match.SetRawData(data);
        }
        else
        {
            throw new ArgumentException($"Could not find file ending with \"{query}\" in the archive.",
                nameof(query));
        }
    }

    public void UpdateFileWithProcessedData(string query, byte[] data)
    {
        var match = Files.FirstOrDefault(f => f.Uri.EndsWith(query));
        if (match != null)
        {
            match.SetDataByFlags(data);
        }
        else
        {
            throw new ArgumentException($"Could not find file ending with \"{query}\" in the archive.",
                nameof(query));
        }
    }

    public void RemoveFile(string uri)
    {
        var match = Files.FirstOrDefault(f => f.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            Files.Remove(match);
        }
        else
        {
            throw new ArgumentException("Could not find file in the archive", nameof(uri));
        }
    }

    public void UpdateFileWithProcessedData(string uri, uint originalSize, byte[] data)
    {
        var match = Files.FirstOrDefault(f => f.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            match.SetProcessedData(originalSize, data);
        }
        else
        {
            throw new FileNotFoundException("File was not found in the archive", uri);
        }
    }

    public void AddFileFromBackup(string uri, string relativePath, uint size, ArchiveFileFlag flags, ushort key,
        byte[] data)
    {
        var file = new ArchiveFile(uri, relativePath, size, (uint)data.Length, flags, 0, 0, key);
        file.SetDataByFlags(data);
        Files.Add(file);
    }

    public void WriteToFile(string path)
    {
        using var archiveStream = new MemoryStream();

        _logger.LogInformation("Packing archive...");

        if (_header.Flags.HasFlag(ArchiveHeaderFlags.Copyguard))
        {
            _header.Version |= ArchiveHeader.ProtectVersionHash;
        }

        Files = new ConcurrentCollection<ArchiveFile>(Files
            .OrderBy(f => f.Flags.HasFlag(ArchiveFileFlag.Autoload))
            .ThenBy(f => f.Uri.EndsWith(".autoext"))
            .ThenBy(f => f.Flags.HasFlag(ArchiveFileFlag.Reference))
            .ThenBy(f => f.TypeHash)
            .ThenBy(f => f.UriHash));

        _header.UriListOffset = ArchiveHeader.Size +
                                Serialization.GetAlignment((uint)Files.Count * ArchiveFile.HeaderSize,
                                    PointerSize);

        var endOfUriList = SerializeUriList(out var uriListStream);
        archiveStream.Seek(_header.UriListOffset, SeekOrigin.Begin);
        uriListStream.CopyTo(archiveStream);
        uriListStream.Dispose();

        _header.PathListOffset =
            Serialization.GetAlignment((uint)(_header.UriListOffset + endOfUriList), PointerSize);

        var endOfPathList = SerializePathList(out var pathListStream);
        archiveStream.Seek(_header.PathListOffset, SeekOrigin.Begin);
        pathListStream.CopyTo(archiveStream);
        pathListStream.Dispose();

        _header.DataOffset = Serialization.GetAlignment(_header.PathListOffset + (uint)endOfPathList, BlockSize);

        var dataStream = SerializeFileData();
        archiveStream.Seek(_header.DataOffset, SeekOrigin.Begin);
        dataStream.CopyTo(archiveStream);
        dataStream.Dispose();

        var headerStream = SerializeHeader();
        archiveStream.Seek(0, SeekOrigin.Begin);
        headerStream.CopyTo(archiveStream);
        headerStream.Dispose();

        var fileHeaderStream = SerializeFileHeaders();
        archiveStream.Seek(ArchiveHeader.Size, SeekOrigin.Begin);
        fileHeaderStream.CopyTo(archiveStream);
        fileHeaderStream.Dispose();

        File.WriteAllBytes(path, archiveStream.ToArray());
    }

    private Stream SerializeHeader()
    {
        _logger.LogInformation("Serializing Archive Header");

        var stream = new MemoryStream();

        stream.Write(ArchiveHeader.DefaultTag);
        stream.Write(BitConverter.GetBytes(_header.Version));
        stream.Write(BitConverter.GetBytes((uint)Files.Count));
        stream.Write(BitConverter.GetBytes(BlockSize));
        stream.Write(BitConverter.GetBytes(ArchiveHeader.Size));
        stream.Write(BitConverter.GetBytes(_header.UriListOffset));
        stream.Write(BitConverter.GetBytes(_header.PathListOffset));
        stream.Write(BitConverter.GetBytes(_header.DataOffset));
        stream.Write(BitConverter.GetBytes((uint)_header.Flags));
        stream.Write(BitConverter.GetBytes(ArchiveHeader.DefaultChunkSize));

        // Archive hash must be zero before the whole header is hashed
        stream.Write(BitConverter.GetBytes((ulong)0));

        // Constant padding
        stream.Write(new byte[16]);

        _header.Hash = Cryptography.Base64Hash(stream.ToArray());
        stream.Dispose();

        stream = new MemoryStream();

        stream.Write(ArchiveHeader.DefaultTag);
        stream.Write(BitConverter.GetBytes(_header.Version));
        stream.Write(BitConverter.GetBytes((uint)Files.Count));
        stream.Write(BitConverter.GetBytes(BlockSize));
        stream.Write(BitConverter.GetBytes(ArchiveHeader.Size));
        stream.Write(BitConverter.GetBytes(_header.UriListOffset));
        stream.Write(BitConverter.GetBytes(_header.PathListOffset));
        stream.Write(BitConverter.GetBytes(_header.DataOffset));
        stream.Write(BitConverter.GetBytes((uint)_header.Flags));
        stream.Write(BitConverter.GetBytes(ArchiveHeader.DefaultChunkSize));
        stream.Write(BitConverter.GetBytes(_header.Hash));

        // Constant padding
        stream.Write(new byte[16]);

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    private Stream SerializeFileHeaders()
    {
        _logger.LogInformation("Serializing File Headers");

        var stream = new MemoryStream();
        var hash = ArchiveFile.HeaderHash ^ _header.Hash;
        
        if (_header.Flags.HasFlag(ArchiveHeaderFlags.Copyguard))
        {
            hash ^= ArchiveHeader.CopyguardHash;
        }

        foreach (var file in Files)
        {
            var size = file.Size;
            var processedSize = file.ProcessedSize;
            var dataOffset = file.DataOffset;

            if (!file.Flags.HasFlag(ArchiveFileFlag.MaskProtected))
            {
                hash = Cryptography.MergeHashes(hash, file.UriAndTypeHash);
                size ^= (uint)(hash >> 32);
                processedSize ^= (uint)hash;
                hash = Cryptography.MergeHashes(hash, ~file.UriAndTypeHash);
                dataOffset ^= hash;
            }

            stream.Write(BitConverter.GetBytes(file.UriAndTypeHash));
            stream.Write(BitConverter.GetBytes(size));
            stream.Write(BitConverter.GetBytes(processedSize));
            stream.Write(BitConverter.GetBytes((uint)file.Flags));
            stream.Write(BitConverter.GetBytes(file.UriOffset));
            stream.Write(BitConverter.GetBytes(dataOffset));
            stream.Write(BitConverter.GetBytes(file.RelativePathOffset));
            stream.WriteByte(file.LocalizationType);
            stream.WriteByte(file.Locale);
            stream.Write(BitConverter.GetBytes(file.Key));
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    private int SerializeUriList(out Stream uriListStream)
    {
        _logger.LogInformation("Serializing File URIs");

        uriListStream = new MemoryStream();
        var currentUriOffset = 0;

        foreach (var file in Files)
        {
            var size = EncodeString(file.Uri, out var bytes);
            file.UriOffset = _header.UriListOffset + (uint)currentUriOffset;
            uriListStream.Seek(currentUriOffset, SeekOrigin.Begin);
            uriListStream.Write(bytes, 0, size);
            currentUriOffset += (int)Serialization.GetAlignment((uint)size, PointerSize);
        }

        uriListStream.Seek(0, SeekOrigin.Begin);
        return currentUriOffset;
    }

    private int SerializePathList(out Stream pathListStream)
    {
        _logger.LogInformation("Serializing File Paths");

        pathListStream = new MemoryStream();
        var currentPathOffset = 0;

        foreach (var file in Files)
        {
            var size = EncodeString(file.RelativePath, out var bytes);
            file.RelativePathOffset = _header.PathListOffset + (uint)currentPathOffset;
            pathListStream.Seek(currentPathOffset, SeekOrigin.Begin);
            pathListStream.Write(bytes, 0, size);
            currentPathOffset += (int)Serialization.GetAlignment((uint)size, PointerSize);
        }

        pathListStream.Seek(0, SeekOrigin.Begin);
        return currentPathOffset;
    }

    private Stream SerializeFileData()
    {
        _logger.LogInformation("Serializing File Data");

        var stream = new MemoryStream();
        var rng = new Random((int)_header.Hash);
        var currentDataOffset = 0L;

        foreach (var file in Files)
        {
            if (file.Key == 0 && _header.IsProtectedArchive)
            {
                var hashCode = file.Uri.GetHashCode();
                file.Key = (ushort)((hashCode >> 16) ^ hashCode);
                if (file.Key == 0)
                {
                    file.Key = 57005;
                }
            }

            if (!file.Flags.HasFlag(ArchiveFileFlag.Compressed) && !file.Flags.HasFlag(ArchiveFileFlag.Encrypted))
            {
                file.Key = 0;
            }

            file.DataOffset = _header.DataOffset + (uint)currentDataOffset;

            var fileData = file.GetDataForExport();
            stream.Write(fileData, 0, fileData.Length);

            var finalSize = Serialization.GetAlignment(file.ProcessedSize, BlockSize);
            var paddingSize = finalSize - file.ProcessedSize;
            var padding = new byte[paddingSize];
            rng.NextBytes(padding);
            stream.Write(padding, 0, (int)paddingSize);

            currentDataOffset += finalSize;
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    private int EncodeString(string value, out byte[] bytes)
    {
        var stringBufferSize = value.Length + 1 > 256 ? value.Length + 1 : 256;

        var stringBuffer = new char[stringBufferSize];
        bytes = new byte[stringBufferSize * 4];

        uint i;
        for (i = 0; i < value.Length; ++i)
        {
            stringBuffer[i] = value[(int)i];
        }

        stringBuffer[i] = char.MinValue;

        return Encoding.UTF8.GetEncoder().GetBytes(stringBuffer, 0, value.Length, bytes, 0, true);
    }
}