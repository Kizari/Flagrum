using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents an editor that can make changes to an <see cref="IFlagrumProject" />.
/// </summary>
public interface IModEditor
{
    /// <summary>
    /// Mod project that is currently active in the editor.
    /// </summary>
    public IFlagrumProject Mod { get; set; }

    /// <summary>
    /// Grouped mod instructions for the project that is active in the editor.
    /// </summary>
    List<IModEditorInstructionGroup> InstructionGroups { get; }

    /// <summary>
    /// Steps the user through creating a modification that replaces a game file with a new file.
    /// </summary>
    void OpenReplaceModal();

    /// <summary>
    /// Steps the user through creating a modificaton that removes an existing game file.
    /// </summary>
    void OpenRemoveModal();

    /// <summary>
    /// Marks the mod as changed and updates the state of the editor.
    /// </summary>
    Task OnModChangedAsync();

    /// <summary>
    /// Adds an archive to the mod project if it is not already added.
    /// </summary>
    /// <param name="uri">URI of the asset whose containing archive is to be added to the project.</param>
    /// <returns>Reference to the archive associated with the target asset.</returns>
    /// <remarks>
    /// Regardless of whether the archive already exists or not, the UI for it will be expanded.
    /// </remarks>
    IFlagrumProjectArchive TryAddArchiveByAsset(string uri);

    /// <summary>
    /// Opens the URI selection modal.
    /// </summary>
    /// <param name="callback">Action to execute when the URI is selected.</param>
    void OpenUriSelectModal(Func<string, Task> callback);

    /// <summary>
    /// Closes the URI selection modal.
    /// </summary>
    Task CloseUriSelectModalAsync();
}