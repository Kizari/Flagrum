using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Abstractions.ModManager;

/// <summary>
/// Represents the context menu that displays when right-clicking a mod card in the mod manager.
/// </summary>
public interface IModCardContextMenu
{
    /// <summary>
    /// Gets the mod associated with this context menu.
    /// </summary>
    IFlagrumProject GetContextMod();

    /// <summary>
    /// Refreshes the state of the mod library.
    /// </summary>
    /// <remarks>
    /// Useful to call if a context action alters the mod library or any of the mod cards in some way.
    /// </remarks>
    void RefreshModLibrary();

    /// <summary>
    /// Creates a deep clone of an <see cref="IFlagrumProject" />.
    /// </summary>
    /// <param name="original">The project to clone.</param>
    IFlagrumProject CloneProject(IFlagrumProject original);

    /// <summary>
    /// Adds a <see cref="IFlagrumProject" /> to the application state if it has not already been added.
    /// </summary>
    void AddProject(IFlagrumProject project);
}