namespace Flagrum.Abstractions.Archive;

[Flags]
public enum EbonyArchiveFileFlags : uint
{
    None = 0,
    Autoload = 1,
    Compressed = 2,
    Reference = 4,
    NoEarc = 8,
    Patched = 16,
    PatchedDeleted = 32,
    Encrypted = 64,
    MaskProtected = 128,
    LZ4Compression = 1342177280
}