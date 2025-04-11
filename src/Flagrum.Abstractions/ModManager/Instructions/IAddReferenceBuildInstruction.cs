using Flagrum.Abstractions.Archive;

namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents an instruction for the mod builder to add a reference entry to an <see cref="IEbonyArchive"/>.
/// </summary>
public interface IAddReferenceBuildInstruction : IPackedBuildInstruction;