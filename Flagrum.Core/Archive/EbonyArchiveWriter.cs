using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Core.Archive;

public partial class EbonyArchive
{
    private List<ArchiveFile> _sorted;

    /// <summary>
    /// Writes the archive back to the path it originates from
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if called on an archive that was not instantiated with a file path</exception>
    public void WriteToSource(LuminousGame targetGame)
    {
        if (_sourcePath == null)
        {
            throw new InvalidOperationException(
                "This method can only be called on archives that were opened from a file");
        }

        WriteToFile(_sourcePath, targetGame);
    }

    public void WriteToFile(string path, LuminousGame targetGame)
    {
        using var archiveStream = new MemoryStream();

        if (_header.Flags.HasFlag(EbonyArchiveFlags.Copyguard))
        {
            _header.Version |= EbonyArchiveHeader.ProtectVersionHash;
        }

        _sorted = Files
            .Select(kvp => kvp.Value)
            .OrderBy(f => f.Flags.HasFlag(ArchiveFileFlag.Autoload))
            .ThenBy(f => f.Uri.EndsWith(".autoext"))
            .ThenBy(f => f.Flags.HasFlag(ArchiveFileFlag.Reference))
            .ThenBy(f => f.TypeHash)
            .ThenBy(f => f.UriHash)
            .ToList();

        _header.UriListOffset = EbonyArchiveHeader.Size +
                                Serialization.GetAlignment((uint)_sorted.Count * ArchiveFile.HeaderSize,
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

        _header.DataOffset =
            Serialization.GetAlignment(_header.PathListOffset + (uint)endOfPathList, _header.BlockSize);

        archiveStream.Seek(_header.DataOffset, SeekOrigin.Begin);
        SerializeFileData(archiveStream, out var endOfFile);
        _stream?.Dispose();

        if (targetGame == LuminousGame.FFXV)
        {
            var headerStream = SerializeHeader();
            archiveStream.Seek(0, SeekOrigin.Begin);
            headerStream.CopyTo(archiveStream);
            headerStream.Dispose();
        }

        var fileHeaderStream = SerializeFileHeaders();
        archiveStream.Seek(EbonyArchiveHeader.Size, SeekOrigin.Begin);
        fileHeaderStream.CopyTo(archiveStream);
        fileHeaderStream.Dispose();

        if (targetGame == LuminousGame.Forspoken)
        {
            var headerStream = SerializeHeaderWitch(archiveStream, endOfFile);
            archiveStream.Seek(0, SeekOrigin.Begin);
            headerStream.CopyTo(archiveStream);
            headerStream.Dispose();
        }

        archiveStream.Seek(0, SeekOrigin.Begin);
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        archiveStream.CopyTo(fileStream);
    }

    private Stream SerializeHeader()
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write(EbonyArchiveHeader.MagicValue);
        writer.Write(_header.Version);
        writer.Write((uint)_sorted.Count);
        writer.Write(_header.BlockSize);
        writer.Write(EbonyArchiveHeader.Size);
        writer.Write(_header.UriListOffset);
        writer.Write(_header.PathListOffset);
        writer.Write(_header.DataOffset);
        writer.Write((uint)_header.Flags);
        writer.Write(_header.ChunkSize);

        // Archive hash must be zero before the whole header is hashed
        writer.Write(0UL);

        // Constant padding
        writer.Write(new byte[16]);

        _header.Hash = Cryptography.Base64Hash(stream.ToArray());
        writer.Dispose();

        stream = new MemoryStream();
        writer = new BinaryWriter(stream, Encoding.UTF8, true);

        writer.Write(EbonyArchiveHeader.MagicValue);
        writer.Write(_header.Version);
        writer.Write((uint)_sorted.Count);
        writer.Write(_header.BlockSize);
        writer.Write(EbonyArchiveHeader.Size);
        writer.Write(_header.UriListOffset);
        writer.Write(_header.PathListOffset);
        writer.Write(_header.DataOffset);
        writer.Write((uint)_header.Flags);
        writer.Write(_header.ChunkSize);
        writer.Write(_header.Hash);

        // Constant padding
        stream.Write(new byte[16]);

        stream.Seek(0, SeekOrigin.Begin);
        writer.Dispose();
        return stream;
    }

    private Stream SerializeFileHeaders()
    {
        var stream = new MemoryStream();
        var hash = ArchiveFile.HeaderHash ^ _header.Hash;

        if (_header.Flags.HasFlag(EbonyArchiveFlags.Copyguard))
        {
            hash ^= EbonyArchiveHeader.CopyguardHash;
        }

        foreach (var file in _sorted)
        {
            var size = file.Size;
            var processedSize = file.ProcessedSize;
            var dataOffset = file.DataOffset;

            if (!file.Flags.HasFlag(ArchiveFileFlag.MaskProtected) && _header.IsProtectedArchive)
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
        uriListStream = new MemoryStream();
        var currentUriOffset = 0;

        foreach (var file in _sorted)
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
        pathListStream = new MemoryStream();
        var currentPathOffset = 0;

        foreach (var file in _sorted)
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

    private void SerializeFileData(Stream stream, out long endOfFile)
    {
        endOfFile = 0L;
        var rng = new Random((int)_header.Hash);
        var currentDataOffset = 0UL;

        for (var i = 0; i < _sorted.Count; i++)
        {
            var file = _sorted.ElementAt(i);

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

            file.DataOffset = _header.DataOffset + currentDataOffset;

            var fileData = file.GetDataForExport();
            stream.Write(fileData, 0, fileData.Length);

            if (i == _sorted.Count - 1)
            {
                endOfFile = stream.Position;
            }

            var finalSize = Serialization.GetAlignment(file.ProcessedSize, _header.BlockSize);
            var paddingSize = finalSize - file.ProcessedSize;
            var padding = new byte[paddingSize];
            rng.NextBytes(padding);
            stream.Write(padding, 0, (int)paddingSize);

            currentDataOffset += finalSize;
        }
    }

    private Stream SerializeHeaderWitch(Stream archiveStream, long endOfFile)
    {
        _header.Hash = CalculateArchiveHash(archiveStream, endOfFile);

        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        writer.Write(EbonyArchiveHeader.MagicValue);
        writer.Write(_header.Version);
        writer.Write((uint)_sorted.Count);
        writer.Write(_header.BlockSize);
        writer.Write(EbonyArchiveHeader.Size);
        writer.Write(_header.UriListOffset);
        writer.Write(_header.PathListOffset);
        writer.Write(_header.DataOffset);
        writer.Write((uint)_header.Flags);
        writer.Write(_header.ChunkSize);
        writer.Write(_header.Hash);

        // Constant padding
        stream.Write(new byte[16]);

        stream.Seek(0, SeekOrigin.Begin);
        writer.Dispose();
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

    /// <summary>
    /// Forspoken uses a different method of hashing than FFXV
    /// See https://github.com/yretenai/Witch/blob/develop/Scarlet/Archive/EbonyArchive.cs
    /// </summary>
    private ulong CalculateArchiveHash(Stream stream, long dataSize, bool force = false)
    {
        if (dataSize > int.MaxValue && !force)
        {
            return 0;
        }

        const uint CHUNK_SIZE = 0x8000000;
        const ulong XOR_CONST = 0x7575757575757575UL;
        const uint STATIC_SEED0 = 0xb16949df;
        const uint STATIC_SEED1 = 0x104098f5;
        const uint STATIC_SEED2 = 0x9eb9b68b;
        const uint STATIC_SEED3 = 0x3120f7cb;

        var position = stream.Position;

        stream.Position = EbonyArchiveHeader.Size;

        try
        {
            var size = dataSize - EbonyArchiveHeader.Size;

            // hash up to MAX_SIZE (128 MiB) of data in CHUNK_SIZE (8MiB) chunks and hash them into a 128-bit hash array.
            var blocks = (int)(Align(size, CHUNK_SIZE) >> 27); // 0x8000000 -> 0x1

            using var chunk = MemoryOwner<byte>.Allocate((int)CHUNK_SIZE);
            using var hashList = MemoryOwner<byte>.Allocate((blocks + 1) << 4); // 1 -> 16
            for (var i = 0; i < blocks; ++i)
            {
                var offset = (long)i << 27; // 1 -> 0x8000000
                var length = (int)Math.Min(size - offset, CHUNK_SIZE);
                _ = stream.Read(chunk.Span[..length]);
                MD5.HashData(chunk.Span[..length]).CopyTo(hashList.Span[(i << 4)..]);
            }

            var hashSeed = MemoryMarshal.Cast<byte, uint>(hashList.Span[^16..]);
            if (blocks > 8)
            {
                var seed = (ulong)size ^ XOR_CONST;
                // xorshift64.
                for (var i = 0; i < 4; ++i)
                {
                    seed ^= seed << 13;
                    seed ^= seed >> 7;
                    seed ^= seed << 17;
                    hashSeed[i] = (uint)seed;
                }
            }
            else
            {
                // some magic numbers fished up from leviathan's egg pond or someth
                hashSeed[0] = STATIC_SEED0;
                hashSeed[1] = STATIC_SEED1;
                hashSeed[2] = STATIC_SEED2;
                hashSeed[3] = STATIC_SEED3;
            }

            // SHA256 the hash list and overlay the 256-bit hash into 64-bits.
            // one might wonder, why would you not just use Murmur64 or xxHash64 as it's faster
            // since we only care about data integrity, we don't need the security benefits of SHA
            var sha = MemoryMarshal.Cast<byte, ulong>(SHA256.HashData(hashList.Span).AsSpan());
            return sha[0] ^ sha[1] ^ sha[2] ^ sha[3];
        }
        finally
        {
            stream.Position = position;
        }
    }

    private long Align(long value, long n)
    {
        return unchecked(value + (n - 1)) & ~(n - 1);
    }
}