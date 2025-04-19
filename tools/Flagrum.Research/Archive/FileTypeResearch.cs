using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Archive;
using Flagrum.Research.Utilities;

namespace Flagrum.Research.Archive;

/// <summary>
/// Methods that perform research tasks related to the file extensions of game files.
/// </summary>
public class FileTypeResearch
{
    /// <summary>
    /// Prints a list of all unique file extensions found for files in the game directory, recursively.
    /// </summary>
    /// <param name="dataDirectory">
    /// Directory that contains game data.
    /// Typically, the folder named "datas" adjacent to the executable.
    /// </param>
    public void PrintLooseFileExtensions(string dataDirectory)
    {
        foreach (var extension in Directory
                     .GetFiles(dataDirectory, "*.*", SearchOption.AllDirectories)
                     .Select(path => path[(path.IndexOf('.') + 1)..])
                     .Distinct()
                     .OrderBy(e => e))
        {
            Console.WriteLine(extension);
        }
    }
    
    /// <summary>
    /// Creates a map of file extensions from the given game data directories.
    /// </summary>
    /// <param name="gameDirectories">
    /// The directories that contains the game executable for each build that file extensions are to be mapped from.
    /// </param>
    public void CreateExtensionMap(params string[] gameDirectories)
    {
        // Create empty collections
        var uriToPathExtensionMap = new Dictionary<string, string>();
        var pathExtensionMap = new Dictionary<string, string>();
        var pathExtensionList = new List<string>();

        // Populate collections from each game directory in the list
        foreach (var directory in gameDirectories)
        {
            Console.WriteLine($"Mapping {directory}...");
            CreateExtensionMap(directory, uriToPathExtensionMap, pathExtensionMap, pathExtensionList);
            Console.WriteLine();
        }
        
        // Write uriToPathExtensionMap out to a C# file
        var builder = new StringBuilder()
            .AppendLine("    private readonly Dictionary<string, string> _uriToPathExtensionMap = new()")
            .AppendLine("    {");

        foreach (var (uri, path) in uriToPathExtensionMap.OrderBy(kvp => kvp.Key))
        {
            builder.AppendLine($"        {{\"{uri}\", \"{path}\"}},");
        }

        builder
            .Remove(builder.Length - 3, 3) // Remove comma, carriage return, and line break from final entry
            .AppendLine()
            .AppendLine("    };");
        
        File.WriteAllText(@"C:\Users\Kieran\Downloads\uriToPathMap.cs", builder.ToString());
        
        // Write pathExtensionMap out to a C# file
        builder = new StringBuilder()
            .AppendLine("    private readonly Dictionary<string, string> _map = new()")
            .AppendLine("    {");

        foreach (var (uri, path) in pathExtensionMap.OrderBy(kvp => kvp.Key))
        {
            builder.AppendLine($"        {{\"{uri}\", \"{path}\"}},");
        }

        builder
            .Remove(builder.Length - 3, 3) // Remove comma, carriage return, and line break from final entry
            .AppendLine()
            .AppendLine("    };");
        
        File.WriteAllText(@"C:\Users\Kieran\Downloads\extensionMap.cs", builder.ToString());
        
        // Write pathExtensionList out to a C# file
        builder = new StringBuilder()
            .AppendLine("            return")
            .AppendLine("            [");

        foreach (var path in pathExtensionList.OrderBy(o => o))
        {
            builder.AppendLine($"                \"{path}\",");
        }
        
        builder
            .Remove(builder.Length - 3, 3) // Remove comma, carriage return, and line break from final entry
            .AppendLine()
            .AppendLine("            ];");
        
        File.WriteAllText(@"C:\Users\Kieran\Downloads\extensionList.cs", builder.ToString());
    }
    
    /// <summary>
    /// Creates a map of file extensions from the given game data directory.
    /// </summary>
    /// <param name="gameDirectory">The directory that contains the game executable.</param>
    /// <param name="uriToPathExtensionMap">
    /// Dictionary that maps URI extensions to path extensions.
    /// Includes keys even if the value is the same as the key.
    /// </param>
    /// <param name="pathExtensionMap">
    /// Dictionary that maps URI extensions to path extensions.
    /// Does not include keys where the value is the same as the key.
    /// </param>
    /// <param name="pathExtensionList">List of all unique file extensions in the game.</param>
    private void CreateExtensionMap(
        string gameDirectory,
        Dictionary<string, string> uriToPathExtensionMap,
        Dictionary<string, string> pathExtensionMap,
        List<string> pathExtensionList)
    {
        // Map out the file extensions
        var map = new ConcurrentDictionary<string, string>();
        FileFinder.Create(gameDirectory)
            .WithBlacklist(TypeFlags.None)
            .QueryAll()
            .FindAndExecute(file =>
            {
                if (file is EbonyArchiveFile archiveFile)
                {
                    var uriExtension = file.Uri[(file.Uri.IndexOf('.') + 1)..];
                    var pathExtension = archiveFile.RelativePath[(archiveFile.RelativePath.IndexOf('.') + 1)..];
                    if (map.TryGetValue(uriExtension, out var other) && other != pathExtension)
                    {
                        // Warn the user if there are multiple path extensions for the same URI extension
                        Console.WriteLine(
                            $"ERROR: Multiple extensions found for {uriExtension} ({pathExtension}, {other})");
                    }
                    else
                    {
                        map[uriExtension] = pathExtension;
                    }
                }
            });
        
        // Check for any conflicts with the merged uriToPathExtensionMap
        foreach (var (uri, path) in map)
        {
            if (uriToPathExtensionMap.TryGetValue(uri, out var result) && result != path)
            {
                Console.WriteLine($"ERROR (uri->path): Extension \"{path}\" conflicts with \"{result}\" for extension \"{uri}\".");
            }
        }

        // Check for any conflicts with the merged pathExtensionMap
        foreach (var (uri, path) in map)
        {
            if (pathExtensionMap.TryGetValue(uri, out var result) && result != path)
            {
                Console.WriteLine($"ERROR (relative): Extension \"{path}\" conflicts with \"{result}\" for extension \"{uri}\".");
            }
        }
        
        // Add new records to each collection
        foreach (var (uri, path) in map)
        {
            uriToPathExtensionMap[uri] = path;
            
            if (uri != path)
            {
                pathExtensionMap[uri] = path;
            }

            if (!pathExtensionList.Contains(path))
            {
                pathExtensionList.Add(path);
            }
        }
    }
}