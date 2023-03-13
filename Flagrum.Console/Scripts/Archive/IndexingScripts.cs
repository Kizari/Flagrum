using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;

namespace Flagrum.Console.Scripts.Archive;

public class IndexingScripts
{
    private readonly ConcurrentDictionary<ArchiveLocation, IEnumerable<(string relativePath, string uri)>> _assets =
        new();

    private readonly ProfileService _profile = new();

    public static void DumpDifferingExtensions()
    {
        var script = new IndexingScripts();
        var relativePaths = script.GetRelativePaths().ToList();

        var extensions = relativePaths.Select(u =>
            {
                var fileName = u.relativePath.Split('\\').Last();
                var relativeExtension = fileName[fileName.IndexOf('.')..];
                fileName = u.uri.Split('/').Last();
                var extension = fileName[fileName.IndexOf('.')..];
                return (relativeExtension, extension);
            })
            .DistinctBy(e => e.extension)
            .ToList();

        foreach (var (relativeExtension, extension) in extensions
                     .OrderBy(e => e.relativeExtension)
                     .Where(e => e.extension != e.relativeExtension))
        {
            System.Console.WriteLine("{\"" + extension[1..] + "\", \"" + relativeExtension[1..] + "\"},");
        }
    }
    
    public static void DumpUniqueRelativeExtensions()
    {
        var script = new IndexingScripts();
        var relativePaths = script.GetRelativePaths().ToList();

        var extensions = relativePaths.Select(u =>
            {
                var fileName = u.relativePath.Split('\\').Last();
                var relativeExtension = fileName[fileName.IndexOf('.')..];
                fileName = u.uri.Split('/').Last();
                var extension = fileName[fileName.IndexOf('.')..];
                return (relativeExtension, extension);
            })
            .DistinctBy(e => e.relativeExtension)
            .ToList();

        foreach (var (relativeExtension, extension) in extensions
                     .OrderBy(e => e.relativeExtension))
        {
            System.Console.WriteLine("\"" + relativeExtension[1..] + "\",");
        }
    }

    public static void DumpUniqueUriExtensions()
    {
        var script = new IndexingScripts();
        var relativePaths = script.GetRelativePaths().ToList();

        var extensions = relativePaths.Select(u =>
            {
                var fileName = u.uri.Split('/').Last();
                var extension = fileName[fileName.IndexOf('.')..];
                return extension;
            })
            .DistinctBy(e => e)
            .ToList();

        foreach (var extension in extensions
                     .OrderBy(e => e))
        {
            System.Console.WriteLine($"\"{extension[1..]}\",");
        }
    }

    private IEnumerable<(string relativePath, string uri)> GetRelativePaths()
    {
        MapDirectory(_profile.GameDataDirectory);
        Parallel.ForEach(Directory.EnumerateDirectories(_profile.GameDataDirectory), GenerateMapRecursively);
        return _assets.SelectMany(kvp => kvp.Value);
    }

    private void GenerateMapRecursively(string directory)
    {
        MapDirectory(directory);
        Parallel.ForEach(Directory.EnumerateDirectories(directory), GenerateMapRecursively);
    }

    private void MapDirectory(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.earc"))
        {
            using var unpacker = new EbonyArchive(file);
            var archive = new ArchiveLocation {Path = file.Replace(_profile.GameDataDirectory + "\\", "")};
            _assets.TryAdd(archive, unpacker.Files
                .Where(f => !f.Value.Flags.HasFlag(ArchiveFileFlag.Reference))
                .Select(f => (f.Value.RelativePath, f.Value.Uri)));
        }
    }
}