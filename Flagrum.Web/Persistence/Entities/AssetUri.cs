using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Persistence.Entities;

public class AssetUri
{
    [Key] public string Uri { get; set; }

    public int ArchiveLocationId { get; set; }
    public ArchiveLocation ArchiveLocation { get; set; }
}

public static class AssetUriExtensions
{
    public static string GetArchiveAbsoluteLocationByUri(this FlagrumDbContext context, string uri)
    {
        var uriPattern = $"%{uri}%";
        var earcRelativePath = context.AssetUris
            .Where(a => EF.Functions.Like(a.Uri, uriPattern))
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();

        return earcRelativePath == null ? null : context.Profile.GameDataDirectory + "\\" + earcRelativePath;
    }

    public static string GetArchiveRelativeLocationByUri(this FlagrumDbContext context, string uri)
    {
        var uriPattern = $"%{uri}%";
        var earcRelativePath = context.AssetUris
            .Where(a => EF.Functions.Like(a.Uri, uriPattern))
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();

        return earcRelativePath ?? "UNKNOWN";
    }

    public static byte[] GetFileByUri(this FlagrumDbContext context, string uri)
    {
        var uriPattern = $"%{uri}%";
        var earcRelativePath = context.AssetUris
            .Where(a => EF.Functions.Like(a.Uri, uriPattern))
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();

        if (earcRelativePath == null)
        {
            return Array.Empty<byte>();
        }

        var location = context.Profile.GameDataDirectory + "\\" + earcRelativePath;
        var archive = context.Profile.OpenArchive(location);
        return archive[uri].GetReadableData();
    }

    public static IDictionary<string, byte[]> GetFilesByUri(this FlagrumDbContext context, IEnumerable<string> uris)
    {
        var items = new ConcurrentDictionary<string, byte[]>();

        var uriPaths = uris.Select(u => context.AssetUris
                .Where(a => a.Uri == u)
                .Select(a => new {a.Uri, a.ArchiveLocation.Path})
                .FirstOrDefault()!)
            .ToList();

        Parallel.ForEach(uriPaths.Select(up => up.Path).Distinct(), earcPath =>
        {
            var earcUris = uriPaths.Where(up => up.Path == earcPath).Select(up => up.Uri);
            using var unpacker = new EbonyArchive(context.Profile.GameDataDirectory + "\\" + earcPath);
            foreach (var uri in earcUris)
            {
                items.TryAdd(uri, unpacker[uri].GetReadableData());
            }
        });

        return items;
    }

    public static bool ArchiveExistsForUri(this FlagrumDbContext context, string uri)
    {
        var location = context.AssetUris
            .Where(a => a.Uri == uri)
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();
        return location != null;
    }
}