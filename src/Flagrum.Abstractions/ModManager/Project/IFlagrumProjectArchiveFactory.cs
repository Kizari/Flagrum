using Flagrum.Abstractions.Archive;

namespace Flagrum.Abstractions.ModManager.Project;

/// <summary>
/// Represents a factory used to create instances of <see cref="IFlagrumProjectArchive" />.
/// </summary>
public interface IFlagrumProjectArchiveFactory
{
    /// <summary>
    /// Creates a new <see cref="IFlagrumProjectArchive" /> with no build instructions.
    /// </summary>
    /// <param name="type">Type of modification to make to the archive this object represents.</param>
    /// <param name="relativePath">Relative path of the archive this object represents.</param>
    /// <param name="flags">Flags to apply to the modified archive.</param>
    IFlagrumProjectArchive Create(ModChangeType type, string relativePath, EbonyArchiveFlags flags);
}