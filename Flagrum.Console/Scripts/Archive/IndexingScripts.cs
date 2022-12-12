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
    private readonly SettingsService _settings = new();
    private readonly ConcurrentDictionary<ArchiveLocation, IEnumerable<(string relativePath, string uri)>> _assets = new();

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
            System.Console.WriteLine("\"" + relativeExtension + "\", ");
        }
    }

    private IEnumerable<(string relativePath, string uri)> GetRelativePaths()
    {
        MapDirectory(_settings.GameDataDirectory);
        Parallel.ForEach(Directory.EnumerateDirectories(_settings.GameDataDirectory), GenerateMapRecursively);
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
            using var unpacker = new Unpacker(file);
            var archive = new ArchiveLocation {Path = file.Replace(_settings.GameDataDirectory + "\\", "")};
            _assets.TryAdd(archive, unpacker.Files
                .Where(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference))
                .Select(f => (f.RelativePath, f.Uri)));
        }
    }
}