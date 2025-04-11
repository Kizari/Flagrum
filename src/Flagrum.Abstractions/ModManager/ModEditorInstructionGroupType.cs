using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Type of instructions contained within a <see cref="IModEditorInstructionGroup" />.
/// </summary>
public enum ModEditorInstructionGroupType
{
    /// <summary>
    /// Instructions apply globally.
    /// </summary>
    None,

    /// <summary>
    /// Instructions apply to a <see cref="IFlagrumProjectArchive" />.
    /// </summary>
    Archive
}