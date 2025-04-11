namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents a modification to the game.
/// </summary>
public interface IModBuildInstruction
{
    /// <summary>
    /// Whether the instruction should be visible to the user in the mod editor.
    /// </summary>
    bool ShouldShowInBuildList { get; }

    /// <summary>
    /// String to compare against when filtering the mod editor build list by a search term.
    /// </summary>
    string FilterableName { get; }
}