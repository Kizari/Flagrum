using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Console.Ps4.Mogfest.Utilities;
using Flagrum.Console.Ps4.Porting;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Mogfest.Subbuilders;

public class MogfestAssetBuilder
{
    private const string StagingDirectory = @"C:\Modding\Chocomog\Staging";

    private readonly SettingsService _pcSettings = new();
    private EarcMod _mod;
    private EarcModEarc _rootEarc;
    private readonly ConcurrentDictionary<string, string> _shaderLocations = new();

    public static string[] Extensions => new[]
    {
        ".anmgph"
    };

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
        _rootEarc = earc;
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
                    var materialUri = uri.Replace("_$h.htpk", ".gmtl");
                    var materialData = MogfestUtilities.GetFileByUri(materialUri);
                    var material = new MaterialReader(materialData).Read();
                    var textureDependencies = material.Header.Dependencies
                        .Where(d => d.Path.EndsWith(".tif")
                                    || d.Path.EndsWith(".exr")
                                    || d.Path.EndsWith(".dds")
                                    || d.Path.EndsWith(".png")
                                    || d.Path.EndsWith(".btex"))
                        .Select(p => p.Path)
                        .ToList();
                    
                    var htpkPath = IOHelper.UriToRelativePath(uri).Replace(".htpk", ".earc");
                    var htpkEarc = new EarcModEarc
                    {
                        EarcRelativePath = htpkPath,
                        Type = File.Exists($@"{_pcSettings.GameDataDirectory}\{htpkPath}") ? EarcChangeType.Change : EarcChangeType.Create,
                        Flags = ArchiveHeaderFlags.None
                    };

                    using var innerContext = new FlagrumDbContext(_pcSettings);
                    foreach (var texture in textureDependencies)
                    {
                        var relativePath = innerContext.GetArchiveRelativeLocationByUri(texture);

                        if (relativePath == "UNKNOWN")
                        {
                            var htpkFolder = texture[..texture.LastIndexOf('/')];
                            relativePath = htpkFolder.Replace("data://", "").Replace('/', '\\') + "\\autoexternal.earc";
                        }
                        
                        var referenceUri = "data://" + relativePath.Replace('\\', '/').Replace(".earc", ".ebex@");

                        if (!htpkEarc.Files.Any(f => f.Uri == referenceUri))
                        {
                            var htpkReference = new EarcModFile
                            {
                                //Uri = $"{sourceimagesFolder}/autoexternal.ebex@",
                                Uri = referenceUri,
                                Type = EarcFileChangeType.AddReference,
                                Flags = ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference
                            };

                            htpkEarc.Files.Add(htpkReference);
                        }
                    }

                    var htpkUri = uri.Replace(".htpk", ".autoext");
                    var htpkFragmentPath =
                        $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(htpkUri)}.ffg";
                    var htpkData = Encoding.UTF8.GetBytes(string.Join(' ', textureDependencies.Select(t => t.Insert(t.LastIndexOf('.'), "_$h"))) + (char)0x00);
                    var htpkFragment = new FmodFragment
                    {
                        OriginalSize = (uint)htpkData.Length,
                        ProcessedSize = (uint)htpkData.Length,
                        Flags = ArchiveFileFlag.Autoload,
                        Key = 0,
                        RelativePath = htpkUri.Replace("data://", ""),
                        Data = htpkData
                    };
                    
                    htpkFragment.Write(htpkFragmentPath);

                    var htpkFile = new EarcModFile
                    {
                        Uri = htpkUri,
                        ReplacementFilePath = htpkFragmentPath,
                        Type = EarcFileChangeType.Add,
                        Flags = htpkFragment.Flags
                    };
                    
                    htpkEarc.Files.Add(htpkFile);
                    _mod.Earcs.Add(htpkEarc);
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
        else if (uri.EndsWith(".gmtl"))
        {
            using var pcContext = new FlagrumDbContext(_pcSettings);
            var fragment = new FmodFragment();
            fragment.Read(@$"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(uri)}.ffg");
            var materialData = fragment.Data;
            var material = new MaterialReader(materialData).Read();
            foreach (var shader in material.Header.Dependencies
                         .Where(d => d.Path.EndsWith(".sb")))
            {
                // if (!_shaderLocations.TryGetValue(shader.Path, out var location))
                // {
                //     var relativeLocation = pcContext.GetArchiveRelativeLocationByUri(shader.Path);
                //     if (relativeLocation == "UNKNOWN")
                //     {
                //         throw new Exception("I can't believe you've done this.");
                //     }
                //     
                //     location = "data://" + relativeLocation.Replace('\\', '/').Replace(".earc", ".ebex@");
                //     _shaderLocations[shader.Path] = location;
                // }
                //
                // if (!_rootEarc.Files.Any(f => f.Uri == location))
                // {
                //     _rootEarc.Files.Add(new EarcModFile
                //     {
                //         Uri = location,
                //         Type = EarcFileChangeType.AddReference,
                //         Flags = ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference
                //     });
                // }
                
                var shaderPath = $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(shader.Path)}.ffg";
                if (!File.Exists(shaderPath))
                {
                    continue;
                }
                // var shaderFragment = new FmodFragment();
                // shaderFragment.Read(shaderPath);
                var shaderFile = new EarcModFile
                {
                    Uri = shader.Path,
                    ReplacementFilePath = shaderPath,
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.Compressed
                };
                
                earc.Files.Add(shaderFile);
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