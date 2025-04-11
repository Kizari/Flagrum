using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents a factory that creates instances of <see cref="IModEditorInstructionGroup" />.
/// </summary>
public interface IModEditorInstructionGroupFactory
{
    /// <summary>
    /// Creates a new instance of an <see cref="IModEditorInstructionGroup" />.
    /// </summary>
    /// <param name="type">Type of the mod instruction group.</param>
    /// <param name="name">Display name that identifies the group to the user.</param>
    /// <param name="archive">Archive associated with the group.</param>
    /// <param name="isExpanded">Whether the associated UI element is expanded.</param>
    IModEditorInstructionGroup Create(
        ModEditorInstructionGroupType type,
        string name,
        IFlagrumProjectArchive archive,
        bool isExpanded);
}