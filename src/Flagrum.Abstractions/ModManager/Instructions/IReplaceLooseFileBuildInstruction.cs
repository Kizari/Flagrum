namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents an instruction for the mod builder to replace a file in the game data directory with a mod file from disk.
/// </summary>
public interface IReplaceLooseFileBuildInstruction : ILooseAssetBuildInstruction;