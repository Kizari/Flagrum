using Flagrum.Abstractions.Archive;

namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents an instruction for the mod builder to add a file to an <see cref="IEbonyArchive"/>.
/// </summary>
public interface IAddPackedFileBuildInstruction : IPackedAssetBuildInstruction;