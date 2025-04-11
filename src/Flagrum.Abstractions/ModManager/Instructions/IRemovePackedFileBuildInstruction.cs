using Flagrum.Abstractions.Archive;

namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents an instruction for the mod builder to remove an entry from an <see cref="IEbonyArchive"/>.
/// </summary>
public interface IRemovePackedFileBuildInstruction : IPackedBuildInstruction;