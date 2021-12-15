using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

public class Unpacker
{
    private const ulong KeyMultiplier = 1103515245;
    private const ulong KeyAdditive = 12345;
    private readonly ArchiveHeader _header;

    private readonly MemoryStream _stream;
    private List<ArchiveFile> _files;

    public Unpacker(string archivePath)
    {
        _stream = new MemoryStream(File.ReadAllBytes(archivePath));
        _header = ReadHeader();
    }

    /// <summary>
    ///     Retrieves the data for one file in the archive
    /// </summary>
    /// <param name="query">A string that must be contained in the URI</param>
    /// <returns>Buffer containing the file data</returns>
    public byte[] UnpackFileByQuery(string query)
    {
        _files ??= ReadFileHeaders().ToList();

        var match = _files.FirstOrDefault(f => f.Uri.Contains(query));
        if (match != null)
        {
            if (!match.HasData)
            {
                ReadFileData(match);
            }

            return match.GetUnencryptedData();
        }

        return Array.Empty<byte>();
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
            
            result.Add(match.Uri, match.GetUnencryptedData());
        }

        return result;
    }

    public Packer ToPacker()
    {
        _files ??= ReadFileHeaders().ToList();

        foreach (var file in _files)
        {
            if (!file.HasData)
            {
                ReadFileData(file);
            }
        }

        return Packer.FromFileList(_files);
    }

    private void ReadFileData(ArchiveFile file)
    {
        _stream.Seek((long)file.DataOffset, SeekOrigin.Begin);
        var buffer = new byte[file.ProcessedSize];
        _stream.Read(buffer, 0, (int)file.ProcessedSize);

        if (file.Key > 0)
        {
            var partialKey = file.Key * KeyMultiplier + KeyAdditive;
            var finalKey = partialKey * KeyMultiplier + KeyAdditive;

            var firstNumber = BitConverter.ToUInt32(buffer, 0);
            var secondNumber = BitConverter.ToUInt32(buffer, 4);

            firstNumber ^= (uint)(finalKey >> 32);
            secondNumber ^= (uint)finalKey;

            var firstKey = BitConverter.GetBytes(firstNumber);
            var secondKey = BitConverter.GetBytes(secondNumber);

            for (var k = 0; k < 4; k++)
            {
                buffer[k] = firstKey[k];
            }

            for (var k = 0; k < 4; k++)
            {
                buffer[k + 4] = secondKey[k];
            }
        }

        if (file.Flags.HasFlag(ArchiveFileFlag.Encrypted))
        {
            buffer = Cryptography.Decrypt(buffer);
        }

        if (file.ProcessedSize > file.Size)
        {
            var finalData = new byte[file.Size];
            Array.Copy(buffer, 0, finalData, 0, file.Size);
            file.SetData(finalData);
        }
        else
        {
            file.SetData(buffer);
        }
    }

    private IEnumerable<ArchiveFile> ReadFileHeaders()
    {
        var hash = ArchiveFile.HeaderHash ^ _header.Hash;

        if ((_header.Flags & (uint)ArchiveFileFlag.Copyguard) > 0)
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
                Flags = (ArchiveFileFlag)ReadUint32(),
                UriOffset = ReadUint32(),
                DataOffset = ReadUint64(),
                RelativePathOffset = ReadUint32(),
                LocalizationType = ReadByte(),
                Locale = ReadByte(),
                Key = ReadUint16()
            };

            if (!file.Flags.HasFlag(ArchiveFileFlag.MaskProtected))
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
        header.Flags = ReadUint32();
        header.ChunkSize = ReadUint32();
        header.Hash = ReadUint64();

        return header;
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