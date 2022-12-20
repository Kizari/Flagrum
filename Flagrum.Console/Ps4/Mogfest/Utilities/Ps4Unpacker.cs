using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;

namespace Flagrum.Console.Ps4.Mogfest.Utilities;

public class Ps4Unpacker
{
    private static readonly ConcurrentDictionary<string, Unpacker> _unpackers = new();

    public static void Run()
    {
        var outerContext = Ps4Utilities.NewContext();
        var uris = outerContext.Ps4AssetUris.Select(a => a.Uri).ToList();

        Parallel.ForEach(uris, uri =>
        {
            var context = Ps4Utilities.NewContext();
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
                lock (_unpackers)
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
                    var path = MogfestUtilities.UriToFilePath(uri);
                    IOHelper.EnsureDirectoriesExistForFilePath(path);
                    File.WriteAllBytes(path, data);
                }
            }
        });
    }
}