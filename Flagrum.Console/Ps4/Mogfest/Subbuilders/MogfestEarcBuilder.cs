using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Console.Ps4.Porting;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Mogfest.Subbuilders;

public class MogfestEarcBuilder
{
    private const string StagingDirectory = @"C:\Modding\Chocomog\Staging";

    private readonly ConcurrentDictionary<string, bool> _assets = new();
    private readonly ConcurrentDictionary<string, bool> _dependencies = new();
    private readonly ConcurrentDictionary<string, string> _existingAssets = new();
    private readonly SettingsService _pcSettings = new();
    private EarcMod _mod;

    public void Run()
    {
        // Load the mod and clear all earcs and files
        using var pcContext = new FlagrumDbContext(_pcSettings);
        _mod = pcContext.EarcMods
            .Include(m => m.LooseFiles)
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .First(m => m.Id == 284);

        foreach (var earc in _mod.Earcs)
        {
            foreach (var file in earc.Files)
            {
                pcContext.Remove(file);
            }

            pcContext.Remove(earc);
        }

        foreach (var file in _mod.LooseFiles)
        {
            pcContext.Remove(file);
        }

        pcContext.SaveChanges();

        using var context = Ps4Utilities.NewContext();
        CreateEarcRecursively(
            context.FestivalDependencies.First(d => d.Uri == "data://level/dlc_ex/mog/area_ravettrice_mog.ebex"));

        pcContext.SaveChanges();

        var assets = _assets
            .Where(kvp => !pcContext.AssetUris
                .Any(a => a.Uri == kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        var json = JsonConvert.SerializeObject(assets, Formatting.Indented);
        File.WriteAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json", json);
    }

    private void CreateEarcRecursively(FestivalDependency ebex)
    {
        lock (ebex)
        {
            if (_dependencies.ContainsKey(ebex.Uri))
            {
                return;
            }

            _dependencies.TryAdd(ebex.Uri, true);
        }

        using var context = Ps4Utilities.NewContext();
        var pcContext = new FlagrumDbContext(_pcSettings);

        var isDuplicate = pcContext.AssetUris.Any(a => a.Uri == ebex.Uri);
        if (isDuplicate)
        {
            return;
        }

        var relativePath = IOHelper.UriToRelativePath(ebex.Uri).Replace(".ebex", ".earc").Replace(".prefab", ".earc");
        var outputPath = $@"{Ps4PorterConfiguration.OutputDirectory}\{relativePath}";
        if (File.Exists(outputPath))
        {
            throw new Exception($"[E] File already exists: {outputPath}");
        }
        
        var earc = new EarcModEarc
        {
            EarcRelativePath = relativePath,
            Type = EarcChangeType.Create,
            Flags = ArchiveHeaderFlags.None
        };

        var fragment = new FmodFragment();
        fragment.Read($@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(ebex.Uri)}.ffg");

        var file = new EarcModFile
        {
            Uri = ebex.Uri,
            ReplacementFilePath = $@"{StagingDirectory}\Fragments\{Cryptography.HashFileUri64(ebex.Uri)}.ffg",
            Type = EarcFileChangeType.Add,
            Flags = ArchiveFileFlag.Autoload
        };

        earc.Files.Add(file);
        _mod.Earcs.Add(earc);

        var children = context.FestivalDependencyFestivalDependency
            .Where(d => d.ParentId == ebex.Id)
            .Select(d => d.Child)
            .ToList();

        var selfReference = ebex.Uri + "@";

        Parallel.ForEach(children, child =>
        {
            lock (earc)
            {
                var childUri = child.Uri + "@";
                if (!earc.Files.Any(f => f.Uri == childUri) && childUri != selfReference)
                {
                    earc.Files.Add(new EarcModFile
                    {
                        Uri = childUri,
                        Type = EarcFileChangeType.AddReference,
                        Flags = ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference
                    });
                }
            }

            CreateEarcRecursively(child);
        });

        var referenceDependencies = new Dictionary<string, bool>();
        var subdependencies = context.FestivalDependencyFestivalSubdependency
            .Where(d => d.DependencyId == ebex.Id)
            .Select(d => d.Subdependency)
            .ToList();

        foreach (var subdependency in subdependencies)
        {
            _assets.TryAdd(subdependency.Uri.ToLower(), true);

            var referenceUri = GetExistingReferenceUri(pcContext, subdependency.Uri);

            if (referenceUri == null && (subdependency.Uri.EndsWith(".max") || subdependency.Uri.EndsWith(".sax")))
            {
                var extension = subdependency.Uri.Split('.').Last().Replace('x', 'b');
                var path =
                    $@"{Ps4PorterConfiguration.StagingDirectory}\Audio\Output\{Cryptography.HashFileUri64(subdependency.Uri)}.orb.{extension}";
                if (File.Exists(path))
                {
                    referenceUri = subdependency.Uri;
                }
            }

            var reference = referenceUri ?? subdependency.Uri[..subdependency.Uri.LastIndexOf('/')].ToLower() +
                "/autoexternal.ebex@";
            referenceDependencies.TryAdd(reference, true);

            var modelDependencies = context.FestivalSubdependencyFestivalModelDependency
                .Where(s => s.SubdependencyId == subdependency.Id)
                .Select(s => s.ModelDependency)
                .ToList();

            foreach (var modelDependency in modelDependencies)
            {
                _assets.TryAdd(modelDependency.Uri.ToLower(), true);
                var modelReferenceUri = GetExistingReferenceUri(pcContext, modelDependency.Uri);
                var modelReference = modelReferenceUri ??
                                     modelDependency.Uri[..modelDependency.Uri.LastIndexOf('/')].ToLower() +
                                     "/autoexternal.ebex@";
                referenceDependencies.TryAdd(modelReference, true);

                var materialDependencies = context.FestivalModelDependencyFestivalMaterialDependency
                    .Where(m => m.ModelDependencyId == modelDependency.Id)
                    .Select(m => m.MaterialDependency)
                    .ToList();

                foreach (var materialDependency in materialDependencies)
                {
                    if (materialDependency.Uri.EndsWith(".htpk"))
                    {
                        _assets.TryAdd(materialDependency.Uri.ToLower(), true);
                        referenceDependencies.TryAdd(materialDependency.Uri, true);
                    }
                    else
                    {
                        _assets.TryAdd(materialDependency.Uri.ToLower(), true);
                        var materialReferenceUri = GetExistingReferenceUri(pcContext, materialDependency.Uri);
                        var materialReference = materialReferenceUri ??
                                                materialDependency.Uri[..materialDependency.Uri.LastIndexOf('/')]
                                                    .ToLower() +
                                                "/autoexternal.ebex@";
                        referenceDependencies.TryAdd(materialReference, true);
                    }
                }
            }
        }

        foreach (var (uri, _) in referenceDependencies)
        {
            if (!earc.Files.Any(f => f.Uri == uri) && uri != selfReference)
            {
                earc.Files.Add(new EarcModFile
                {
                    Uri = uri,
                    Type = EarcFileChangeType.AddReference,
                    Flags = uri.EndsWith(".htpk")
                        ? ArchiveFileFlag.Reference
                        : ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference
                });
            }
        }

        // TODO: Create and add autoext file
    }

    private string GetExistingReferenceUri(FlagrumDbContext pcContext, string uri)
    {
        uri = uri.ToLower();

        if (_existingAssets.TryGetValue(uri, out var referenceUri))
        {
            return referenceUri;
        }

        var isDuplicate = pcContext.AssetUris.Any(a => EF.Functions.Like(a.Uri, uri));
        if (isDuplicate)
        {
            var location = pcContext.GetArchiveRelativeLocationByUri(uri);
            if (location == "UNKNOWN")
            {
                throw new Exception("Oh no...");
            }

            referenceUri = "data://" + location.Replace('\\', '/').Replace(".earc", ".ebex@");
            _existingAssets.TryAdd(uri, referenceUri);
        }
        else
        {
            _existingAssets.TryAdd(uri, null);
        }

        return referenceUri;
    }
}