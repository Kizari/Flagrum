using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        return earcRelativePath == null ? null : context.Settings.GameDataDirectory + "\\" + earcRelativePath;
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

        var location = context.Settings.GameDataDirectory + "\\" + earcRelativePath;

        return Unpacker.GetFileByLocation(location, uri);
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