using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4EarcPorter
{
    private readonly ConcurrentDictionary<string, bool> _assets = new();
    private readonly List<string> _modifiedEarcs = new();
    private readonly SettingsService _pcSettings = new();
    private readonly ConcurrentDictionary<string, string> _existingAssets = new();
    private readonly Dictionary<string, bool> _dependencies = new();
    private readonly object _packerLock = new();

    public void RunSingleEarc()
    {
        using var context = Ps4Utilities.NewContext();
        var packer = new Packer();
        packer.Header.Flags = 0;
        CreateEarcRecursivelySingle(context.FestivalDependencies.First(d => d.Uri == "data://level/dlc_ex/mog/area_ravettrice_mog.ebex"), packer);
        packer.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
    }
    
    public void Run()
    {
        using var context = Ps4Utilities.NewContext();
        var pcContext = new FlagrumDbContext(new SettingsService());
        CreateEarcRecursively(context.FestivalDependencies.First(d => d.Uri == "data://level/dlc_ex/mog/area_ravettrice_mog.ebex"));
    
        // Uniqueness is handled in recursive function so this isn't needed
        var assets = _assets
            .Where(kvp => !pcContext.AssetUris
                .Any(a => a.Uri == kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();
    
        //var assets = _assets.Select(kvp => kvp.Key).ToList();
        var json = JsonConvert.SerializeObject(assets, Formatting.Indented);
        File.WriteAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json", json);
    
        json = JsonConvert.SerializeObject(_modifiedEarcs);
        File.WriteAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\modified_ebex_earcs.json", json);
    }
    
    private void CreateEarcRecursively(FestivalDependency ebex)
    {
        lock (ebex)
        {
            if (_dependencies.ContainsKey(ebex.Uri))
            {
                return;
            }
            
            _dependencies.Add(ebex.Uri, true);
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
            System.Console.WriteLine($"[E] File already exists: {outputPath}");
        }
    
        Packer packer;
        if (isDuplicate)
        {
            var originalEarcLocation = pcContext.GetArchiveAbsoluteLocationByUri(ebex.Uri);
            using var unpacker = new Unpacker(originalEarcLocation);
            packer = unpacker.ToPacker();
            outputPath = originalEarcLocation;
        }
        else
        {
            packer = new Packer();
            packer.Header.Flags = 0;
            var exml = Ps4Utilities.GetFileByUri(context, ebex.Uri);
            packer.AddCompressedFile(ebex.Uri, exml, true);
        }
    
        var locker = new object();
        var children = context.FestivalDependencyFestivalDependency
            .Where(d => d.ParentId == ebex.Id)
            .Select(d => d.Child)
            .ToList();
        
        var selfReference = ebex.Uri + "@";
        
        Parallel.ForEach(children, child =>
        {
            lock (locker)
            {
                var childUri = child.Uri + "@";
                if (!packer.HasFile(childUri) && childUri != selfReference)
                {
                    packer.AddReference(child.Uri + "@", true);
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
                var path = $@"{Ps4PorterConfiguration.StagingDirectory}\Audio\Output\{Cryptography.HashFileUri64(subdependency.Uri)}.orb.{extension}";
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
            if (!packer.HasFile(uri) && uri != selfReference)
            {
                packer.AddReference(uri, !uri.EndsWith(".htpk"));
            }
        }
    
        // TODO: Create and add autoext file
    
        IOHelper.EnsureDirectoriesExistForFilePath(outputPath);
        packer.WriteToFile(outputPath);
        _modifiedEarcs.Add(outputPath);
    }
    
    private void CreateEarcRecursivelySingle(FestivalDependency ebex, Packer packer)
    {
        lock (ebex)
        {
            if (_dependencies.ContainsKey(ebex.Uri))
            {
                return;
            }
            
            _dependencies.Add(ebex.Uri, true);
        }
        
        using var context = Ps4Utilities.NewContext();
        var pcContext = new FlagrumDbContext(_pcSettings);
    
        var isDuplicate = pcContext.AssetUris.Any(a => a.Uri == ebex.Uri);

        if (isDuplicate)
        {
            if (ebex.Uri != "data://level/dlc_ex/mog/area_ravettrice_mog.ebex")
            {
                lock (_packerLock)
                {
                    packer.AddReference(ebex.Uri + "@", true);
                }
            }

            return;
        }
        
        var exml = Ps4Utilities.GetFileByUri(context, ebex.Uri);
        lock (_packerLock)
        {
            packer.AddCompressedFile(ebex.Uri, exml, true);
        }
        
        var children = context.FestivalDependencyFestivalDependency
            .Where(d => d.ParentId == ebex.Id)
            .Select(d => d.Child)
            .ToList();
        
        Parallel.ForEach(children, child =>
        {
            CreateEarcRecursivelySingle(child, packer);
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
                var path = $@"{Ps4PorterConfiguration.StagingDirectory}\Audio\Output\{Cryptography.HashFileUri64(subdependency.Uri)}.orb.{extension}";
                if (File.Exists(path))
                {
                    referenceUri = subdependency.Uri;
                }
            }

            if (referenceUri != null)
            {
                referenceDependencies.TryAdd(referenceUri, true);
            }

            var modelDependencies = context.FestivalSubdependencyFestivalModelDependency
                .Where(s => s.SubdependencyId == subdependency.Id)
                .Select(s => s.ModelDependency)
                .ToList();
            
            foreach (var modelDependency in modelDependencies)
            {
                _assets.TryAdd(modelDependency.Uri.ToLower(), true);
                var modelReferenceUri = GetExistingReferenceUri(pcContext, modelDependency.Uri);
                if (modelReferenceUri != null)
                {
                    referenceDependencies.TryAdd(modelReferenceUri, true);
                }

                var materialDependencies = context.FestivalModelDependencyFestivalMaterialDependency
                    .Where(m => m.ModelDependencyId == modelDependency.Id)
                    .Select(m => m.MaterialDependency)
                    .ToList();
                
                foreach (var materialDependency in materialDependencies)
                {
                    if (materialDependency.Uri.EndsWith(".htpk"))
                    {
                        //_assets.TryAdd(materialDependency.Uri.ToLower(), true);
                        //referenceDependencies.TryAdd(materialDependency.Uri, true);
                    }
                    else
                    {
                        _assets.TryAdd(materialDependency.Uri.ToLower(), true);
                        var materialReferenceUri = GetExistingReferenceUri(pcContext, materialDependency.Uri);
                        if (materialReferenceUri != null)
                        {
                            referenceDependencies.TryAdd(materialReferenceUri, true);
                        }
                    }
                }
            }
        }
    
        var selfReference = ebex.Uri + "@";
        foreach (var (uri, _) in referenceDependencies)
        {
            lock (_packerLock)
            {
                if (!packer.HasFile(uri) && uri != selfReference)
                {
                    packer.AddReference(uri, !uri.EndsWith(".htpk"));
                }
            }
        }
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