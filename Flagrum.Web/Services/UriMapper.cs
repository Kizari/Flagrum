using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Services;

public class UriMapper
{
    private readonly FlagrumDbContext _context;
    private readonly ProfileService _profile;
    private ConcurrentDictionary<ArchiveLocation, IEnumerable<string>> _assets;

    public UriMapper(
        FlagrumDbContext context,
        ProfileService profile)
    {
        _context = context;
        _profile = profile;
    }

    public void RegenerateMap()
    {
        _context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_context.AssetExplorerNodes)};");
        _context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_context.ArchiveLocations)};");
        _context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_context.AssetUris)};");
        _context.Database.ExecuteSqlRaw(
            $"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(_context.AssetExplorerNodes)}';");
        _context.Database.ExecuteSqlRaw(
            $"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(_context.ArchiveLocations)}';");
        _context.Database.ExecuteSqlRaw($"DELETE FROM SQLITE_SEQUENCE WHERE name='{nameof(_context.AssetUris)}';");

        _assets = new ConcurrentDictionary<ArchiveLocation, IEnumerable<string>>();
        var assetUris = new Dictionary<string, AssetUri>();
        var allUris = new Dictionary<string, ArchiveLocation>();

        MapDirectory(_profile.GameDataDirectory);
        Parallel.ForEach(Directory.EnumerateDirectories(_profile.GameDataDirectory), GenerateMapRecursively);

        var root = new AssetExplorerNode
        {
            Name = ""
        };

        foreach (var (archive, uris) in _assets)
        {
            foreach (var uri in uris)
            {
                allUris.TryAdd(uri, archive);

                var earcDirectory = Path.GetDirectoryName(archive.Path)!;
                var uriReplaced = Path.GetDirectoryName(uri.Replace('/', '\\'))!;
                uriReplaced = uriReplaced[(uriReplaced.IndexOf(':') + 1)..];    // Remove prefixes like data:\
                if (earcDirectory.Equals(uriReplaced, StringComparison.OrdinalIgnoreCase))
                {
                    assetUris.TryAdd(uri, new AssetUri
                    {
                        ArchiveLocation = archive,
                        Uri = uri
                    });
                }

                var tokens = uri.Replace("://", ":/").Split('/');
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
            using var unpacker = new EbonyArchive(file);
            var archive = new ArchiveLocation {Path = file.Replace(_profile.GameDataDirectory + "\\", "")};
            _assets.TryAdd(archive, unpacker.Files
                .Where(f => !f.Value.Flags.HasFlag(ArchiveFileFlag.Reference))
                .Select(f => f.Value.Uri));
        }
    }
}