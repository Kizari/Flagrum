using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents a modification to a game file archive.
/// </summary>
public interface IPackedBuildInstruction : IModBuildInstruction
{
    /// <summary>
    /// URI associated with this modification. Typically, represents a file path or a reference to another file.
    /// </summary>
    string Uri { get; set; }
    
    /// <summary>
    /// Settings to apply to this entry when packed into the target archive.
    /// </summary>
    EbonyArchiveFileFlags Flags { get; set; }

    /// <summary>
    /// Any operations that need to be performed on the archive should be performed before this method returns
    /// </summary>
    void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive);

    /// <summary>
    /// Any operations that need to be performed on the archive should be performed before this method returns
    /// </summary>
    void Revert(IEbonyArchive archive, IFlagrumProjectArchive projectArchive);
}