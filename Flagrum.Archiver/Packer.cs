using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Archiver.Data;
using Flagrum.Core.Services.Logging;
using Flagrum.Core.Utilities;

namespace Flagrum.Archiver;

public class Packer
{
    public const uint PointerSize = 8;
    public const uint BlockSize = 512;
    private readonly List<ArchiveFile> _files;
    private readonly ArchiveHeader _header;

    private readonly Logger _logger;

    public Packer()
    {
        _logger = new ConsoleLogger();
        _header = new ArchiveHeader();
        _files = new List<ArchiveFile>();
    }

    private Packer(List<ArchiveFile> files) : this()
    {
        _files = files;
    }

    public static Packer FromFileList(List<ArchiveFile> files)
    {
        return new Packer(files);
    }

    public void AddFile(byte[] data, string uri)
    {
        var file = new ArchiveFile(uri);
        file.SetData(data);

        _files.Add(file);
    }

    public void UpdateFile(string query, byte[] data)
    {
        var match = _files.FirstOrDefault(f => f.Uri.Contains(query));
        if (match != null)
        {
            match.SetData(data);
        }
    }

    public void WriteToFile(string path)
    {
        var archiveStream = new MemoryStream();

        _logger.LogInformation("Packing archive...");

        // Sort files by TypeHash, then UriHash
        _files.Sort((first, second) =>
        {
            var difference = first.TypeHash.CompareTo(second.TypeHash);
            return difference == 0 ? first.UriHash.CompareTo(second.UriHash) : difference;
        });

        _header.UriListOffset = ArchiveHeader.Size +
                                Serialization.GetAlignment((uint)_files.Count * ArchiveFile.HeaderSize,
                                    PointerSize);

        var endOfUriList = SerializeUriList(out var uriListStream);
        archiveStream.Seek(_header.UriListOffset, SeekOrigin.Begin);
        uriListStream.CopyTo(archiveStream);

        _header.PathListOffset =
            Serialization.GetAlignment((uint)(_header.UriListOffset + endOfUriList), PointerSize);

        var endOfPathList = SerializePathList(out var pathListStream);
        archiveStream.Seek(_header.PathListOffset, SeekOrigin.Begin);
        pathListStream.CopyTo(archiveStream);

        _header.DataOffset = Serialization.GetAlignment(_header.PathListOffset + (uint)endOfPathList, BlockSize);

        var dataStream = SerializeFileData();
        archiveStream.Seek(_header.DataOffset, SeekOrigin.Begin);
        dataStream.CopyTo(archiveStream);

        var headerStream = SerializeHeader();
        archiveStream.Seek(0, SeekOrigin.Begin);
        headerStream.CopyTo(archiveStream);

        var fileHeaderStream = SerializeFileHeaders();
        archiveStream.Seek(ArchiveHeader.Size, SeekOrigin.Begin);
        fileHeaderStream.CopyTo(archiveStream);

        File.WriteAllBytes(path, archiveStream.ToArray());
    }

    private Stream SerializeHeader()
    {
        _logger.LogInformation("Serializing Archive Header");

        var stream = new MemoryStream();

        stream.Write(ArchiveHeader.DefaultTag);
        stream.Write(BitConverter.GetBytes(_header.Version));
        stream.Write(BitConverter.GetBytes((uint)_files.Count));
        stream.Write(BitConverter.GetBytes(BlockSize));
        stream.Write(BitConverter.GetBytes(ArchiveHeader.Size));
        stream.Write(BitConverter.GetBytes(_header.UriListOffset));
        stream.Write(BitConverter.GetBytes(_header.PathListOffset));
        stream.Write(BitConverter.GetBytes(_header.DataOffset));
        stream.Write(BitConverter.GetBytes((uint)0)); // Flags are always zero
        stream.Write(BitConverter.GetBytes(ArchiveHeader.DefaultChunkSize));

        // Archive hash must be zero before the whole header is hashed
        stream.Write(BitConverter.GetBytes((ulong)0));

        // Constant padding
        stream.Write(new byte[16]);

        _header.Hash = Cryptography.Base64Hash(stream.ToArray());

        stream = new MemoryStream();

        stream.Write(ArchiveHeader.DefaultTag);
        stream.Write(BitConverter.GetBytes(_header.Version));
        stream.Write(BitConverter.GetBytes((uint)_files.Count));
        stream.Write(BitConverter.GetBytes(BlockSize));
        stream.Write(BitConverter.GetBytes(ArchiveHeader.Size));
        stream.Write(BitConverter.GetBytes(_header.UriListOffset));
        stream.Write(BitConverter.GetBytes(_header.PathListOffset));
        stream.Write(BitConverter.GetBytes(_header.DataOffset));
        stream.Write(BitConverter.GetBytes((uint)0));
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

        foreach (var file in _files)
        {
            var size = file.Size;
            var processedSize = file.ProcessedSize;
            var dataOffset = file.DataOffset;
            ushort key = 0;

            if (!file.Flags.HasFlag(ArchiveFileFlag.MaskProtected))
            {
                key = file.Key;
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
            stream.Write(BitConverter.GetBytes(key));
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    private int SerializeUriList(out Stream uriListStream)
    {
        _logger.LogInformation("Serializing File URIs");

        uriListStream = new MemoryStream();
        var currentUriOffset = 0;

        foreach (var file in _files)
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

        foreach (var file in _files)
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

        foreach (var file in _files)
        {
            file.DataOffset = _header.DataOffset + (uint)currentDataOffset;

            var fileData = file.GetData();
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