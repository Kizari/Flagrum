using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Abstractions.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Archive;

public partial class EbonyArchive
{
    private IDictionary<AssetId, EbonyArchiveFile> ReadFileHeaders()
    {
        using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
        var result = new Dictionary<AssetId, EbonyArchiveFile>();
        var hash = EbonyArchiveFile.HeaderHash ^ Header.Hash;

        if (Header.Flags.HasFlag(EbonyArchiveFlags.Copyguard))
        {
            hash ^= EbonyArchiveHeader.CopyguardHash;
        }

        for (var i = 0; i < Header.FileCount; i++)
        {
            _stream.Seek(Header.FileHeadersOffset + i * EbonyArchiveFile.HeaderSize, SeekOrigin.Begin);

            var file = new EbonyArchiveFile
            {
                Id = reader.ReadUInt64(),
                Size = reader.ReadUInt32(),
                ProcessedSize = reader.ReadUInt32(),
                Flags = (EbonyArchiveFileFlags)reader.ReadInt32(),
                UriOffset = reader.ReadUInt32(),
                DataOffset = reader.ReadUInt64(),
                RelativePathOffset = reader.ReadUInt32(),
                LocalizationType = reader.ReadByte(),
                Locale = reader.ReadByte(),
                Key = reader.ReadUInt16()
            };

            var f = (uint)file.Flags;
            if ((f & 256) > 0)
            {
                file.Flags = (EbonyArchiveFileFlags)(f | 2);
            }

            if (!file.Flags.HasFlag(EbonyArchiveFileFlags.MaskProtected) && Header.IsProtectedArchive)
            {
                var subhash = Cryptography.MergeHashes(hash, file.Id);
                file.Size ^= (uint)(subhash >> 32);
                file.ProcessedSize ^= (uint)subhash;
                hash = Cryptography.MergeHashes(subhash, ~file.Id);
                file.DataOffset ^= hash;
            }

            _stream.Seek(file.UriOffset, SeekOrigin.Begin);
            file.Uri = reader.ReadNullTerminatedString();

            _stream.Seek(file.RelativePathOffset, SeekOrigin.Begin);
            file.RelativePath = reader.ReadNullTerminatedString();

            file.IsDataCompressed = file.Flags.HasFlag(EbonyArchiveFileFlags.Compressed);
            file.IsDataEncrypted = file.Flags.HasFlag(EbonyArchiveFileFlags.Encrypted);
            file.SetRawData(_stream);

            // This is being recomputed via the AssetId constructor as a band-aid fix for a bigger problem for now
            // See https://github.com/Kizari/Flagrum/issues/192
            // Ideally this would simply use "file.Id" as the key, but due to differences between different builds
            // of the game, this causes issues with the demos, PS4 retail, and possibly other older versions
            result[new AssetId(file.Uri)] = file;
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