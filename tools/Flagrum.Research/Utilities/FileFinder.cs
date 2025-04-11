using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Abstractions.Archive;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using LinqKit;

namespace Flagrum.Research.Utilities;

/// <inheritdoc cref="IFileFinder" />
/// <remarks>The state of the file finder is determined by the interface each method returns.</remarks>
public class FileFinder : INewFileFinder, IFilteredFileFinder, IFileFinder
{
    private readonly ConcurrentDictionary<string, bool> _extensions = [];
    private readonly string _gameDirectory;
    private Func<IGameFile, bool> _filter;
    private Expression<Func<IGameFile, bool>> _filterExpression;
    private bool _includeLooseFiles;
    private bool _includeReferences;
    private Func<IGameFile, bool> _query;

    /// <summary>
    /// Constructor is private so the user is forced to use the static factory method to ensure the correct
    /// methods are called in the right order according to the implemented interfaces.
    /// </summary>
    private FileFinder(string gameDirectory) => _gameDirectory = gameDirectory;

    /// <inheritdoc />
    public IFileFinder IncludeReferences()
    {
        _includeReferences = true;
        return this;
    }

    /// <inheritdoc />
    public IFileFinder IncludeLooseFiles()
    {
        _includeLooseFiles = true;
        return this;
    }

    /// <inheritdoc />
    public void FindAndExecute(Action<IGameFile> action)
    {
        if (!_includeReferences)
        {
            // Filter out reference entries
            _filterExpression = _filterExpression.And(f => !f.Flags.HasFlag(EbonyArchiveFileFlags.Reference));
        }

        _filter = _filterExpression.Compile();

        System.Console.WriteLine("Starting search...");
        var watch = Stopwatch.StartNew();

        // Execute the search
        FindRecursively(_gameDirectory, action);

        watch.Stop();
        System.Console.WriteLine($"Search finished after {watch.ElapsedMilliseconds} milliseconds.");
    }

    /// <inheritdoc />
    public void FindAndLog()
    {
        FindAndExecute(file => { System.Console.WriteLine($"{file.Uri} ({file.Path})"); });
    }

    /// <inheritdoc />
    public IFileFinder QueryAll()
    {
        _query = _ => true;
        return this;
    }

    /// <inheritdoc />
    public IFileFinder QueryBytePattern(byte[] pattern)
    {
        _query = f =>
        {
            var data = f.GetReadableData();
            var searcher = new BoyerMooreSearch(pattern);
            return searcher.Search(data).Any();
        };

        return this;
    }

    /// <inheritdoc />
    public IFileFinder QueryHexPattern(string hex)
    {
        hex = hex.Replace(" ", "");

        if (hex.Length % 2 != 0)
        {
            throw new ArgumentException("The hex string must have an even number of hex characters", nameof(hex));
        }

        var hexBytes = new byte[hex.Length / 2];
        for (var i = 0; i < hex.Length; i += 2)
        {
            hexBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return QueryBytePattern(hexBytes);
    }

    /// <inheritdoc />
    public IFileFinder QueryText(string text) => QueryBytePattern(Encoding.UTF8.GetBytes(text));

    /// <inheritdoc />
    public IFileFinder QueryCustom(Func<IGameFile, bool> predicate)
    {
        _query = predicate;
        return this;
    }

    /// <inheritdoc />
    public IFilteredFileFinder WithBlacklist(TypeFlags exclusions = null)
    {
        exclusions ??= TypeFlags.BlacklistDefault;
        _filterExpression = f => !exclusions.HasFlag(f.TypeId);
        return this;
    }

    /// <inheritdoc />
    public IFilteredFileFinder WithWhitelist([NotNull] TypeFlags whitelist)
    {
        ArgumentNullException.ThrowIfNull(whitelist);
        _filterExpression = f => whitelist.HasFlag(f.TypeId);
        return this;
    }

    /// <summary>
    /// Creates a new unconfigured <see cref="FileFinder" />.
    /// </summary>
    /// <param name="gameDirectory">The absolute path of the directory that contains the game executable.</param>
    public static INewFileFinder Create(string gameDirectory) => new FileFinder(gameDirectory);

    /// <summary>
    /// Performs the file search recursively.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <param name="onMatch">The action to execute on matching files.</param>
    private void FindRecursively(string directory, Action<IGameFile> onMatch)
    {
        // Search all files in this directory
        Parallel.ForEach(Directory.EnumerateFiles(directory), path =>
        {
            if (path.EndsWith(".earc"))
            {
                using var archive = new EbonyArchive(path);
                foreach (var (_, file) in archive.Files)
                {
                    file.Path = path;

                    if (_filter(file))
                    {
                        if (_query(file))
                        {
                            onMatch(file);
                        }
                    }
                }
            }
            else if (_includeLooseFiles)
            {
                var file = new LooseGameFile(path);
                if (_filter(file) && _query(file))
                {
                    if (_query(file))
                    {
                        onMatch(file);
                    }
                }
            }
        });

        // Search all files in this directory's subdirectories (recursively)
        Parallel.ForEach(Directory.EnumerateDirectories(directory),
            subdirectory => { FindRecursively(subdirectory, onMatch); });
    }
}