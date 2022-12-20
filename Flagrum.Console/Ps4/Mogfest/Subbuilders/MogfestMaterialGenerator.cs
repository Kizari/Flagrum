using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Console.Ps4.Mogfest.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
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

namespace Flagrum.Console.Ps4.Porting;

/// <summary>
/// This requires that the copy of Flagrum setup for PS4 files has
/// indexed the current release build of the game
/// </summary>
public class MogfestMaterialGenerator
{
    private const string _fallbackMaterial =
        "data://environment/insomnia/props/in_ar_build05/materials/in_ar_build05_typa_fence_alpha_mat.gmtl";

    private readonly ConcurrentDictionary<string, Material> _materialCache = new();
    private readonly ConcurrentDictionary<string, string> _materialMap = new();
    private readonly SettingsService _pcSettings = new() {GamePath = Ps4PorterConfiguration.ReleaseGamePath};
    private readonly ConcurrentBag<string> _stubbornMaterials = new();
    private readonly ConcurrentDictionary<string, VertexLayoutType> _vertexLayoutTypes = new();
    private ConcurrentBag<string> _non4KMaterials = new();
    private List<Ps4VertexLayoutTypeMap> _releaseMaterials;
    private readonly ConcurrentDictionary<string, FmodFragment> _shaders = new();
    private ConcurrentDictionary<string, bool> _shadersToIgnore;
    private readonly ConcurrentDictionary<string, ArchiveFile> _shaderMap = new();

    /// <summary>
    /// Requires that assets.json has been built by Ps4EarcPorter
    /// </summary>
    public void BuildVertexLayoutMapForFinalAssets()
    {
        using var context = Ps4Utilities.NewContext();
        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;
        assets = assets.Where(a => a.EndsWith(".gmdl")).ToList();

        Parallel.ForEach(assets, uri =>
        {
            using var innerContext = Ps4Utilities.NewContext();
            var gmdl = Ps4Utilities.GetFileByUri(innerContext, uri);

            if (gmdl.Length == 0)
            {
                System.Console.WriteLine($"[W] {uri} had no data!");
                return;
            }

            var gpubin = Ps4Utilities.GetFileByUri(innerContext, uri.Replace(".gmdl", ".gpubin"));
            var model = new ModelReader(gmdl, gpubin).Read();
            foreach (var subdependency in model.Header.Dependencies)
            {
                if (subdependency.Path.EndsWith(".gmtl"))
                {
                    var hash = ulong.Parse(subdependency.PathHash);
                    var mesh = model.MeshObjects
                        .Select(mo => mo.Meshes
                            .FirstOrDefault(m => m.DefaultMaterialHash == hash))
                        .FirstOrDefault();

                    _vertexLayoutTypes.TryAdd(subdependency.Path, mesh?.VertexLayoutType ?? VertexLayoutType.NULL);
                }
            }
        });

        json = JsonConvert.SerializeObject(_vertexLayoutTypes);
        File.WriteAllText($@"{Ps4PorterConfiguration.StagingDirectory}\vertex_layout_type_map.json", json);
    }

    public void FindStubbornMatches()
    {
        using var context = Ps4Utilities.NewContext();
        _releaseMaterials = context.AssetUris
            .Where(a => a.Uri.EndsWith(".gmtl"))
            .GroupJoin(context.Ps4VertexLayoutTypeMaps,
                a => a.Uri,
                m => m.Uri,
                (a, m) => new {a, m})
            .SelectMany(r => r.m.DefaultIfEmpty(),
                (r, m) => new Ps4VertexLayoutTypeMap
                {
                    Uri = r.a.Uri,
                    VertexLayoutType = m == null ? VertexLayoutType.NULL : m.VertexLayoutType
                })
            .ToList();

        var json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\vertex_layout_type_map.json");
        var vertexLayoutTypeMap = JsonConvert.DeserializeObject<ConcurrentDictionary<string, VertexLayoutType>>(json)!;

        Parallel.ForEach(_releaseMaterials, material =>
        {
            using var innerContext = new FlagrumDbContext(_pcSettings, Ps4Constants.DatabasePath);
            var data = innerContext.GetFileByUri(material.Uri);
            _materialCache[material.Uri] = new MaterialReader(data).Read();
        });

        // var uris = new[]
        // {
        //     "data://character/pr/pr56/model_002/materials/al_pr_mog01_flag_b.gmtl",
        //     "data://character/pr/pr82/model_000/materials/pr82_000_backscatter_foliage_00_mat.gmtl",
        //     "data://character/ds/ds32/model_000/materials/ds32_body_mat.gmtl",
        //     "data://character/ds/ds36/model_000/materials/ds36_body_mat.gmtl",
        //     "data://character/um/um20/model_002/materials/um20_002_basic_00_mat.gmtl",
        //     "data://character/ds/ds30/model_000/materials/mat_target_01a.gmtl",
        //     "data://character/pr/pr82/model_001/materials/pr82_001_backscatter_foliage_00_mat.gmtl",
        //     "data://character/um/um20/model_001/materials/um20_001_basic_00_mat.gmtl"
        // };

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\stubborn_materials.json");
        var uris = JsonConvert.DeserializeObject<List<string>>(json)!;

        Parallel.ForEach(uris, uri =>
        {
            using var innerContext = Ps4Utilities.NewContext();
            var data = Ps4Utilities.GetFileByUri(innerContext, uri);
            var material = new MaterialReader(data).Read();
            vertexLayoutTypeMap.TryGetValue(uri, out var vertexLayoutType);
            var match = FindMatchingMaterialLoose(uri, material, vertexLayoutType);
            _materialMap.TryAdd(uri, match);
        });

        json = JsonConvert.SerializeObject(_materialMap);
        File.WriteAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map2.json", json);
    }

    /// <summary>
    /// Requires that assets.json is built after running Ps4EarcPorter
    /// </summary>
    public void BuildMaterialMap()
    {
        using var context = Ps4Utilities.NewContext();
        _releaseMaterials = context.AssetUris
            .Where(a => a.Uri.EndsWith(".gmtl"))
            .GroupJoin(context.Ps4VertexLayoutTypeMaps,
                a => a.Uri,
                m => m.Uri,
                (a, m) => new {a, m})
            .SelectMany(r => r.m.DefaultIfEmpty(),
                (r, m) => new Ps4VertexLayoutTypeMap
                {
                    Uri = r.a.Uri,
                    VertexLayoutType = m == null ? VertexLayoutType.NULL : m.VertexLayoutType
                })
            .ToList();

        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;
        assets = assets.Where(a => a.EndsWith(".gmtl")).ToList();

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\vertex_layout_type_map.json");
        var vertexLayoutTypeMap = JsonConvert.DeserializeObject<ConcurrentDictionary<string, VertexLayoutType>>(json)!;

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\non_4K_materials.json");
        _non4KMaterials = JsonConvert.DeserializeObject<ConcurrentBag<string>>(json);

        _releaseMaterials = _releaseMaterials.Where(m => _non4KMaterials.Contains(m.Uri)).ToList();

        Parallel.ForEach(_releaseMaterials, material =>
        {
            using var innerContext = new FlagrumDbContext(_pcSettings, Ps4Constants.DatabasePath);
            var data = innerContext.GetFileByUri(material.Uri);
            _materialCache[material.Uri] = new MaterialReader(data).Read();
        });

        Parallel.ForEach(assets, materialUri =>
        {
            var ps4MaterialData = MogfestUtilities.GetFileByUri(materialUri);
            var ps4Material = new MaterialReader(ps4MaterialData).Read();

            vertexLayoutTypeMap.TryGetValue(materialUri, out var vertexLayoutType);
            var match = FindMatchingMaterial(materialUri, ps4Material, vertexLayoutType);
            _materialMap.TryAdd(materialUri, match);
        });

        json = JsonConvert.SerializeObject(_materialMap);
        File.WriteAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map.json", json);

        json = JsonConvert.SerializeObject(_stubbornMaterials);
        File.WriteAllText($@"{Ps4PorterConfiguration.StagingDirectory}\stubborn_materials.json", json);
    }

    private VertexLayoutType GetSkinningType(string uri, VertexLayoutType type)
    {
        var types = new List<VertexLayoutType>();

        if (type.HasFlag(VertexLayoutType.Skinning_1Bones))
        {
            types.Add(VertexLayoutType.Skinning_1Bones);
        }

        if (type.HasFlag(VertexLayoutType.Skinning_4Bones))
        {
            types.Add(VertexLayoutType.Skinning_4Bones);
        }

        if (type.HasFlag(VertexLayoutType.Skinning_6Bones))
        {
            types.Add(VertexLayoutType.Skinning_6Bones);
        }

        if (type.HasFlag(VertexLayoutType.Skinning_8Bones))
        {
            types.Add(VertexLayoutType.Skinning_8Bones);
        }

        if (type.HasFlag(VertexLayoutType.Skinning_Any))
        {
            types.Add(VertexLayoutType.Skinning_Any);
        }

        if (types.Count > 1)
        {
            throw new Exception(
                $"{uri} had multiple skinning types: {string.Join(", ", types.Select(t => t.ToString()))}");
        }

        return types.Any() ? types[0] : VertexLayoutType.NULL;
    }

    private string FindMatchingMaterialLoose(string uri, Material material, VertexLayoutType vertexLayoutType)
    {
        string match = null;
        var cancellationTokenSource = new CancellationTokenSource();
        var options = new ParallelOptions {CancellationToken = cancellationTokenSource.Token};

        var skinningType = GetSkinningType(uri, vertexLayoutType);

        try
        {
            Parallel.ForEach(_releaseMaterials.Where(m => m.VertexLayoutType.HasFlag(skinningType)), options,
                releaseAsset =>
                {
                    var releaseMaterial = _materialCache[releaseAsset.Uri];

                    if (!DoTextureSlotsMatch(releaseMaterial, material))
                    {
                        return;
                    }

                    match = releaseAsset.Uri;
                    cancellationTokenSource.Cancel();
                });
        }
        catch (OperationCanceledException e)
        {
            // Breaks out of the loop
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        if (match == null)
        {
            System.Console.WriteLine($"[E] Could not find match for {uri}");
            return _fallbackMaterial;
        }

        return match;
    }
    
    private IEnumerable<string> FindMatchingMaterialsLoose(string uri, Material material, VertexLayoutType vertexLayoutType)
    {
        var results = new ConcurrentBag<string>();
        string match = null;
        var cancellationTokenSource = new CancellationTokenSource();
        var options = new ParallelOptions {CancellationToken = cancellationTokenSource.Token};

        var skinningType = GetSkinningType(uri, vertexLayoutType);

        try
        {
            Parallel.ForEach(_releaseMaterials.Where(m => m.VertexLayoutType.HasFlag(skinningType)), options,
                releaseAsset =>
                {
                    var releaseMaterial = _materialCache[releaseAsset.Uri];

                    if (!DoTextureSlotsMatch(releaseMaterial, material))
                    {
                        return;
                    }

                    results.Add(releaseAsset.Uri);
                });
        }
        catch (OperationCanceledException e)
        {
            // Breaks out of the loop
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        return results;
    }

    private string FindMatchingMaterial(string uri, Material material, VertexLayoutType vertexLayoutType)
    {
        string match = null;
        var cancellationTokenSource = new CancellationTokenSource();
        var options = new ParallelOptions {CancellationToken = cancellationTokenSource.Token};

        var skinningType = GetSkinningType(uri, vertexLayoutType);

        try
        {
            Parallel.ForEach(_releaseMaterials.Where(m => m.VertexLayoutType.HasFlag(skinningType)), options,
                releaseAsset =>
                {
                    var releaseMaterial = _materialCache[releaseAsset.Uri];

                    if (releaseMaterial.Interfaces[0].Name != material.Interfaces[0].Name)
                    {
                        return;
                    }

                    match = releaseAsset.Uri;
                    cancellationTokenSource.Cancel();
                });
        }
        catch (OperationCanceledException e)
        {
            // Breaks out of the loop
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        if (match == null)
        {
            System.Console.WriteLine($"[E] Could not find match for {uri}");
            _stubbornMaterials.Add(uri);
            return _fallbackMaterial;
        }

        return match;
    }
    
    private IEnumerable<string> FindMatchingMaterials(string uri, Material material, VertexLayoutType vertexLayoutType)
    {
        string match = null;
        var cancellationTokenSource = new CancellationTokenSource();
        var options = new ParallelOptions {CancellationToken = cancellationTokenSource.Token};
        var matches = new ConcurrentBag<string>();

        var skinningType = GetSkinningType(uri, vertexLayoutType);

        try
        {
            Parallel.ForEach(_releaseMaterials.Where(m => m.VertexLayoutType.HasFlag(skinningType)), options,
                releaseAsset =>
                {
                    var releaseMaterial = _materialCache[releaseAsset.Uri];

                    if (releaseMaterial.Interfaces[0].Name != material.Interfaces[0].Name)
                    {
                        return;
                    }

                    matches.Add(releaseAsset.Uri);
                });
        }
        catch (OperationCanceledException e)
        {
            // Breaks out of the loop
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        return matches;
    }

    private bool DoTextureSlotsMatch(Material releaseMaterial, Material material)
    {
        foreach (var texture in material.Textures.Where(t =>
                     !t.Path.EndsWith(".sb") && !t.Path.StartsWith("data://shader")))
        {
            if (texture.ShaderGenName.Contains("LayeredMask0"))
            {
                continue;
            }

            if (!releaseMaterial.Textures.Any(t =>
                    t.ShaderGenName.Replace("_", "") == texture.ShaderGenName.Replace("_", "")))
            {
                return false;
            }
        }

        return true;
    }

    public void DumpMaterialsByMatch(IEnumerable<string> ps4MaterialUris)
    {
        using var pcContext = new FlagrumDbContext(new SettingsService());
        var mod = pcContext.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .First(m => m.Id == 293);

        foreach (var earc in mod.Earcs)
        {
            foreach (var file in earc.Files)
            {
                pcContext.Remove(file);
            }

            pcContext.Remove(earc);
        }

        pcContext.SaveChanges();
        
        // Map out the default shaders so they aren't packed in
        _shadersToIgnore = new ConcurrentDictionary<string, bool>(pcContext.AssetUris
            .Where(a => a.ArchiveLocation.Path == @"shader\shadergen\autoexternal.earc")
            .ToDictionary(a => a.Uri, a => true));
        
        using var context = Ps4Utilities.NewContext();
        _releaseMaterials = context.AssetUris
            .Where(a => a.Uri.EndsWith(".gmtl"))
            .GroupJoin(context.Ps4VertexLayoutTypeMaps,
                a => a.Uri,
                m => m.Uri,
                (a, m) => new {a, m})
            .SelectMany(r => r.m.DefaultIfEmpty(),
                (r, m) => new Ps4VertexLayoutTypeMap
                {
                    Uri = r.a.Uri,
                    VertexLayoutType = m == null ? VertexLayoutType.NULL : m.VertexLayoutType
                })
            .ToList();

        var json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\vertex_layout_type_map.json");
        var vertexLayoutTypeMap = JsonConvert.DeserializeObject<ConcurrentDictionary<string, VertexLayoutType>>(json)!;

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\non_4K_materials.json");
        _non4KMaterials = JsonConvert.DeserializeObject<ConcurrentBag<string>>(json);

        _releaseMaterials = _releaseMaterials.Where(m => _non4KMaterials.Contains(m.Uri)).ToList();

        Parallel.ForEach(_releaseMaterials, material =>
        {
            using var innerContext = new FlagrumDbContext(_pcSettings, Ps4Constants.DatabasePath);
            var data = innerContext.GetFileByUri(material.Uri);
            _materialCache[material.Uri] = new MaterialReader(data).Read();
        });

        Parallel.ForEach(ps4MaterialUris, ps4MaterialUri =>
        {
            var ps4MaterialData = MogfestUtilities.GetFileByUri(ps4MaterialUri);
            var ps4Material = new MaterialReader(ps4MaterialData).Read();

            vertexLayoutTypeMap.TryGetValue(ps4MaterialUri, out var vertexLayoutType);
            var matches = FindMatchingMaterials(ps4MaterialUri, ps4Material, vertexLayoutType)
                .Select(m => new {Uri = m, IsStrict = true}).ToList();
            var matches2 = FindMatchingMaterialsLoose(ps4MaterialUri, ps4Material, vertexLayoutType)
                .Select(m => new {Uri = m, IsStrict = false}).ToList();

            if (matches.Count > 20)
            {
                matches = matches.Take(20).ToList();
            }
            
            if (matches2.Count > 20)
            {
                matches2 = matches2.Take(20).ToList();
            }

            if (matches.Count < 20)
            {
                foreach (var match in matches2.Where(match => !matches.Any(m => m.Uri == match.Uri)))
                {
                    matches.Add(match);
                }
            }

            Parallel.ForEach(matches, matchUri =>
            {
                using var pcContext = new FlagrumDbContext(_pcSettings, Ps4Constants.DatabasePath);
                var materialData = pcContext.GetFileByUri(matchUri.Uri);
                var material = new MaterialReader(materialData).Read();
            
                material.HighTexturePackAsset = ps4Material.HighTexturePackAsset;
                material.Name = ps4Material.Name;
                material.NameHash = ps4Material.NameHash;
                material.Uri = ps4MaterialUri;
            
                foreach (var input in ps4Material.InterfaceInputs)
                {
                    var match = material.InterfaceInputs.FirstOrDefault(i => i.ShaderGenName == input.ShaderGenName);
                    if (match != null)
                    {
                        match.Values = input.Values;
                    }
                }
            
                // Update all the texture slots with the PS4 textures
                foreach (var texture in material.Textures.Where(t => !t.Path.EndsWith(".sb")))
                {
                    var match = ps4Material.Textures.FirstOrDefault(t => t.ShaderGenName.Replace("_", "").ToLower() == texture.ShaderGenName.Replace("_", "").ToLower());
                    if (match == null)
                    {
                        if (!texture.Path.StartsWith("data://shader"))
                        {
                            texture.Path = texture.ShaderGenName.Contains("MRS") ? "data://shader/defaulttextures/white.tif" : "data://shader/defaulttextures/black.tif";
                            texture.PathHash = Cryptography.Hash32("data://shader/defaulttextures/black.tif");
                            texture.ResourceFileHash =
                                Cryptography.HashFileUri64("data://shader/defaulttextures/black.tif");
                        }
                    }
                    else
                    {
                        texture.Path = match.Path;
                        texture.PathHash = match.PathHash;
                        texture.ResourceFileHash = match.ResourceFileHash;
                    }
                }
            
                material.RegenerateDependencyTable();
            
                var materialResult = new MaterialWriter(material).Write();
                var outPath =
                    @$"C:\Modding\Chocomog\Testing\MaterialGenerator\{ps4MaterialUri.Split('/').Last()}\{(matchUri.IsStrict ? "Strict" : "Non-Strict")}\{matchUri.Uri.Split('/').Last()}.gfxbin";
                IOHelper.EnsureDirectoriesExistForFilePath(outPath);
                File.WriteAllBytes(outPath, materialResult);
                
                foreach (var shader in material.Header.Dependencies
                             .Where(d => d.Path.EndsWith(".sb")))
                {
                    if (_shadersToIgnore.ContainsKey(shader.Path))
                    {
                        continue;
                    }

                    if (!_shaderMap.TryGetValue(shader.Path, out var shaderFile))
                    {
                        shaderFile = pcContext.GetArchiveFileByUri(shader.Path);
                        _shaderMap[shader.Path] = shaderFile;
                    }

                    var shaderFragment = new FmodFragment
                    {
                        OriginalSize = shaderFile.Size,
                        ProcessedSize = shaderFile.ProcessedSize,
                        Flags = shaderFile.Flags,
                        Key = shaderFile.Key,
                        RelativePath = shaderFile.RelativePath,
                        Data = shaderFile.GetRawData()
                    };

                    _shaders[shader.Path] = shaderFragment;
                }
            });
        });

        var shaderEarc = new EarcModEarc
        {
            EarcRelativePath = @"shader\shadergen\autoexternal.earc",
            Type = EarcChangeType.Change
        };

        // Write shader fragments
        foreach (var (uri, fragment) in _shaders)
        {
            var hash = Cryptography.HashFileUri64(uri);
            var fragmentPath = $@"C:\Modding\Chocomog\Testing\ShaderFragments\{hash}.ffg";

            if (!File.Exists(fragmentPath))
            {
                fragment.Write(fragmentPath);
            }
            
            shaderEarc.Files.Add(new EarcModFile
            {
                Uri = uri,
                ReplacementFilePath = fragmentPath,
                Type = EarcFileChangeType.Add,
                Flags = ArchiveFileFlag.Autoload
            });
        }
        
        mod.Earcs.Add(shaderEarc);
        pcContext.SaveChanges();
    }
}