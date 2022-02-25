using System.ComponentModel.DataAnnotations;
using System.Linq;
using Flagrum.Core.Archive;

namespace Flagrum.Web.Persistence.Entities;

public class AssetUri
{
    [Key] public string Uri { get; set; }

    public int ArchiveLocationId { get; set; }
    public ArchiveLocation ArchiveLocation { get; set; }
}

public static class AssetUriExtensions
{
    public static byte[] GetFileByUri(this FlagrumDbContext context, string uri)
    {
        var location = context.Settings.GameDataDirectory + "\\" + context.AssetUris
            .Where(a => a.Uri == uri)
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();

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