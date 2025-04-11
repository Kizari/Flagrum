using Flagrum.Abstractions.ModManager.Instructions;

namespace Flagrum.Abstractions.ModManager.Project;

/// <summary>
/// Represents a mod project for Flagrum's mod manager.
/// </summary>
public interface IFlagrumProject
{
    /// <summary>
    /// Unique identifier for this project.
    /// </summary>
    Guid Identifier { get; set; }

    /// <summary>
    /// Settings that apply to this project.
    /// </summary>
    ModFlags Flags { get; set; }

    /// <summary>
    /// Human-readable display name for this mod project.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Name(s) of the author(s) of the mod.
    /// </summary>
    string Author { get; set; }

    /// <summary>
    /// Brief description of the mod.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Large HTML string containing detailed information about the mod.
    /// </summary>
    string Readme { get; set; }

    /// <summary>
    /// Game file archives that will be modified.
    /// </summary>
    List<IFlagrumProjectArchive> Archives { get; }

    /// <summary>
    /// Modifications that do not explicitly target game file archives.
    /// </summary>
    List<IModBuildInstruction> Instructions { get; set; }

    /// <summary>
    /// Retrieves all build instructions from the project (both global instructions,
    /// and those associated with archives).
    /// </summary>
    /// <remarks>
    /// Essentially just a combination of <see cref="Instructions" /> and the instructions in <see cref="Archives" />.
    /// </remarks>
    IEnumerable<IModBuildInstruction> AllInstructions { get; }

    /// <summary>
    /// Whether the source files for the mod project have changed since the last mod build.
    /// </summary>
    bool HaveFilesChanged { get; }

    /// <summary>
    /// Causes this project to update the timestamps of the source files to their most recent write time.
    /// </summary>
    void UpdateLastModifiedTimestamps();

    /// <summary>
    /// Whether the build <see cref="Instructions" /> contain a loose file represented by the given file path.
    /// </summary>
    /// <param name="relativePath">Path to the loose file, relative to the game data directory.</param>
    /// <returns><c>true</c> if the project contains the file, otherwise <c>false</c>.</returns>
    bool ContainsLooseFile(string relativePath);

    /// <summary>
    /// Saves the project to disk.
    /// </summary>
    /// <param name="path">Path to the file on disk.</param>
    Task Save(string path);

    /// <summary>
    /// Checks for invalid URIs in any reference archive entries.
    /// </summary>
    bool AreReferencesValid();

    /// <summary>
    /// Gets a list of files that are part of this project but are no longer present in the file system.
    /// </summary>
    List<string> GetDeadFiles();
}