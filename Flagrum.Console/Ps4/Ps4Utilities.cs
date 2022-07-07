using System;
using System.Collections.Concurrent;
using System.Linq;
using Flagrum.Console.Ps4.Porting;
using Flagrum.Core.Archive;
using Flagrum.Web.Persistence;
using Flagrum.Web.Services;

namespace Flagrum.Console.Ps4;

public static class Ps4Utilities
{
    private static readonly object _lock = new();
    private static readonly ConcurrentDictionary<string, Unpacker> _unpackers = new();

    private static readonly object _settingsLock = new();
    private static SettingsService _settings;
    private static SettingsService Settings => _settings ??= new SettingsService {GamePath = Ps4Constants.GamePath};

    public static FlagrumDbContext NewContext()
    {
        lock (_settingsLock)
        {
            return new FlagrumDbContext(Settings, Ps4Constants.DatabasePath);
        }
    }

    public static byte[] GetFileByUri(FlagrumDbContext context, string uri)
    {
        uri = uri.ToLower();

        var earcRelativePaths = context.Ps4AssetUris
            .Where(a => a.Uri == uri)
            .SelectMany(a => a.ArchiveAssets.Select(aa => aa.Ps4ArchiveLocation.Path))
            .ToList();

        foreach (var earcRelativePath in earcRelativePaths)
        {
            Unpacker unpacker;
            var location = Ps4Constants.DatasDirectory + "\\" + earcRelativePath;
            lock (_lock)
            {
                if (!_unpackers.TryGetValue(location, out unpacker))
                {
                    unpacker = new Unpacker(location);
                    _ = unpacker.Files; // Forces file headers to read
                    _unpackers[location] = unpacker;
                }
            }

            var data = unpacker.UnpackReadableByUri(uri);
            if (data.Length > 0 && !(data[0] == 100 && data[1] == 101))
            {
                return data;
            }
        }
        
        return Array.Empty<byte>();
    }
}