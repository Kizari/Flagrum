using System;
using System.IO;
using Flagrum.Abstractions.Archive;
using Flagrum.Core.Archive.DataSource;
using Flagrum.Core.Utilities;
using Ionic.Zlib;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;

namespace Flagrum.Core.Archive;

public class EbonyArchiveFile : IGameFile
{
    private const ulong KeyMultiplier = 1103515245;
    private const ulong KeyAdditive = 12345;

    public const uint HeaderSize = 40;
    public const ulong HeaderHash = 14695981039346656037;

    private IEbonyArchiveFileDataSource _dataSource;

    public EbonyArchiveFile() { }

    public EbonyArchiveFile(string uri, string relativePathOverride = null)
    {
        if (relativePathOverride == null)
        {
            RelativePath = uri.Replace("data://", "");
            FixRelativePath();
        }
        else
        {
            RelativePath = relativePathOverride;
        }

        var newUri = uri
            .Replace(".gmdl.gfxbin", ".gmdl")
            .Replace(".gmtl.gfxbin", ".gmtl")
            .Replace(".exml", ".ebex");

        Uri = newUri;
        Id = new AssetId(Uri);
        Flags = GetDefaultBinmodFlags();
    }

    public EbonyArchiveFile(string uri, string relativePath, uint size, uint processedSize, EbonyArchiveFileFlags flags,
        byte localizationType,
        byte locale, ushort key)
    {
        RelativePath = relativePath;
        Uri = uri;
        Size = size;
        ProcessedSize = processedSize;
        Flags = flags;
        LocalizationType = localizationType;
        Locale = locale;
        Key = key;
        Id = new AssetId(Uri);
    }

    public AssetId Id { get; set; }
    public uint UriOffset { get; set; }
    public string RelativePath { get; set; }
    public uint RelativePathOffset { get; set; }
    public ulong DataOffset { get; set; }
    public uint Size { get; set; }
    public uint ProcessedSize { get; set; }
    public byte LocalizationType { get; set; }
    public byte Locale { get; set; }
    public ushort Key { get; set; }

    public bool IsDataEncrypted { get; set; }
    public bool IsDataCompressed { get; set; }

    /// <summary>
    /// This was added for the sake of FileFinder in Flagrum.Research, can otherwise be ignored.
    /// It is the ID corresponding to the file extension of the URI.
    /// </summary>
    public uint TypeId => (uint)(Id >> 44);

    /// <summary>
    /// This was added for the sake of FileFinder in Flagrum.Research, can otherwise be ignored.
    /// It is the absolute path of the archive the file is contained in.
    /// </summary>
    public string Path { get; set; }

    public string Uri { get; set; }
    public EbonyArchiveFileFlags Flags { get; set; }

    public byte[] GetReadableData()
    {
        var buffer = _dataSource.GetData();

        if (Key > 0)
        {
            var partialKey = Key * KeyMultiplier + KeyAdditive;
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

        if (IsDataCompressed)
        {
            if (Flags.HasFlag(EbonyArchiveFileFlags.LZ4Compression))
            {
                var stream = new MemoryStream(buffer);
                using var lz4Stream = LZ4Stream.Decode(stream);
                using var destinationStream = new MemoryStream();
                lz4Stream.CopyTo(destinationStream);
                stream.Dispose();
                buffer = destinationStream.ToArray();
            }
            else
            {
                buffer = DecompressData(buffer);
            }
        }
        else if (IsDataEncrypted)
        {
            buffer = Cryptography.Decrypt(buffer);
        }

        if (ProcessedSize > Size)
        {
            var finalData = new byte[Size];
            Array.Copy(buffer, 0, finalData, 0, Size);
            return finalData;
        }

        return buffer;
    }

    public static byte[] GetProcessedData(string uri, EbonyArchiveFileFlags flags, byte[] data, ushort key,
        bool isProtectedArchive, out EbonyArchiveFile file)
    {
        if (key == 0 && isProtectedArchive)
        {
            var hashCode = uri.GetHashCode();
            key = (ushort)((hashCode >> 16) ^ hashCode);
            if (key == 0)
            {
                key = 57005;
            }
        }

        if (!flags.HasFlag(EbonyArchiveFileFlags.Compressed) && !flags.HasFlag(EbonyArchiveFileFlags.Encrypted))
        {
            key = 0;
        }

        file = new EbonyArchiveFile
        {
            Flags = flags,
            Key = key
        };

        file.SetRawData(data);
        return file.GetDataForExport();
    }

    public static byte[] GetUnprocessedData(EbonyArchiveFileFlags flags, uint originalSize, ushort key, byte[] data)
    {
        var file = new EbonyArchiveFile
        {
            Flags = flags,
            Key = key
        };

        file.SetProcessedData(originalSize, data);
        return file.GetReadableData();
    }

    public byte[] GetDataForExport()
    {
        if (_dataSource == null)
        {
            return Array.Empty<byte>();
        }

        var buffer = _dataSource.GetData();

        if (!IsDataEncrypted && !IsDataCompressed)
        {
            Size = _dataSource.Size;
        }

        if (!(Flags.HasFlag(EbonyArchiveFileFlags.Encrypted) || Flags.HasFlag(EbonyArchiveFileFlags.Compressed)))
        {
            ProcessedSize = Size;
        }

        if (!IsDataEncrypted && Flags.HasFlag(EbonyArchiveFileFlags.Encrypted))
        {
            var encryptedData = Cryptography.Encrypt(buffer);
            ProcessedSize = (uint)encryptedData.Length;
            return encryptedData;
        }

        if (!IsDataCompressed && Flags.HasFlag(EbonyArchiveFileFlags.Compressed))
        {
            byte[] compressedData;

            if (Flags.HasFlag(EbonyArchiveFileFlags.LZ4Compression))
            {
                using var stream = new MemoryStream(buffer);
                using var destinationStream = new MemoryStream();
                var lz4Stream = LZ4Stream.Encode(destinationStream, LZ4Level.L12_MAX);
                stream.CopyTo(lz4Stream);
                lz4Stream.Dispose();
                compressedData = destinationStream.ToArray();
            }
            else
            {
                compressedData = CompressData(buffer);
            }

            if (compressedData.Length >= buffer.Length)
            {
                Flags &= ~EbonyArchiveFileFlags.Compressed;
                Key = 0;
                ProcessedSize = Size;
            }
            else
            {
                ProcessedSize = (uint)compressedData.Length;
                return compressedData;
            }
        }

        return buffer;
    }

    public byte[] GetRawData() => _dataSource.GetData();

    public void SetDataByFlags(byte[] data)
    {
        _dataSource = new MemoryDataSource(data);
        IsDataCompressed = Flags.HasFlag(EbonyArchiveFileFlags.Compressed);
        IsDataEncrypted = Flags.HasFlag(EbonyArchiveFileFlags.Encrypted);
    }

    public void SetProcessedData(uint originalSize, byte[] data)
    {
        _dataSource = new MemoryDataSource(data);
        Size = originalSize;
        ProcessedSize = (uint)data.Length;
    }

    public void SetRawData(byte[] data)
    {
        _dataSource = new MemoryDataSource(data);
        IsDataCompressed = false;
        IsDataEncrypted = false;
    }

    public void SetRawData(Stream stream)
    {
        _dataSource = new OpenFileDataSource(stream, DataOffset, ProcessedSize);
    }

    private EbonyArchiveFileFlags GetDefaultBinmodFlags()
    {
        var flags = EbonyArchiveFileFlags.Autoload;

        if (Uri.EndsWith(".modmeta") || Uri.EndsWith(".bin"))
        {
            flags |= EbonyArchiveFileFlags.MaskProtected;
        }
        else
        {
            flags |= EbonyArchiveFileFlags.Encrypted;
        }

        return flags;
    }

    private byte[] CompressData(byte[] data)
    {
        var currentPosition = 0;
        const int chunkSize = 128 * 1024;
        var encryptionKey = Key;
        using var dataStream = new MemoryStream(data);
        using var memoryStream = new MemoryStream();
        using var temporaryMemoryStream = new MemoryStream();

        while (currentPosition < dataStream.Length)
        {
            var sizeBefore = temporaryMemoryStream.Length;

            var compressionStream = new ZlibStream(temporaryMemoryStream, CompressionMode.Compress,
                CompressionLevel.BestCompression, true);

            var remainingBytes = dataStream.Length - currentPosition;
            var size = (int)(remainingBytes > chunkSize ? chunkSize : remainingBytes);
            var buffer = new byte[size];

            dataStream.Seek(currentPosition, SeekOrigin.Begin);
            dataStream.Read(buffer, 0, size);
            compressionStream.Write(buffer, 0, size);
            compressionStream.Dispose();

            var compressedSize = temporaryMemoryStream.Length - sizeBefore;

            if (encryptionKey != 0)
            {
                var key = (encryptionKey * 1103515245UL + 12345UL) * 1103515245UL + 12345UL;
                memoryStream.Write(BitConverter.GetBytes((int)compressedSize ^ (uint)(key >> 32)), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(size ^ (uint)key), 0, 4);
                encryptionKey = 0;
            }
            else
            {
                memoryStream.Write(BitConverter.GetBytes((int)compressedSize), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(size), 0, 4);
            }

            memoryStream.Write(temporaryMemoryStream.ToArray(),
                (int)(temporaryMemoryStream.Length - compressedSize),
                (int)compressedSize);

            var alignment = (int)(memoryStream.Length % 4);
            if (alignment > 0)
            {
                memoryStream.Write(BitConverter.GetBytes(0), 0, 4 - alignment);
            }

            currentPosition += size;
        }

        return memoryStream.ToArray();
    }

    private byte[] DecompressData(byte[] dataSourceBuffer)
    {
        const int chunkSize = 128 * 1024;
        var chunks = Size / chunkSize;

        // If the integer division wasn't even, add 1 more chunk
        if (Size % chunkSize != 0)
        {
            chunks++;
        }

        using var memoryStream = new MemoryStream(dataSourceBuffer);
        using var outStream = new MemoryStream();
        using var writer = new BinaryWriter(outStream);

        for (var index = 0; index < chunks; index++)
        {
            // Skip padding
            if (index > 0)
            {
                var offset = 4 - (int)(memoryStream.Position % 4);
                if (offset > 3)
                {
                    offset = 0;
                }

                memoryStream.Seek(offset, SeekOrigin.Current);
            }

            // Read the data sizes
            var buffer = new byte[4];
            memoryStream.ReadExactly(buffer, 0, 4);
            var compressedSize = BitConverter.ToUInt32(buffer);
            buffer = new byte[4];
            memoryStream.ReadExactly(buffer, 0, 4);
            var decompressedSize = BitConverter.ToUInt32(buffer);

            // Decompress the current chunk and write to the output stream
            buffer = new byte[compressedSize];
            memoryStream.ReadExactly(buffer, 0, (int)compressedSize);
            var decompressedBuffer = new byte[decompressedSize];

            using var stream = new MemoryStream(buffer);
            using var zlibStream = new ZlibStream(stream, CompressionMode.Decompress);
            zlibStream.ReadExactly(decompressedBuffer, 0, (int)decompressedSize);
            writer.Write(decompressedBuffer);
        }

        return outStream.ToArray();
    }

    private void FixRelativePath()
    {
        if (RelativePath.EndsWith(".tga") || RelativePath.EndsWith(".tif") || RelativePath.EndsWith(".dds") ||
            RelativePath.EndsWith(".png"))
        {
            RelativePath = RelativePath
                .Replace(".tga", ".btex")
                .Replace(".tif", ".btex")
                .Replace(".dds", ".btex")
                .Replace(".png", ".btex");
        }
        else if (RelativePath.EndsWith(".gmtl"))
        {
            RelativePath = RelativePath.Replace(".gmtl", ".gmtl.gfxbin");
        }
        else if (RelativePath.EndsWith(".gmdl"))
        {
            RelativePath = RelativePath.Replace(".gmdl", ".gmdl.gfxbin");
        }
        else if (RelativePath.EndsWith(".ebex"))
        {
            RelativePath = RelativePath.Replace(".ebex", ".exml");
        }
        else if (RelativePath.EndsWith(".ebex@"))
        {
            const string prefix = "$archives/";
            RelativePath = prefix + RelativePath.Replace(".ebex@", ".earc");
        }
        else if (RelativePath.EndsWith(".prefab@"))
        {
            RelativePath = RelativePath.Replace(".prefab@", ".earc");
        }
        else if (RelativePath.EndsWith(".htpk"))
        {
            RelativePath = RelativePath.Replace(".htpk", ".earc");
        }
        else if (RelativePath.EndsWith(".max"))
        {
            RelativePath = RelativePath.Replace(".max", ".win.mab");
        }
        else if (RelativePath.EndsWith(".sax"))
        {
            RelativePath = RelativePath.Replace(".sax", ".win.sab");
        }
    }

    public static byte[] GetReadableDataFromFragment(string uri, string relativePath, uint size, uint processedSize,
        EbonyArchiveFileFlags flags, ushort key, byte[] data)
    {
        var file = new EbonyArchiveFile(uri, relativePath, size, processedSize, flags, 0, 0, key);
        file.SetDataByFlags(data);
        return file.GetReadableData();
    }
}