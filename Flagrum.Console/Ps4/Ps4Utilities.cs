using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public static byte[] GetFileByUri(FlagrumDbContext context, string uri, bool patch2Only = false)
    {
        uri = uri.ToLower();

        var earcRelativePaths = context.Ps4AssetUris
            .Where(a => a.Uri == uri)
            .SelectMany(a => a.ArchiveAssets)
            .OrderByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch2"))
            .ThenByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch1"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch2"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch1"))
            .Select(a => a.Ps4ArchiveLocation.Path)
            .ToList();

        var hasNonPatch2Data = false;

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
                if (patch2Only && !earcRelativePath.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch2"))
                {
                    hasNonPatch2Data = true;
                }
                else
                {
                    return data;
                }
            }
        }

        return hasNonPatch2Data ? new byte[] {0x00} : Array.Empty<byte>();
    }
    
    public static ArchiveFile GetArchiveFileByUri(FlagrumDbContext context, string uri)
    {
        uri = uri.ToLower();

        var earcRelativePaths = context.Ps4AssetUris
            .Where(a => a.Uri == uri)
            .SelectMany(a => a.ArchiveAssets)
            .OrderByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch2"))
            .ThenByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch1"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch2"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch1"))
            .Select(a => a.Ps4ArchiveLocation.Path)
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

            var result = unpacker.Files.FirstOrDefault(f => f.Uri == uri);
            unpacker.ReadFileData(result);

            var buffer = result.GetRawData();
            if (buffer is {Length: > 0} && !IsDeleted(buffer))
            {
                return result;
            }
        }

        return null;
    }

    private static bool IsDeleted(byte[] buffer)
    {
        var deletedString = Encoding.UTF8.GetBytes("deleted");
        var chars = new[] {buffer[0], buffer[1], buffer[2], buffer[3], buffer[4], buffer[5], buffer[6]};
        for (var i = 0; i < 7; i++)
        {
            if (deletedString[i] != chars[i])
            {
                return false;
            }
        }

        return true;
    }

    public static byte[] GetFileByUri(FlagrumDbContext context, string uri, out ArchiveFile file)
    {
        uri = uri.ToLower();

        var earcRelativePaths = context.Ps4AssetUris
            .Where(a => a.Uri == uri)
            .SelectMany(a => a.ArchiveAssets)
            .OrderByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch2"))
            .ThenByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch1"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch2"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch1"))
            .Select(a => a.Ps4ArchiveLocation.Path)
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
            file = unpacker.Files.FirstOrDefault(f => f.Uri == uri);

            if (data.Length > 0 && !(data[0] == 100 && data[1] == 101))
            {
                return data;
            }
        }

        file = null;
        return Array.Empty<byte>();
    }

    public static byte[] GetFileByUriRaw(FlagrumDbContext context, string uri, out ArchiveFile file)
    {
        uri = uri.ToLower();

        var earcRelativePaths = context.Ps4AssetUris
            .Where(a => a.Uri == uri)
            .SelectMany(a => a.ArchiveAssets)
            .OrderByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch2"))
            .ThenByDescending(a =>
                a.Ps4ArchiveLocation.Path.Contains(@"CUSA01633-patch_115\CUSA01633-patch\patch\patch1"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch2"))
            .ThenByDescending(a => a.Ps4ArchiveLocation.Path.Contains(@"FFXV_Patch\patch1"))
            .Select(a => a.Ps4ArchiveLocation.Path)
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

            var data = unpacker.UnpackRawByUri(uri);
            file = unpacker.Files.FirstOrDefault(f => f.Uri == uri);

            if (data.Length > 0 && !(data[0] == 100 && data[1] == 101))
            {
                return data;
            }
        }

        file = null;
        return Array.Empty<byte>();
    }

    public static IEnumerable<(string Earc, byte[] Data)> GetFilesByUri(FlagrumDbContext context, string uri)
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
            yield return (earcRelativePath, data);
        }
    }
}