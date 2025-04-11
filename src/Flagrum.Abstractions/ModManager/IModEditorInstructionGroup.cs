using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents a group of modification instructions within the mod editor.
/// </summary>
public interface IModEditorInstructionGroup
{
    /// <summary>
    /// Display name that identifies the group to the user.
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Game file archive associated with this instruction group.
    /// </summary>
    /// <remarks>
    /// <c>null</c> if <see cref="Type" /> is <b>not</b> <see cref="ModEditorInstructionGroupType.Archive" />.
    /// </remarks>
    IFlagrumProjectArchive? Archive { get; set; }

    /// <summary>
    /// Type of instructions contained within this group.
    /// </summary>
    ModEditorInstructionGroupType Type { get; set; }

    /// <summary>
    /// Whether the instruction group is expanded within the application UI.
    /// </summary>
    bool IsExpanded { get; set; }

    /// <summary>
    /// The instructions represented by this group.
    /// </summary>
    IEnumerable<IModBuildInstruction> Instructions { get; set; }
}