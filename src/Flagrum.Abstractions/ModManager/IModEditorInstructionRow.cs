using Flagrum.Abstractions.ModManager.Instructions;

namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents a single instruction in the mod editor build list UI.
/// </summary>
public interface IModEditorInstructionRow
{
    /// <summary>
    /// The buid instruction represented by this UI element.
    /// </summary>
    IModBuildInstruction Instruction { get; set; }
}