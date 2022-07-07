using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Newtonsoft.Json;
using BinaryReader = Flagrum.Core.Gfxbin.Serialization.BinaryReader;

namespace Flagrum.Console.Ps4.Porting;

/// <summary>
/// This requires that the copy of Flagrum setup for PS4 files has
/// indexed the current release build of the game
/// </summary>
public class Ps4MaterialGenerator
{
    private const string _fallbackMaterial =
        "data://environment/insomnia/props/in_ar_build05/materials/in_ar_build05_typa_fence_alpha_mat.gmtl";

    private readonly ConcurrentDictionary<string, Material> _materialCache = new();
    private readonly ConcurrentDictionary<string, string> _materialMap = new();
    private readonly SettingsService _pcSettings = new() {GamePath = Ps4PorterConfiguration.ReleaseGamePath};
    private readonly ConcurrentDictionary<string, VertexLayoutType> _vertexLayoutTypes = new();
    private ConcurrentBag<string> _non4KMaterials = new();
    private List<Ps4VertexLayoutTypeMap> _releaseMaterials;

    public void BuildNon4KMaterialList()
    {
        using var outerContext = new FlagrumDbContext(_pcSettings, Ps4Constants.DatabasePath);
        var materialUris = outerContext.AssetUris
            .Where(a => a.Uri.EndsWith(".gmtl"))
            .Select(a => a.Uri)
            .ToList();

        Parallel.ForEach(materialUris, uri =>
        {
            using var context = new FlagrumDbContext(_pcSettings, Ps4Constants.DatabasePath);
            var data = context.GetFileByUri(uri);
            var material = new MaterialReader(data).Read();

            if (!Has4KTextures(material, context))
            {
                _non4KMaterials.Add(uri);
            }
        });

        var json = JsonConvert.SerializeObject(_non4KMaterials);
        File.WriteAllText($@"{Ps4PorterConfiguration.StagingDirectory}\non_4K_materials.json", json);
    }

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

    /// <summary>
    /// Requires that the PS4 files have already been indexed
    /// </summary>
    public void BuildVertexLayoutMap()
    {
        var releaseSettings = new SettingsService {GamePath = Ps4PorterConfiguration.ReleaseGamePath};
        using var outerContext = new FlagrumDbContext(releaseSettings, Ps4Constants.DatabasePath);
        var results = new ConcurrentDictionary<string, VertexLayoutType>();

        Parallel.ForEach(outerContext.AssetUris.Where(a => a.Uri.EndsWith(".gmdl")), asset =>
        {
            using var context = new FlagrumDbContext(releaseSettings, Ps4Constants.DatabasePath);
            var gmdl = context.GetFileByUri(asset.Uri);

            var reader = new BinaryReader(gmdl);
            var header = new GfxbinHeader();
            header.Read(reader);

            var gpubinUri = header.Dependencies.FirstOrDefault(d => d.Path.EndsWith(".gpubin"))?.Path;
            if (gpubinUri == null)
            {
                return;
            }

            var gpubin = context.GetFileByUri(gpubinUri);
            var model = new ModelReader(gmdl, gpubin).Read();

            foreach (var meshObject in model.MeshObjects)
            {
                foreach (var mesh in meshObject.Meshes)
                {
                    var hash = mesh.DefaultMaterialHash.ToString();
                    var uri = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == hash)!.Path;
                    results.TryAdd(uri, mesh.VertexLayoutType);
                }
            }
        });

        foreach (var (uri, type) in results)
        {
            outerContext.Ps4VertexLayoutTypeMaps.Add(new Ps4VertexLayoutTypeMap
            {
                Uri = uri,
                VertexLayoutType = type
            });
        }

        outerContext.SaveChanges();
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

        var uris = new[]
        {
            "data://character/pr/pr56/model_002/materials/al_pr_mog01_flag_b.gmtl",
            "data://character/pr/pr82/model_000/materials/pr82_000_backscatter_foliage_00_mat.gmtl",
            "data://character/ds/ds32/model_000/materials/ds32_body_mat.gmtl",
            "data://character/ds/ds36/model_000/materials/ds36_body_mat.gmtl",
            "data://character/um/um20/model_002/materials/um20_002_basic_00_mat.gmtl",
            "data://character/ds/ds30/model_000/materials/mat_target_01a.gmtl",
            "data://character/pr/pr82/model_001/materials/pr82_001_backscatter_foliage_00_mat.gmtl",
            "data://character/um/um20/model_001/materials/um20_001_basic_00_mat.gmtl"
        };

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
            using var innerContext = Ps4Utilities.NewContext();
            var ps4MaterialData = Ps4Utilities.GetFileByUri(innerContext, materialUri);
            var ps4Material = new MaterialReader(ps4MaterialData).Read();

            vertexLayoutTypeMap.TryGetValue(materialUri, out var vertexLayoutType);
            var match = FindMatchingMaterial(materialUri, ps4Material, vertexLayoutType);
            _materialMap.TryAdd(materialUri, match);
        });

        json = JsonConvert.SerializeObject(_materialMap);
        File.WriteAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map.json", json);
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
            return _fallbackMaterial;
        }

        return match;
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

    private bool Has4KTextures(Material releaseMaterial, FlagrumDbContext context)
    {
        foreach (var texture in releaseMaterial.Textures.Where(t => t.Path != null && !t.Path.EndsWith(".sb")))
        {
            if (texture.Path.StartsWith("data://shader"))
            {
                continue;
            }

            var dotIndex = texture.Path.LastIndexOf('.');

            if (dotIndex < 0)
            {
                continue;
            }

            var query = texture.Path[..dotIndex].Replace("/sourceimages/", "/highimages/");
            if (context.AssetUris.Any(a => a.Uri.Contains(query)))
            {
                return true;
            }
        }

        return false;
    }
}