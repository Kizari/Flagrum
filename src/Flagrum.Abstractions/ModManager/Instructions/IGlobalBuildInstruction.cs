namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents a mod builder instruction that applies globally to the game.
/// </summary>
public interface IGlobalBuildInstruction : IModBuildInstruction
{
    /// <summary>
    /// Display name to group this instruction under in the mod editor interface.
    /// </summary>
    string InstructionGroupName { get; }
}