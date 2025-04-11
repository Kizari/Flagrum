namespace Flagrum.Abstractions.ModManager.Project;

/// <summary>
/// Represents the type of modification that applies to a <see cref="IFlagrumProjectArchive" />.
/// </summary>
public enum ModChangeType
{
    /// <summary>
    /// The modification applies to an existing archive and changes it.
    /// </summary>
    Change,

    /// <summary>
    /// The modification adds a new archive to the game.
    /// </summary>
    Create
}