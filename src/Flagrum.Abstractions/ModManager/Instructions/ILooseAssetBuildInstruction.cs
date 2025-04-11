namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents a loose file in the game files.
/// </summary>
public interface ILooseAssetBuildInstruction : IModBuildInstruction
{
    /// <summary>
    /// Path to where the asset is to be stored in the game data directory.
    /// </summary>
    string RelativePath { get; set; }

    /// <summary>
    /// Path to the original mod file on disk.
    /// </summary>
    /// <remarks>
    /// This is the file that will be copied to the location represented by <see cref="RelativePath" />
    /// when the mod builder installs the mod.
    /// </remarks>
    string FilePath { get; set; }
}