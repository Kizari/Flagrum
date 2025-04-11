using Flagrum.Abstractions.Archive;

namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents a modification to an asset inside an <see cref="IEbonyArchive" />.
/// </summary>
public interface IPackedAssetBuildInstruction : IPackedBuildInstruction
{
    /// <summary>
    /// Path to the mod file on disk.
    /// </summary>
    string FilePath { get; set; }
}