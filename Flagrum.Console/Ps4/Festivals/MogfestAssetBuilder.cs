using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Console.Ps4.Porting;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Festivals;

public class MogfestAssetBuilder
{
    private const string StagingDirectory = @"C:\Modding\Chocomog\Staging";

    public static string[] Extensions => new[]
    {
        ".anmgph"
    };
    
    private readonly SettingsService _pcSettings = new();
    private EarcMod _mod;

    public void Run()
    {
        using var pcContext = new FlagrumDbContext(_pcSettings);
        _mod = pcContext.EarcMods
            .Include(m => m.Earcs)
            .First(m => m.Id == 284);

        using var context = Ps4Utilities.NewContext();
        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;

        var usAnis = assets
            .Where(a => a.EndsWith(".ani") && a.Contains("/jp/"))
            .Select(a => a.Replace("/jp/", "/us/"))
            .ToList();

        assets.AddRange(usAnis);

        var earc = _mod.Earcs.First(e => e.EarcRelativePath == @"level\dlc_ex\mog\area_ravettrice_mog.earc");
        foreach (var asset in assets.Where(a => Extensions.Any(a.EndsWith)))
        {
            if (!earc.Files.Any(f => f.Uri == asset))
            {
                var fragment = new FmodFragment();
                fragment.Read($@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(asset)}.ffg");

                var file = new EarcModFile
                {
                    Uri = asset,
                    ReplacementFilePath = $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(asset)}.ffg",
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.Autoload
                };

                earc.Files.Add(file);
            }
        }

        assets = assets.Where(a => !Extensions.Any(a.EndsWith))
            .ToList();
        
        var earcs = new Dictionary<string, List<string>>();
        foreach (var uri in assets)
        {
            var fixedUri = uri.Replace('\\', '/');
            var folder = fixedUri[..fixedUri.LastIndexOf('/')];
            earcs.TryGetValue(folder, out var list);
            if (list == null)
            {
                list = new List<string>();
                earcs.Add(folder, list);
            }

            list.Add(fixedUri);
        }

        Parallel.ForEach(earcs, kvp =>
        {
            var (folder, uris) = kvp;
            var earcRelativePath = @$"{IOHelper.UriToRelativePath(folder)}\autoexternal.earc";
            var earcPath = $@"{Ps4PorterConfiguration.OutputDirectory}\{earcRelativePath}";

            EarcModEarc assetEarc;
            if (File.Exists(earcPath))
            {
                assetEarc = new EarcModEarc
                {
                    EarcRelativePath = earcRelativePath,
                    Type = EarcChangeType.Change
                };
            }
            else
            {
                assetEarc = new EarcModEarc
                {
                    EarcRelativePath = earcRelativePath,
                    Type = EarcChangeType.Create,
                    Flags = ArchiveHeaderFlags.HasLooseData
                };
            }

            foreach (var uri in uris)
            {
                if (uri.EndsWith(".htpk"))
                {
                    var htpkFolder = uri[..uri.LastIndexOf('/')];
                    var sourceimagesFolder = htpkFolder[..htpkFolder.LastIndexOf('/')] + "/sourceimages";

                    if (earcs.ContainsKey(sourceimagesFolder))
                    {
                        var htpkPath = IOHelper.UriToRelativePath(uri).Replace(".htpk", ".earc");

                        var htpkEarc = new EarcModEarc
                        {
                            EarcRelativePath = htpkPath,
                            Type = EarcChangeType.Create,
                            Flags = ArchiveHeaderFlags.None
                        };

                        var htpkReference = new EarcModFile
                        {
                            Uri = $"{sourceimagesFolder}/autoexternal.ebex@",
                            Type = EarcFileChangeType.AddReference,
                            Flags = ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference
                        };

                        var htpkUri = uri.Replace(".htpk", ".autoext");
                        var htpkFragmentPath =
                            $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(htpkUri)}.ffg";
                        var htpkFragment = new FmodFragment();
                        htpkFragment.Read(htpkFragmentPath);

                        var htpkFile = new EarcModFile
                        {
                            Uri = htpkUri,
                            ReplacementFilePath = htpkFragmentPath,
                            Type = EarcFileChangeType.Add,
                            Flags = htpkFragment.Flags
                        };

                        htpkEarc.Files.Add(htpkReference);
                        htpkEarc.Files.Add(htpkFile);
                        _mod.Earcs.Add(htpkEarc);
                    }
                }
                else
                {
                    AddFileByUri(assetEarc, uri);
                }
            }

            if (assetEarc.Files.Any())
            {
                _mod.Earcs.Add(assetEarc);
            }
        });

        pcContext.SaveChanges();
    }

    private void AddFileByUri(EarcModEarc earc, string uri)
    {
        using var context = Ps4Utilities.NewContext();

        if (uri.EndsWith(".sax") || uri.EndsWith(".max") || uri.EndsWith(".bk2"))
        {
            // Calculate paths
            var extension = uri.Split('.').Last().Replace('x', 'b');
            var loosePath = uri.EndsWith(".bk2")
                ? Ps4PorterConfiguration.GameDirectory + @"\CUSA01633-patch_115\CUSA01633-patch\" +
                  uri.Replace("data://", "").Replace('/', '\\')
                : $@"{Ps4PorterConfiguration.StagingDirectory}\Audio\Output\{Cryptography.HashFileUri64(uri)}.orb.{extension}";

            if (File.Exists(loosePath))
            {
                if (context.Ps4AssetUris.Any(a => a.Uri == uri))
                {
                    earc.Files.Add(new EarcModFile
                    {
                        Uri = uri,
                        ReplacementFilePath = loosePath,
                        Type = EarcFileChangeType.Add
                    });
                }
                else
                {
                    // Add reference to earc
                    earc.Files.Add(new EarcModFile
                    {
                        Uri = uri,
                        Type = EarcFileChangeType.AddReference,
                        Flags = ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference
                    });

                    // Add loose file to mod
                    var directPath = uri.Replace("data://", "").Replace('/', '\\');
                    if (!uri.EndsWith(".bk2"))
                    {
                        directPath = directPath.Insert(directPath.LastIndexOf('.'), ".win");
                        directPath = directPath[..^1] + 'b'; // Change sax/max to sab/mab
                    }

                    _mod.LooseFiles.Add(new EarcModLooseFile
                    {
                        RelativePath = directPath,
                        FilePath = loosePath,
                        Type = EarcChangeType.Create
                    });
                }
            }

            // Need to return here so the final part doesn't run
            return;
        }

        if (uri.EndsWith(".tif") || uri.EndsWith(".dds") || uri.EndsWith(".png") || uri.EndsWith(".btex") ||
            uri.EndsWith(".exr"))
        {
            // Add high texture from altered uri if fragment exists
            var highTextureUri = uri.Insert(uri.LastIndexOf('.'), "_$h");
            var highPath = $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(highTextureUri)}.ffg";
            if (File.Exists(highPath))
            {
                var highFragment = new FmodFragment();
                highFragment.Read(highPath);
                var highFile = new EarcModFile
                {
                    Uri = highTextureUri,
                    ReplacementFilePath = highPath,
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.None
                };

                earc.Files.Add(highFile);
            }
        }
        else if (uri.EndsWith(".swf"))
        {
            // Add listb and swf if fragment exists
            var listbUri = uri.Replace(".swf", ".listb");
            var folder = uri[..uri.LastIndexOf('/')];
            var folderName = folder[(folder.LastIndexOf('/') + 1)..];
            var swfUri = $"{folder}/{folderName}.btex";

            var listbPath = $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(listbUri)}.ffg";
            if (File.Exists(listbPath))
            {
                var listbFragment = new FmodFragment();
                listbFragment.Read(listbPath);
                var listbFile = new EarcModFile
                {
                    Uri = listbUri,
                    ReplacementFilePath = listbPath,
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.None
                };

                earc.Files.Add(listbFile);
            }

            var swfPath = $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(swfUri)}.ffg";
            if (File.Exists(swfPath))
            {
                var swfFragment = new FmodFragment();
                swfFragment.Read(swfPath);
                var swfFile = new EarcModFile
                {
                    Uri = swfUri,
                    ReplacementFilePath = swfPath,
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.None
                };

                earc.Files.Add(swfFile);
            }
        }

        // Add file for uri is fragment exists
        var path = $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(uri)}.ffg";
        if (File.Exists(path))
        {
            var fragment = new FmodFragment();
            fragment.Read(path);
            var file = new EarcModFile
            {
                Uri = uri,
                ReplacementFilePath = path,
                Type = EarcFileChangeType.Add,
                Flags = ArchiveFileFlag.None
            };

            earc.Files.Add(file);
        }
    }
}