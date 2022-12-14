using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flagrum.Web.Services;

public class UriMapper
{
    private readonly FlagrumDbContext _context;
    private readonly SettingsService _settings;
    private ConcurrentDictionary<ArchiveLocation, IEnumerable<string>> _assets;
    private bool _usePs4Mode;

    public UriMapper(
        FlagrumDbContext context,
        SettingsService settings)
    {
        _context = context;
        _settings = settings;
    }

    public void UsePs4Mode()
    {
        _usePs4Mode = true;
    }

    public void RegenerateMap()
    {
        var start = DateTime.Now;

        _context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_context.AssetExplorerNodes)};");
        _context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_context.ArchiveLocations)};");
        _context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_context.AssetUris)};");
        _context.Database.ExecuteSqlRaw($"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(_context.AssetExplorerNodes)}';");
        _context.Database.ExecuteSqlRaw($"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(_context.ArchiveLocations)}';");
        _context.Database.ExecuteSqlRaw($"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(_context.AssetUris)}';");
        
        _assets = new ConcurrentDictionary<ArchiveLocation, IEnumerable<string>>();
        var assetUris = new Dictionary<string, AssetUri>();
        var allUris = new Dictionary<string, ArchiveLocation>();

        MapDirectory(_settings.GameDataDirectory);
        Parallel.ForEach(Directory.EnumerateDirectories(_settings.GameDataDirectory), GenerateMapRecursively);

        var root = new AssetExplorerNode
        {
            Name = "data://"
        };

        foreach (var (archive, uris) in _assets)
        {
            foreach (var uri in uris)
            {
                if (uri.StartsWith("CRAF"))
                {
                    continue;
                }

                var fullUri = "data://" + uri;
                allUris.TryAdd(fullUri, archive);

                var earcDirectory = Path.GetDirectoryName(archive.Path)!;
                var uriReplaced = Path.GetDirectoryName(uri.Replace('/', '\\'));
                if (earcDirectory.Equals(uriReplaced, StringComparison.OrdinalIgnoreCase))
                {
                    assetUris.TryAdd(fullUri, new AssetUri
                    {
                        ArchiveLocation = archive,
                        Uri = fullUri
                    });
                }

                var tokens = uri.Split('/');
                var currentNode = root;
                foreach (var token in tokens)
                {
                    var subdirectory = currentNode.ChildNodes
                        .FirstOrDefault(c => c.Name == token);

                    if (subdirectory == null)
                    {
                        subdirectory = new AssetExplorerNode
                        {
                            Name = token,
                            Parent = currentNode
                        };

                        currentNode.ChildNodes.Add(subdirectory);
                    }

                    currentNode = subdirectory;
                }
            }
        }

        foreach (var (uri, archiveLocation) in allUris)
        {
            if (!assetUris.ContainsKey(uri))
            {
                assetUris.TryAdd(uri, new AssetUri
                {
                    ArchiveLocation = archiveLocation,
                    Uri = uri
                });
            }
        }

        _context.Add(root);
        _context.AddRange(assetUris.Select(kvp => kvp.Value));
        _context.SaveChanges();
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
                .Select(f => f.Uri.Replace("data://", "")));
        }
    }
}