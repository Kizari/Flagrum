using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

public partial class EbonyArchive
{
    private IDictionary<ulong, ArchiveFile> ReadFileHeaders()
    {
        using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
        var result = new Dictionary<ulong, ArchiveFile>();
        var hash = ArchiveFile.HeaderHash ^ _header.Hash;

        if (_header.Flags.HasFlag(EbonyArchiveFlags.Copyguard))
        {
            hash ^= EbonyArchiveHeader.CopyguardHash;
        }

        for (var i = 0; i < _header.FileCount; i++)
        {
            _stream.Seek(_header.FileHeadersOffset + i * ArchiveFile.HeaderSize, SeekOrigin.Begin);

            var file = new ArchiveFile
            {
                UriAndTypeHash = reader.ReadUInt64(),
                Size = reader.ReadUInt32(),
                ProcessedSize = reader.ReadUInt32(),
                Flags = (ArchiveFileFlag)reader.ReadInt32(),
                UriOffset = reader.ReadUInt32(),
                DataOffset = reader.ReadUInt64(),
                RelativePathOffset = reader.ReadUInt32(),
                LocalizationType = reader.ReadByte(),
                Locale = reader.ReadByte(),
                Key = reader.ReadUInt16()
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
            file.Uri = reader.ReadNullTerminatedString();

            _stream.Seek(file.RelativePathOffset, SeekOrigin.Begin);
            file.RelativePath = reader.ReadNullTerminatedString();

            file.DeconstructUriAndTypeHash();

            file.IsDataCompressed = file.Flags.HasFlag(ArchiveFileFlag.Compressed);
            file.IsDataEncrypted = file.Flags.HasFlag(ArchiveFileFlag.Encrypted);
            file.SetRawData(_stream);

            result[Cryptography.HashFileUri64(file.Uri)] = file;
        }

        return result;
    }

    private EbonyArchiveHeader ReadHeader()
    {
        var header = new EbonyArchiveHeader();
        using var reader = new BinaryReader(_stream, Encoding.UTF8, true);

        header.Magic = reader.ReadUInt32();
        header.Version = reader.ReadUInt32();
        header.FileCount = reader.ReadUInt32();
        header.BlockSize = reader.ReadUInt32();
        header.FileHeadersOffset = reader.ReadUInt32();
        header.UriListOffset = reader.ReadUInt32();
        header.PathListOffset = reader.ReadUInt32();
        header.DataOffset = reader.ReadUInt32();
        header.Flags = (EbonyArchiveFlags)reader.ReadInt32();
        header.ChunkSize = reader.ReadUInt32();
        header.Hash = reader.ReadUInt64();

        var version = header.Version & ~EbonyArchiveHeader.ProtectVersionHash;
        header.VersionMajor = (ushort)(version >> 16);
        header.VersionMinor = (ushort)(version & ushort.MaxValue);

        return header;
    }
}