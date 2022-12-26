using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Console.Ps4.Mogfest.Utilities;
using Flagrum.Console.Utilities;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Core.Vfx;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Porting;

public static class Ps4VfxDependencyMapper
{
    private static readonly ConcurrentDictionary<string, byte[]> _vfxCache = new();
    
    public static void Map()
    {
        // Cache all vfx
        new FileFinder().FindByQuery(file => file.Uri.EndsWith(".vfx"),
            (_, file) =>
            {
                var data = file.GetReadableData();
                _vfxCache[file.Uri] = data;
            }, true);

        using var context = new FlagrumDbContext(new SettingsService());
        var allVfxMaterialUris = context.AssetUris
            .Where(a => a.Uri.EndsWith(".gmtl") && a.Uri.Contains("vfx_"))
            .Select(a => a.Uri)
            .ToList();
        
        // Map the vfx materials back to the vfx files
        var results = new Dictionary<string, string>();
        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!.Where(a => a.EndsWith(".vfx"));

        foreach (var asset in assets)
        {
            var vfx = MogfestUtilities.GetFileByUri(asset);
            var dependencies = Vfx.GetDependencies(vfx).Where(d => d.EndsWith(".gmtl"));

            foreach (var dependency in dependencies)
            {
                if (!allVfxMaterialUris.Any(u => u.Equals(dependency, StringComparison.OrdinalIgnoreCase)))
                {
                    results.TryAdd(dependency, asset);
                }
            }
        }

        var materialMap = new ConcurrentDictionary<string, string>();
        Parallel.ForEach(results, kvp =>
        {
            var (materialUri, vfxUri) = kvp;
            materialMap.TryAdd(materialUri, GetSuitableSampleMaterial(materialUri, vfxUri));
        });

        json = JsonConvert.SerializeObject(materialMap, Formatting.Indented);
        File.WriteAllText(@"C:\Modding\Chocomog\Testing\vfx_map.json", json);
    }
    
    private static string GetSuitableSampleMaterial(string originalMaterialUri, string vfxUri)
    {
        var originalData = MogfestUtilities.GetFileByUri(vfxUri);
        var originalDependencies = Vfx.GetDependencies(originalData).ToList();
        var originalMaterialData = MogfestUtilities.GetFileByUri(originalMaterialUri);
        var originalMaterial = new MaterialReader(originalMaterialData).Read();

        var extensions = originalDependencies.Select(d => d.Split('.').Last())
            .Distinct()
            .ToList();

        var matches = new ConcurrentDictionary<string, string>();
        using var context = new FlagrumDbContext(new SettingsService());

        foreach (var (_, data) in _vfxCache)
        {
            var dependencies = Vfx.GetDependencies(data).ToList();
            if (dependencies.Count != originalDependencies.Count)
            {
                continue;
            }
                
            var areCountsSuitable = extensions.All(e => dependencies.Count(d => d.EndsWith("." + e)) 
                                                        == originalDependencies.Count(d => d.EndsWith("." + e)));

            if (areCountsSuitable)
            {
                foreach (var materialUri in dependencies.Where(d => d.EndsWith(".gmtl")))
                {
                    var materialData = context.GetFileByUri(materialUri);
                    if (materialData.Length == 0)
                    {
                        System.Console.WriteLine($"Couldn't find material at {materialUri}");
                        continue;
                    }
                    
                    var material = new MaterialReader(materialData).Read();
                    if (DoTextureSlotsMatch(originalMaterial, material))
                    {
                        matches.TryAdd(originalMaterialUri, materialUri);
                    }
                }
            }
        }

        return matches.First().Value;
    }
    
    private static bool DoTextureSlotsMatch(Material sampleMaterial, Material material)
    {
        foreach (var texture in material.Textures.Where(t => !t.Path.EndsWith(".sb")))
        {
            if (!sampleMaterial.Textures.Any(t =>
                    t.ShaderGenName.Replace("_", "") == texture.ShaderGenName.Replace("_", "")))
            {
                return false;
            }
        }

        return true;
    }
}