using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

public class MogfestModBuilder
{
    private const string StagingDirectory = @"C:\Modding\Chocomog\Staging";

    private readonly SettingsService _pcSettings = new();

    public void Run()
    {
        RunWithTimer("earc builder", new MogfestEarcBuilder().Run);
        RunWithTimer("asset builder", new MogfestAssetBuilder().Run);
        RunWithTimer("weird earc fixer", FixWeirdEarcs);
        RunWithTimer("sound asset adder", AddConvertedSoundsToMainEarc);
        RunWithTimer("extension flag fixer", ChangeFlagsOnExtensionExceptions);
        RunWithTimer("fragment compresser", CompressFragments);
        RunWithTimer("duplicate destroyer", RemoveDuplicateSoundFiles);
    }

    private void RunWithTimer(string processName, Action action)
    {
        var start = DateTime.Now;
        System.Console.WriteLine($"Starting {processName}...");
        action();
        System.Console.WriteLine(
            $"Finished {processName} after {(DateTime.Now - start).TotalMilliseconds} milliseconds.");
    }

    private void RemoveDuplicateSoundFiles()
    {
        var results = new Dictionary<string, List<string>>();
        using var context = new FlagrumDbContext(new SettingsService());
        var mod = context.EarcMods
            .Include(m => m.LooseFiles)
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .First(m => m.Id == 284);
        
        var allFiles = mod.Earcs
            .SelectMany(e => e.Files.Select(f => new {e.EarcRelativePath, f.Uri, f.Flags}))
            .Where(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference))
            .ToList();
        
        foreach (var file in allFiles)
        {
            if (allFiles.Count(f => f.Uri == file.Uri) > 1)
            {
                if (!results.TryGetValue(file.Uri, out var earcs))
                {
                    earcs = new List<string>();
                    results.Add(file.Uri, earcs);
                }
                
                earcs.Add(file.EarcRelativePath);
            }
        }

        foreach (var (uri, earcs) in results)
        {
            if (earcs.Any(e => e == @"level\dlc_ex\mog\area_ravettrice_mog.earc"))
            {
                var match = earcs.First(e => e != @"level\dlc_ex\mog\area_ravettrice_mog.earc");
                var earc = mod.Earcs.First(e => e.EarcRelativePath == match);
                var file = earc.Files.First(f => f.Uri == uri);
                earc.Files.Remove(file);
            }
            else
            {
                var match = earcs.First();
                var earc = mod.Earcs.First(e => e.EarcRelativePath == match);
                var file = earc.Files.First(f => f.Uri == uri);
                earc.Files.Remove(file);
            }
        }

        context.SaveChanges();
    }

    private void CompressFragments()
    {
        using var context = new FlagrumDbContext(new SettingsService());
        var mod = context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .First(m => m.Id == 284);
        
        foreach (var file in mod.Earcs.SelectMany(e => e.Files)
                     .Where(f => f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                        && f.ReplacementFilePath.EndsWith(".ffg")))
        {
            var hash = Cryptography.HashFileUri64(file.Uri);
            
            var fragment = new FmodFragment();
            fragment.Read($@"C:\Modding\Chocomog\Staging\Fragments\{hash}.ffg");

            if (!fragment.Flags.HasFlag(ArchiveFileFlag.Compressed) && fragment.Data.Length > 4096)
            {
                var processedData = ArchiveFile.GetProcessedData(
                    file.Uri,
                    fragment.Flags | ArchiveFileFlag.Compressed,
                    fragment.Data,
                    0,
                    true,
                    out var archiveFile);

                fragment.ProcessedSize = (uint)processedData.Length;
                fragment.Flags = archiveFile.Flags;
                fragment.Key = archiveFile.Key;
                fragment.Data = processedData;
                
                fragment.Write($@"C:\Modding\Chocomog\Staging\Fragments\{hash}.ffg");
            }
        }
    }

    private void ChangeFlagsOnExtensionExceptions()
    {
        using var context = Ps4Utilities.NewContext();
        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;

        var usAnis = assets
            .Where(a => a.EndsWith(".ani") && a.Contains("/jp/"))
            .Select(a => a.Replace("/jp/", "/us/"))
            .ToList();

        assets.AddRange(usAnis);

        foreach (var uri in assets.Where(a => MogfestAssetBuilder.Extensions.Any(a.EndsWith)))
        {
            var hash = Cryptography.HashFileUri64(uri);
            var path = $@"C:\Modding\Chocomog\Staging\Fragments\{hash}.ffg";
            var fragment = new FmodFragment();
            fragment.Read(path);
            fragment.Flags |= ArchiveFileFlag.Autoload;
            fragment.Write(path);
        }
    }

    private void FixWeirdEarcs()
    {
        FmodFragment fragment;

        var context = Ps4Utilities.NewContext();
        var pcContext = new FlagrumDbContext(_pcSettings);
        var uri = "data://celltool_dlc_mog.mid";
        var path = EnsureFragmentExistsForUri(context, uri, ArchiveFileFlag.None);

        var mod = pcContext.EarcMods
            .Include(m => m.Earcs)
            .First(m => m.Id == 284);

        var earc = mod.Earcs.FirstOrDefault(e => e.EarcRelativePath == "autoexternal.earc");
        if (earc == null)
        {
            earc = new EarcModEarc
            {
                EarcRelativePath = "autoexternal.earc",
                Type = EarcChangeType.Change
            };

            mod.Earcs.Add(earc);
        }

        if (!earc.Files.Any(f => f.Uri == uri))
        {
            fragment = new FmodFragment();
            fragment.Read(path);

            earc.Files.Add(new EarcModFile
            {
                Uri = uri,
                ReplacementFilePath = path,
                Type = EarcFileChangeType.Add,
                Flags = fragment.Flags
            });
        }

        var cells = context.Ps4AssetUris.Where(a =>
                a.Uri.StartsWith("data://navimesh/level/dlc_ex/mog/worldshare/worldshare_mog/"))
            .Select(a => a.Uri)
            .ToList();

        earc = mod.Earcs.FirstOrDefault(e => e.EarcRelativePath == @"level\dlc_ex\mog\worldshare\worldshare_mog.earc");
        if (earc == null)
        {
            earc = new EarcModEarc
            {
                EarcRelativePath = @"level\dlc_ex\mog\worldshare\worldshare_mog.earc",
                Type = EarcChangeType.Change
            };

            mod.Earcs.Add(earc);
        }

        uri = "data://navimesh/level/dlc_ex/mog/worldshare/worldshare_mog/data.nav_world";
        path = EnsureFragmentExistsForUri(context, uri, ArchiveFileFlag.Autoload);

        if (!earc.Files.Any(f => f.Uri == uri))
        {
            fragment = new FmodFragment();
            fragment.Read(path);

            earc.Files.Add(new EarcModFile
            {
                Uri = uri,
                ReplacementFilePath = path,
                Type = EarcFileChangeType.Add,
                Flags = ArchiveFileFlag.Autoload
            });
        }

        foreach (var cell in cells)
        {
            path = EnsureFragmentExistsForUri(context, cell, ArchiveFileFlag.Autoload);

            if (!earc.Files.Any(f => f.Uri == cell))
            {
                fragment = new FmodFragment();
                fragment.Read(path);

                earc.Files.Add(new EarcModFile
                {
                    Uri = cell,
                    ReplacementFilePath = path,
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.Autoload
                });
            }
        }

        pcContext.SaveChanges();
    }

    private string EnsureFragmentExistsForUri(FlagrumDbContext context, string uri, ArchiveFileFlag flags)
    {
        var hash = Cryptography.HashFileUri64(uri);
        var path = $@"{StagingDirectory}\Fragments\{hash}.ffg";

        if (!File.Exists(path))
        {
            var file = Ps4Utilities.GetArchiveFileByUri(context, uri);
            if (file == null)
            {
                return null;
            }
            
            var fragment = new FmodFragment
            {
                OriginalSize = file.Size,
                ProcessedSize = file.ProcessedSize,
                Flags = flags,
                Key = file.Key,
                RelativePath = file.RelativePath,
                Data = file.GetRawData()
            };

            fragment.Write(path);
        }

        return path;
    }
    
    private string EnsureFragmentExistsForUri(string filePath, string uri, ArchiveFileFlag flags)
    {
        var hash = Cryptography.HashFileUri64(uri);
        var path = $@"{StagingDirectory}\Fragments\{hash}.ffg";

        if (!File.Exists(path))
        {
            var data = File.ReadAllBytes(filePath);
            var fragment = new FmodFragment
            {
                OriginalSize = (uint)data.Length,
                ProcessedSize = (uint)data.Length,
                Flags = flags,
                Key = 0,
                RelativePath = uri.Replace("data://", "").Replace('/', '\\').Replace(".sax", ".win.sab"),
                Data = data
            };

            fragment.Write(path);
        }

        return path;
    }
    
    public void AddConvertedSoundsToMainEarc()
    {
        using var ps4Context = Ps4Utilities.NewContext();
        using var pcContext = new FlagrumDbContext(new SettingsService());
        
        var mod = pcContext.EarcMods
            .Include(m => m.Earcs)
            .First(m => m.Id == 284);

        var sounds = ps4Context.Ps4AssetUris.Where(a => a.Uri.EndsWith(".sax"))
            .Select(a => a.Uri)
            .ToList();

        var missingSounds = sounds
            .Where(sound => !pcContext.AssetUris.Any(a => a.Uri == sound))
            .ToList();

        const string audioDirectory = $@"{Ps4PorterConfiguration.StagingDirectory}\Audio2";
        const string outputDirectory = $@"{audioDirectory}\Output";

        var earc = mod.Earcs.First(e => e.EarcRelativePath == @"level\dlc_ex\mog\area_ravettrice_mog.earc");

        foreach (var uri in missingSounds)
        {
            var hash = Cryptography.HashFileUri64(uri).ToString();
            var extension = uri.Split('.').Last() == "sax" ? "orb.sab" : "orb.mab";
            var fileName = $@"{outputDirectory}\{hash}.{extension}";

            if (File.Exists(fileName))
            {
                var saxPath = EnsureFragmentExistsForUri(fileName, uri, ArchiveFileFlag.Autoload);
                
                earc.Files.Add(new EarcModFile
                {
                    Uri = uri,
                    ReplacementFilePath = saxPath,
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.Autoload
                });
                
                var lsd = uri.Insert(uri.LastIndexOf('/'), "/lsd").Replace(".sax", ".lsd").Replace(".max", ".lsd");
                var lsdPath = EnsureFragmentExistsForUri(ps4Context, lsd, ArchiveFileFlag.Autoload);
                if (lsdPath == null)
                {
                    continue;
                }
                
                var lsdFragment = new FmodFragment();
                lsdFragment.Read(lsdPath);
                
                earc.Files.Add(new EarcModFile
                {
                    Uri = lsd,
                    ReplacementFilePath = lsdPath,
                    Type = EarcFileChangeType.Add,
                    Flags = ArchiveFileFlag.Autoload
                });
            }
        }

        pcContext.SaveChanges();
    }
}