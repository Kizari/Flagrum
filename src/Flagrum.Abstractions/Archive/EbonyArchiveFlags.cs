namespace Flagrum.Abstractions.Archive;

[Flags]
public enum EbonyArchiveFlags : uint
{
    None = 0,
    HasLooseData = 1,
    HasLocaleData = 2,
    DebugArchive = 4,
    Copyguard = 8,
    FlagrumModArchive = 16
}