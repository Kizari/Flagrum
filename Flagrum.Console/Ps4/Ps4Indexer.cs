using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Web.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Console.Ps4;

public class Ps4Indexer
{
    private readonly ConcurrentDictionary<Ps4ArchiveLocation, IEnumerable<string>> _assets = new();
    private readonly Dictionary<string, Ps4AssetUri> _uris = new();

    public void RegenerateMap()
    {
        using var context = Ps4Utilities.NewContext();
        context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(context.Ps4AssetUris)};");
        context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(context.Ps4ArchiveLocations)};");
        context.Database.ExecuteSqlRaw(
            $"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(context.Ps4AssetUris)}';");
        context.Database.ExecuteSqlRaw(
            $"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(context.Ps4ArchiveLocations)}';");

        MapDirectory(Ps4Constants.DatasDirectory);
        Parallel.ForEach(Directory.EnumerateDirectories(Ps4Constants.DatasDirectory), GenerateMapRecursively);

        var total = _assets.Count;
        var orderedAssets = _assets
            .OrderByDescending(a => a.Key.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch2_initial"))
            .ThenByDescending(a => a.Key.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch1_initial"))
            .ThenByDescending(a => a.Key.Path.Contains(@"FFXV_Patch\patch2_initial"))
            .ThenByDescending(a => a.Key.Path.Contains(@"FFXV_Patch\patch1_initial"))
            .ToList();

        for (var i = 0; i < total; i++)
        {
            var (archive, uris) = orderedAssets[i];
            System.Console.WriteLine($"Processing file {i + 1}/{total}");

            foreach (var uri in uris)
            {
                _uris.TryGetValue(uri, out var assetUri);
                if (assetUri == null)
                {
                    assetUri = new Ps4AssetUri {Uri = uri};
                    _uris.Add(uri, assetUri);
                }

                context.Add(new Ps4ArchiveAsset
                {
                    Ps4ArchiveLocation = archive,
                    Ps4AssetUri = assetUri
                });
            }
        }

        context.SaveChanges();
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
            var archive = new Ps4ArchiveLocation
            {
                Path = file.Replace(Ps4Constants.DatasDirectory + "\\", "")
            };

            _assets.TryAdd(archive, unpacker.Files
                .Where(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference))
                .Select(f => f.Uri));
        }
    }
}