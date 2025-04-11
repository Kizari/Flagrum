using System;
using System.Diagnostics.CodeAnalysis;
using Flagrum.Core.Archive;

namespace Flagrum.Research.Utilities;

/// <inheritdoc cref="IFileFinder"/>
/// <remarks>The file finder is in an unconfigured state.</remarks>
public interface INewFileFinder
{
    /// <summary>
    /// Configures the file finder to exclude certain file types from the search.<br/>
    /// This is useful to speed up the search since some files are never worth searching.
    /// </summary>
    /// <param name="exclusions">
    /// The extensions to exclude from the search.<br/>
    /// Defaults to <see cref="TypeFlags.BlacklistDefault"/> if left <c>null</c>.
    /// </param>
    /// <remarks>
    /// <see cref="TypeFlags.BlacklistDefault"/> is recommended unless you need to include those file types.<br/>
    /// <see cref="TypeFlags.BlacklistStrict"/> is recommended if you need to search scripts as well as other files.<br/>
    /// <see cref="TypeFlags.BlacklistRelaxed"/> is recommended if you also need to search model files.<br/>
    /// <see cref="TypeFlags.None"/> can be used if not wanting to filter any files.<br/>
    /// Flags can be manually combined if none of these presets are suitable.
    /// </remarks>
    IFilteredFileFinder WithBlacklist(TypeFlags exclusions = null);

    /// <summary>
    /// Configures the file finder to only include certain file types in the search.<br/>
    /// This is useful to speed up the search since only files of interest will be searched.
    /// </summary>
    /// <param name="whitelist">The file extensions to allow in the search.</param>
    /// <exception cref="ArgumentNullException">Throws if <b><paramref name="whitelist"/></b> is null.</exception>
    IFilteredFileFinder WithWhitelist([NotNull] TypeFlags whitelist);
}

/// <inheritdoc cref="IFileFinder"/>
/// <remarks>The file finder is currently configured with a file extension filter.</remarks>
public interface IFilteredFileFinder
{
    /// <summary>
    /// Configures the file finder to query against all files that match the configured whitelist/blacklist.
    /// </summary>
    IFileFinder QueryAll();
    
    /// <summary>
    /// Configures the file finder to only find files that contain a specific set of bytes.
    /// </summary>
    /// <param name="pattern">The byte pattern to check for.</param>
    /// <example>
    /// Find the fixid <c>123456</c> in all files.
    /// <code>
    /// builder.QueryBytePattern(BitConverter.GetBytes(123456u));
    /// </code>
    /// </example>
    IFileFinder QueryBytePattern(byte[] pattern);

    /// <summary>
    /// Configure the file finder to only find files that contain the given hexadecimal sequence.
    /// </summary>
    /// <param name="hex">The hexadecimal sequence to find.</param>
    IFileFinder QueryHexPattern(string hex);

    /// <summary>
    /// Configures the file finder to only find files that contain a specific text string.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <example>
    /// Find everywhere in all files that the URI "data://data/weapon/bin/weapon.win32.bins" is referenced.
    /// <code>
    /// builder.QueryText("data://data/weapon/bin/weapon.win32.bins");
    /// </code>
    /// </example>
    IFileFinder QueryText(string text);

    /// <summary>
    /// Configures the file finder to only find files that match a custom condition.
    /// </summary>
    /// <param name="predicate">
    /// The input parameter (<c>byte[]</c>) contains the decompressed data of the game file.
    /// The output parameter (<c>bool</c>) determines if the file matches the query (<c>true</c> to include the file).
    /// </param>
    /// <example>
    /// Find all materials in the game that use a certain texture.
    /// <code>
    /// builder.QueryCustom(file =>
    /// {
    ///     var material = new GameMaterial();
    ///     material.Read(file.GetReadableData());
    ///     return material.Textures.Any(t => t.Uri == "data://some/texture.tif");
    /// });
    /// </code>
    /// </example>
    IFileFinder QueryCustom(Func<IGameFile, bool> predicate);
}

/// <summary>
/// Finds files in the game directory based on certain conditions and acts on them accordingly.
/// </summary>
/// <remarks>
/// The file finder is fully configured and can now add optional configurations or execute the search.
/// </remarks>
public interface IFileFinder
{
    /// <summary>
    /// Configures the file finder to also search archive entries that are just reference entries with no data.
    /// </summary>
    IFileFinder IncludeReferences();

    /// <summary>
    /// Configures the file finder to also search loose files (those that aren't archived inside <c>earc</c> files).<br/>
    /// Note that loose files in this context will always have <see cref="IGameFile.Flags"/> set to
    /// <see cref="EbonyArchiveFileFlags.None"/>, so take this into account when configuring the queries.
    /// </summary>
    IFileFinder IncludeLooseFiles();

    /// <summary>
    /// Finds files by the configured query and executes the given action on each match.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void FindAndExecute(Action<IGameFile> action);

    /// <summary>
    /// Finds files by the configured query and dumps all matches' URIs to the console.
    /// </summary>
    void FindAndLog();
}