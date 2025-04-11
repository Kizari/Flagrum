using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;

namespace Flagrum.Abstractions.ModManager.Project;

/// <summary>
/// Represents a modification to an EbonyArchive (.earc) file.
/// </summary>
public interface IFlagrumProjectArchive
{
    /// <summary>
    /// Type of modification that applies to the target archive.
    /// </summary>
    ModChangeType Type { get; set; }

    /// <summary>
    /// Path to where the target archive resides in the game files, relative to the "datas" folder.
    /// </summary>
    string RelativePath { get; set; }

    /// <summary>
    /// Flags to apply to the modified archive.
    /// </summary>
    EbonyArchiveFlags Flags { get; set; }

    /// <summary>
    /// Modifications to apply to the target archive.
    /// </summary>
    List<IPackedBuildInstruction> Instructions { get; set; }
}