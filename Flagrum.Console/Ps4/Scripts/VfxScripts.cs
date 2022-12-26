using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Console.Ps4.Mogfest.Utilities;
using Flagrum.Console.Utilities;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Core.Vfx;

namespace Flagrum.Console.Ps4.Scripts;

public class VfxScripts
{
    public static void GetSuitableSampleMaterial(string vfxUri)
    {
        var originalData = MogfestUtilities.GetFileByUri(vfxUri);
        var originalDependencies = Vfx.GetDependencies(originalData).ToList();
        var originalMaterialUri = originalDependencies.First(d => d.EndsWith(".gmtl"));
        var originalMaterialData = MogfestUtilities.GetFileByUri(originalMaterialUri);
        var originalMaterial = new MaterialReader(originalMaterialData).Read();

        var extensions = originalDependencies.Select(d => d.Split('.').Last())
            .Distinct()
            .ToList();

        var matches = new ConcurrentDictionary<string, string>();
        
        new FileFinder().FindByQuery(file => file.Uri.EndsWith(".vfx"),
            (_, file) =>
            {
                var data = file.GetReadableData();
                var dependencies = Vfx.GetDependencies(data).ToList();

                if (dependencies.Count != originalDependencies.Count)
                {
                    return;
                }
                
                var areCountsSuitable = extensions.All(e => dependencies.Count(d => d.EndsWith("." + e)) 
                                                == originalDependencies.Count(d => d.EndsWith("." + e)));

                if (areCountsSuitable)
                {
                    var materialUri = dependencies.First(d => d.EndsWith(".gmtl"));
                    var materialData = MogfestUtilities.GetFileByUri(materialUri);
                    var material = new MaterialReader(materialData).Read();
                    if (DoTextureSlotsMatch(originalMaterial, material))
                    {
                        matches.TryAdd(file.Uri, materialUri);
                    }
                }
            }, true);

        foreach (var (vfx, materialUri) in matches)
        {
            System.Console.WriteLine($"{vfx}: {materialUri}");
        }
    }
    
    private static bool DoTextureSlotsMatch(Material sampleMaterial, Material material)
    {
        foreach (var texture in material.Textures.Where(t => !t.Path.EndsWith(".sb")))
        {
            if (texture.ShaderGenName.Contains("LayeredMask0"))
            {
                continue;
            }

            if (!sampleMaterial.Textures.Any(t =>
                    t.ShaderGenName.Replace("_", "") == texture.ShaderGenName.Replace("_", "")))
            {
                return false;
            }
        }

        return true;
    }
}