using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents a service that manages modals specific to the <see cref="IModEditor" />.
/// </summary>
public interface IModEditorModalService
{
    /// <summary>
    /// Steps the user through adding a file entry to the given archive.
    /// </summary>
    /// <param name="isReference">Whether the entry is a reference to a file rather than the file itself.</param>
    /// <param name="onUriSet">Callback to execute when the file URI is selected from the modal.</param>
    Task OpenAddFileModal(bool isReference, Func<string, Task> onUriSet);

    /// <summary>
    /// Steps the user through adding a mod project file reference to the given archive.
    /// </summary>
    /// <param name="project">The mod project to reference the file from.</param>
    /// <param name="archive">Archive to add the reference entry to.</param>
    /// <param name="flags">Flags that apply to the reference entry.</param>
    Task OpenAddSelfReferenceModal(
        IFlagrumProject project,
        IFlagrumProjectArchive archive,
        EbonyArchiveFileFlags flags);
}