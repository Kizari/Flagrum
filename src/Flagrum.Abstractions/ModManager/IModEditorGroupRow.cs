namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents a UI entry for an instruction group within the <see cref="IModEditor" />.
/// </summary>
public interface IModEditorGroupRow
{
    /// <summary>
    /// Instruction group that this row represents.
    /// </summary>
    IModEditorInstructionGroup Group { get; set; }
}