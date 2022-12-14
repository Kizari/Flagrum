using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Console.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Console.Scripts.Desktop;

public static class ModScripts
{
    public static void MergeMods(int first, int second, int target, bool bypassDuplicatesCheck)
    {
        using var context = new FlagrumDbContext(new SettingsService());

        var mod1 = context.EarcMods
            .Include(m => m.LooseFiles)
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .AsNoTracking()
            .Where(m => m.Id == first)
            .ToList()
            .First();

        var mod2 = context.EarcMods
            .Include(m => m.LooseFiles)
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .AsNoTracking()
            .Where(m => m.Id == second)
            .ToList()
            .First();

        System.Console.WriteLine("Checking first mod for duplicates");
        var canProceed = !ModHasDuplicates(mod1);

        System.Console.WriteLine("Checking second mod for duplicates");
        if (ModHasDuplicates(mod2))
        {
            canProceed = false;
        }
        
        System.Console.WriteLine("Comparing both mods for duplicates");
        if (ModsHaveDuplicates(mod1, mod2))
        {
            canProceed = false;
        }

        if (!bypassDuplicatesCheck && !canProceed)
        {
            return;
        }
        
        // Clear the mod
        foreach (var earc in context.EarcModEarcs
                     .Include(e => e.Files)
                     .Where(e => e.EarcModId == target))
        {
            foreach (var change in earc.Files)
            {
                context.Remove(change);
            }

            context.Remove(earc);
        }

        foreach (var file in context.EarcModLooseFile
                     .Where(f => f.EarcModId == target))
        {
            context.Remove(file);
        }

        context.SaveChanges();

        var mod = context.EarcMods.First(m => m.Id == target);
        mod.Earcs = mod1.Earcs.ToList();
        ((List<EarcModEarc>)mod.Earcs).AddRange(mod2.Earcs);

        foreach (var earc in mod.Earcs)
        {
            earc.Id = 0;
            earc.EarcMod = null;
            earc.EarcModId = 0;

            foreach (var file in earc.Files)
            {
                file.Id = 0;
                file.EarcModEarc = null;
                file.EarcModEarcId = 0;
            }
        }

        mod.LooseFiles = mod1.LooseFiles.ToList();
        ((List<EarcModLooseFile>)mod.LooseFiles).AddRange(mod2.LooseFiles);

        foreach (var file in mod.LooseFiles)
        {
            file.Id = 0;
            file.EarcMod = null;
            file.EarcModId = 0;
        }

        context.SaveChanges();
    }
    
    public static bool CheckModForDuplicates(int modId)
    {
        using var context = new FlagrumDbContext(new SettingsService());
        var mod = context.EarcMods
            .Include(m => m.LooseFiles)
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .First(m => m.Id == modId);

        return ModHasDuplicates(mod);
    }

    private static bool ModsHaveDuplicates(EarcMod first, EarcMod second)
    {
        var result = false;

        var earcs = first.Earcs.ToList();
        earcs.AddRange(second.Earcs);
    
        foreach (var earc in earcs)
        {
            if (earcs.Count(e => e.EarcRelativePath == earc.EarcRelativePath) > 1)
            {
                System.Console.WriteLine($"Duplicate earc {earc.EarcRelativePath} found");
                result = true;
            }
        
            foreach (var file in earc.Files.Where(f => f.Type is EarcFileChangeType.AddReference))
            {
                if (earc.Files.Count(f => f.Uri == file.Uri) > 1)
                {
                    System.Console.WriteLine($"Duplicate reference {file.Uri} found in {earc.EarcRelativePath}");
                    result = true;
                }
            }
        }

        var allFiles = earcs
            .SelectMany(e => e.Files)
            .Where(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference))
            .ToList();
    
        foreach (var file in allFiles)
        {
            if (allFiles.Count(f => f.Uri == file.Uri) > 1)
            {
                System.Console.WriteLine($"Duplicate of {file.Uri} found!");
                result = true;
            }
        }

        return result;
    }

    private static bool ModHasDuplicates(EarcMod mod)
    {
        var result = false;
        var results = new Dictionary<string, List<string>>();
        
        foreach (var earc in mod.Earcs)
        {
            if (mod.Earcs.Count(e => e.EarcRelativePath == earc.EarcRelativePath) > 1)
            {
                System.Console.WriteLine($"Duplicate earc {earc.EarcRelativePath} found");
                result = true;
            }
            
            foreach (var file in earc.Files.Where(f => f.Type is EarcFileChangeType.AddReference))
            {
                if (earc.Files.Count(f => f.Uri == file.Uri) > 1)
                {
                    System.Console.WriteLine($"Duplicate reference {file.Uri} found in {earc.EarcRelativePath}");
                    result = true;
                }
            }
        }

        var allFiles = mod.Earcs
            .SelectMany(e => e.Files.Select(f => new {e.EarcRelativePath, f.Uri, f.Flags}))
            .Where(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference))
            .ToList();
        
        foreach (var file in allFiles)
        {
            if (allFiles.Count(f => f.Uri == file.Uri) > 1)
            {
                var match = results.TryGetValue(file.Uri, out var earcs);
                if (earcs == null)
                {
                    earcs = new List<string>();
                    results.Add(file.Uri, earcs);
                }
                
                earcs.Add(file.EarcRelativePath);
                result = true;
            }
        }

        foreach (var (uri, earcs) in results)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine(uri);
            foreach (var earc in earcs)
            {
                System.Console.WriteLine(" - " + earc);
            }
        }

        return result;
    }
    
    public static void CreateModFromEarc()
    {
        using var context = new FlagrumDbContext(new SettingsService());
        var mod = new EarcMod
        {
            Name = "Tonberry",
            Author = "Kizari",
            Description = "Test",
            IsFavourite = true
        };

        using var unpacker =
            new Unpacker(
                @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\character\mf\mf11\entry\mf11_001_mastertonberry.earc");

        for (var i = 2; i < 5; i++)
        {
            var earc = new EarcModEarc
            {
                EarcRelativePath = @$"character\mf\mf11\entry\mf11_00{i}_xmastonberry.earc",
                Type = EarcChangeType.Create
            };

            foreach (var file in unpacker.Files.Where(f => f.Flags.HasFlag(ArchiveFileFlag.Reference)))
            {
                earc.Files.Add(new EarcModFile
                {
                    Uri = file.Uri,
                    Type = EarcFileChangeType.AddReference,
                    Flags = file.Flags
                });
            }

            mod.Earcs.Add(earc);
        }

        context.Add(mod);
        context.SaveChanges();
    }
    
    public static void AlterExecutable()
    {
        var newValue = BitConverter.GetBytes(999u);
        var searcher = new BoyerMoore(BitConverter.GetBytes(200u));
        var executable =
            File.ReadAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\ffxv_s.backup");

        // var offsets = searcher.Search(executable).ToList();
        // for (var i = offsets.Count - 10; i < offsets.Count; i++)
        // {
        //     var offset = offsets[i];
        //     System.Console.WriteLine($"Updated value at offset {offset}");
        //     executable[offset] = newValue[0];
        //     executable[offset + 1] = newValue[1];
        //     executable[offset + 2] = newValue[2];
        //     executable[offset + 3] = newValue[3];
        // }

        File.WriteAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\ffxv_s.exe", executable);
    }

    public static void ClearProjectFoldersForRemovedMods()
    {
        using var context = new FlagrumDbContext(new SettingsService());
        var modIds = context.EarcMods.Select(m => m.Id).ToList();

        foreach (var folder in Directory.EnumerateDirectories($@"{context.Settings.FlagrumDirectory}\earc"))
        {
            if (int.TryParse(folder.Split('\\').Last(), out var folderId))
            {
                if (!modIds.Contains(folderId))
                {
                    Directory.Delete(folder, true);
                }
            }
        }
    }
}