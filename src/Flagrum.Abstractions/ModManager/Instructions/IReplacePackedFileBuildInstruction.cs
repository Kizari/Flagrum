using Flagrum.Abstractions.Archive;

namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents an instruction for the mod builder to replace an existing file in an <see cref="IEbonyArchive"/>
/// with a mod file on disk.
/// </summary>
public interface IReplacePackedFileBuildInstruction : IPackedAssetBuildInstruction;