using Flagrum.Abstractions.Archive;

namespace Flagrum.Core.Archive;

// TODO: Refactor this so these properties are part of EbonyArchive instead
public class EbonyArchiveHeader
{
    public const uint ArchiveVersion = 196628;
    public const uint ProtectedArchiveVersion = 2147483648;
    private const uint DefaultChunkSize = 128;
    private const uint DefaultBlockSize = 512;

    public const uint Size = 64;
    public const ulong CopyguardHash = 10026789885951819402;
    public const uint ProtectVersionHash = 2147483648;
    public const uint MagicValue = 0x46415243;

    public uint Magic { get; set; }
    public uint Version { get; set; }
    public ushort VersionMajor { get; set; }
    public ushort VersionMinor { get; set; }
    public uint FileCount { get; set; }
    public uint BlockSize { get; set; } = DefaultBlockSize;
    public uint FileHeadersOffset { get; set; }
    public uint UriListOffset { get; set; }
    public uint PathListOffset { get; set; }
    public uint DataOffset { get; set; }
    public EbonyArchiveFlags Flags { get; set; }
    public uint ChunkSize { get; set; } = DefaultChunkSize;
    public ulong Hash { get; set; }

    public bool IsProtectedArchive => (Version & ProtectVersionHash) > 0;
}