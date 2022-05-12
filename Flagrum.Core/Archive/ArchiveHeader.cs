using System;
using System.Text;

namespace Flagrum.Core.Archive;

[Flags]
public enum ArchiveHeaderFlags : uint
{
    HasLooseData = 1,
    HasLocaleData = 2,
    DebugArchive = 4,
    Copyguard = 8
}

public class ArchiveHeader
{
    public const uint ArchiveVersion = 196628;
    public const uint ProtectedArchiveVersion = 2147483648;
    public const uint Size = 64;
    public const uint DefaultChunkSize = 128;
    public const ulong CopyguardHash = 10026789885951819402;

    public ArchiveHeader()
    {
        Version = ProtectedArchiveVersion | ArchiveVersion;
    }

    public static byte[] DefaultTag { get; } = Encoding.UTF8.GetBytes("CRAF");

    public byte[] Tag { get; set; }
    public uint Version { get; set; }
    public uint FileCount { get; set; }
    public uint BlockSize { get; set; }
    public uint FileHeadersOffset { get; set; }
    public uint UriListOffset { get; set; }
    public uint PathListOffset { get; set; }
    public uint DataOffset { get; set; }
    public ArchiveHeaderFlags Flags { get; set; }
    public uint ChunkSize { get; set; }
    public ulong Hash { get; set; }

    public bool IsProtectedArchive => (Version & 2147483648) > 0;
}