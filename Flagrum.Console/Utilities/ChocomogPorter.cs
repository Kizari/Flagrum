using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Black.Entity;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Ps4;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using SQEX.Ebony.Framework.Entity;
using BinaryReader = Flagrum.Core.Gfxbin.Serialization.BinaryReader;

namespace Flagrum.Console.Utilities;

public static class ChocomogPorter
{
    private const string Ps4DatabasePath = @"C:\Users\Kieran\AppData\Local\Flagrum-PS4\flagrum.db";
    private const string GameDirectory = @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\dummy.exe";
    private const string DatasDirectory = @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\datas";
    private const string OutputDirectory = @"C:\Modding\Chocomog\Testing";

    private static readonly SettingsService _settings = new();
    private static readonly ConcurrentDictionary<string, bool> _dependencies = new();
    private static readonly ConcurrentDictionary<string, Unpacker> _unpackers = new();
    private static readonly object _lock = new();
    private static byte[] _materialTemplateData;
    private static SettingsService _pcSettings;

    public static async Task Run()
    {
        _settings.GamePath = GameDirectory;
        _pcSettings = new SettingsService();

        //Step1BuildUriMapFromPs4Files();
        //Step2BuildDependencyList();
        //Step3BuildSubdependencyList();
        //Step4GetModelDependencies();
        //Step5GetMaterialDependencies();
        //Step6AggregateDependencyTables();
        //Step7FilterMissingDependencies();

        //await using var context = new FlagrumDbContext(_pcSettings);
        // using var unpacker =
        //     new Unpacker(
        //         @"C:\Program Files (x86)\Steam\steamapps\common\Final Fantasy XV\datas\environment\altissia\ebex\map_al_al_c_env.earc");
        // var matches = unpacker.Files.Where(f => f.Uri.Contains("mogcho1"));
        // var packer = unpacker.ToPacker();
        // packer.AddReference("data://environment/altissia/props/al_pr_mogcho1/materials/autoexternal.ebex@");
        // packer.AddReference("data://environment/altissia/props/al_pr_mogcho1/sourceimages/autoexternal.ebex@");
        // packer.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\Final Fantasy XV\datas\environment\altissia\ebex\map_al_al_c_env.earc");
        //_materialTemplateData = context.GetFileByUri(
        //    "data://environment/leide/props/le_ar_gqshop1/materials/le_ar_gqshop1_ground_01_wood.gmtl");

        PortTest4();
        //PortPs4Asset();

        // await using var context = new FlagrumDbContext(_pcSettings);
        // var data = context.GetFileByUri(
        //     "data://environment/altissia/props/al_ar_port01/models/al_ar_port01_boardA.gmdl");
        // var data2 = context.GetFileByUri(
        //     "data://environment/altissia/props/al_ar_port01/models/al_ar_port01_boardA.gpubin");
        //
        // var packer = new Packer();
        // packer.Header.Flags = ArchiveHeaderFlags.HasLooseData;
        // packer.AddCompressedFile("data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl",
        //     data);
        // packer.AddCompressedFile("data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gpubin",
        //     data2);
        //
        // //var basePath = @"C:\Modding\Chocomog\Testing\datas";
        // var basePath = _pcSettings.GameDataDirectory;
        // var modelEarcPath = basePath + '\\' + IOHelper
        //     .UriToRelativePath("data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl")
        //     .Replace("al_pr_mogcho_gamegate.gmdl", "autoexternal.earc");
        // IOHelper.EnsureDirectoriesExistForFilePath(modelEarcPath);
        // packer.WriteToFile(modelEarcPath);
        //
        // var path = _pcSettings.GameDataDirectory + @"\environment\altissia\ebex\map_al_al_c_env.earc";
        // var backupPath = path.Replace(".earc", ".backup");
        // using var unpacker = new Unpacker(backupPath);
        // var envPacker = unpacker.ToPacker();
        // envPacker.AddReference("data://environment/altissia/props/al_pr_mogcho1/models/autoexternal.ebex@");
        // envPacker.WriteToFile(path);
    }

    /// <summary>
    /// Run this if you need to generate the URI map for the PS4 database
    /// </summary>
    private static void Step1BuildUriMapFromPs4Files()
    {
        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        var mapper = new UriMapper(context, _settings);
        mapper.UsePs4Mode();
        mapper.RegenerateMap();
    }

    /// <summary>
    /// Run this if you need to build the dependency list for the mogchoco festival
    /// </summary>
    private static void Step2BuildDependencyList()
    {
        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        context.ClearTable(nameof(context.FestivalDependencies));
        var uri = "data://level/dlc_ex/mog/area_ravettrice_mog.ebex";
        var data = GetFileByUri(context, uri);
        TraverseEbexTree(uri, data);
        var result = _dependencies.Select(kvp => new FestivalDependency {Uri = kvp.Key});
        context.FestivalDependencies.AddRange(result);
        context.SaveChanges();
    }

    /// <summary>
    /// Run this if you need to get all dependencies from the ebexes in the main dependency list
    /// </summary>
    private static void Step3BuildSubdependencyList()
    {
        using var outerContext = new FlagrumDbContext(_settings, Ps4DatabasePath);
        outerContext.ClearTable(nameof(outerContext.FestivalSubdependencies));
        var dependencies = outerContext.FestivalDependencies.Select(d => d.Uri).ToList();
        var subdependencies = new ConcurrentDictionary<string, bool>();

        Parallel.ForEach(dependencies, dependency =>
        {
            using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
            var xmb2 = GetFileByUri(context, dependency);
            var root = Xmb2Document.GetRootElement(xmb2);
            var objects = root.GetElementByName("objects");
            var elements = objects.GetElements();

            foreach (var element in elements)
            {
                var typeAttribute = element.GetAttributeByName("type").GetTextValue();
                var type = Assembly.GetAssembly(typeof(EntityPackageReference))!.GetTypes()
                    .FirstOrDefault(t => t.FullName?.Contains(typeAttribute) == true);
                if (!type.IsAssignableTo(typeof(EntityPackageReference)))
                {
                    var path = element.GetElementByName("sourcePath_")?.GetTextValue();
                    if (path != null && !path.EndsWith(".ebex"))
                    {
                        string combinedUriString;
                        if (path.StartsWith('.'))
                        {
                            var uriUri = new Uri(dependency.Replace("data://", "data://data/"));
                            var combinedUri = new Uri(uriUri, path);
                            combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                        }
                        else
                        {
                            combinedUriString = $"data://{path}";
                        }

                        subdependencies.TryAdd(combinedUriString, true);
                    }

                    if (type.IsAssignableTo(typeof(StaticModelEntity)))
                    {
                        var collisionPath = element.GetElementByName("MeshCollision_")?.GetTextValue();
                        if (collisionPath != null)
                        {
                            string combinedUriString;
                            if (collisionPath.StartsWith('.'))
                            {
                                var uriUri = new Uri(dependency.Replace("data://", "data://data/"));
                                var combinedUri = new Uri(uriUri, collisionPath);
                                combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                            }
                            else
                            {
                                combinedUriString = $"data://{collisionPath}";
                            }

                            subdependencies.TryAdd(combinedUriString, true);
                        }
                    }
                }
            }
        });

        var result = subdependencies.Select(s => new FestivalSubdependency {Uri = s.Key});
        outerContext.FestivalSubdependencies.AddRange(result);
        outerContext.SaveChanges();
    }

    /// <summary>
    /// Run this if you need to get all model dependencies from the subdependencies list
    /// </summary>
    private static void Step4GetModelDependencies()
    {
        var dependencies = new ConcurrentDictionary<string, bool>();

        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        context.ClearTable(nameof(context.FestivalModelDependencies));

        Parallel.ForEach(context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".gmdl")), dependency =>
        {
            using var innerContext = new FlagrumDbContext(_settings, Ps4DatabasePath);
            var data = GetFileByUri(innerContext, dependency.Uri);

            if (data.Length == 0)
            {
                return;
            }

            var header = new GfxbinHeader();
            header.Read(new BinaryReader(data));
            foreach (var subdependency in header.Dependencies.Where(subdependency =>
                         subdependency.PathHash != "asset_uri" && subdependency.PathHash != "ref"))
            {
                dependencies.TryAdd(subdependency.Path, true);
            }
        });

        context.FestivalModelDependencies.AddRange(dependencies.Select(kvp => new FestivalModelDependency
        {
            Uri = kvp.Key
        }));

        context.SaveChanges();
    }

    /// <summary>
    /// Run this if you need to get all material dependencies from the model dependencies list
    /// </summary>
    private static void Step5GetMaterialDependencies()
    {
        var dependencies = new ConcurrentDictionary<string, bool>();

        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        context.ClearTable(nameof(context.FestivalMaterialDependencies));

        Parallel.ForEach(context.FestivalModelDependencies.Where(d => d.Uri.EndsWith(".gmtl")), dependency =>
        {
            using var innerContext = new FlagrumDbContext(_settings, Ps4DatabasePath);
            var data = GetFileByUri(innerContext, dependency.Uri);
            var header = new GfxbinHeader();
            header.Read(new BinaryReader(data));
            foreach (var subdependency in header.Dependencies.Where(subdependency =>
                         subdependency.PathHash != "asset_uri" && subdependency.PathHash != "ref"))
            {
                dependencies.TryAdd(subdependency.Path, true);
            }
        });

        context.FestivalMaterialDependencies.AddRange(dependencies.Select(kvp => new FestivalMaterialDependency
        {
            Uri = kvp.Key
        }));

        context.SaveChanges();
    }

    /// <summary>
    /// Run this if you need to aggregate all the dependency tables into one
    /// </summary>
    private static void Step6AggregateDependencyTables()
    {
        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        context.ClearTable(nameof(context.FestivalAllDependencies));

        var levelDependencies = context.FestivalDependencies
            .Select(d => new FestivalAllDependency {Uri = d.Uri})
            .ToList();

        var ebexDependencies = context.FestivalSubdependencies
            .Select(d => new FestivalAllDependency {Uri = d.Uri})
            .ToList();

        var modelDependencies = context.FestivalModelDependencies
            .Select(d => new FestivalAllDependency {Uri = d.Uri})
            .ToList();

        var materialDependencies = context.FestivalMaterialDependencies
            .Select(d => new FestivalAllDependency {Uri = d.Uri})
            .ToList();

        var allDependencies = levelDependencies
            .Union(ebexDependencies)
            .Union(modelDependencies)
            .Union(materialDependencies)
            .DistinctBy(d => d.Uri.ToLower())
            .ToList();

        context.FestivalAllDependencies.AddRange(allDependencies);
        context.SaveChanges();
    }

    /// <summary>
    /// This compares the full dependency list to the asset list in the release build to filter
    /// Down to only the missing assets
    /// </summary>
    private static void Step7FilterMissingDependencies()
    {
        using var pcContext = new FlagrumDbContext(new SettingsService());
        using var ps4Context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        ps4Context.ClearTable(nameof(ps4Context.FestivalFinalDependencies));

        var list = new List<string>();

        foreach (var asset in ps4Context.FestivalAllDependencies)
        {
            if (!pcContext.AssetUris.Any(a => EF.Functions.Like(a.Uri, asset.Uri)))
            {
                list.Add(asset.Uri);
            }
        }

        ps4Context.FestivalFinalDependencies.AddRange(list.Select(s => new FestivalFinalDependency {Uri = s}));
        ps4Context.SaveChanges();
    }

    private static void TraverseEbexTree(string uri, byte[] xmb2)
    {
        if (xmb2[0] == 'd')
        {
            System.Console.WriteLine($"[W] {uri} was deleted!");
            return;
        }

        _dependencies.TryAdd(uri, true);
        var root = Xmb2Document.GetRootElement(xmb2);
        var objects = root.GetElementByName("objects");
        var elements = objects.GetElements();

        Parallel.ForEach(elements, element =>
        {
            // Ignore the self-declaration
            if (element == elements.First())
            {
                return;
            }

            var typeAttribute = element.GetAttributeByName("type").GetTextValue();
            var type = Assembly.GetAssembly(typeof(EntityPackageReference))!.GetTypes()
                .FirstOrDefault(t => t.FullName?.Contains(typeAttribute) == true);

            if (type == typeof(EntityPackageReference))
            {
                var path = element.GetElementByName("sourcePath_");
                var relativeUri = path.GetTextValue();
                var uriUri = new Uri(uri.Replace("data://", "data://data/"));
                var combinedUri = new Uri(uriUri, relativeUri);
                var combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");

                byte[] innerXmb2;
                using (var context = new FlagrumDbContext(_settings, Ps4DatabasePath))
                {
                    innerXmb2 = GetFileByUri(context, combinedUriString);
                }

                if (innerXmb2.Length > 0)
                {
                    TraverseEbexTree(combinedUriString, innerXmb2);
                }
            }
        });

        GC.Collect();
    }

    private static byte[] GetFileByUri(FlagrumDbContext context, string uri)
    {
        var uriPattern = $"%{uri}";
        var earcRelativePath = context.AssetUris
            .Where(a => EF.Functions.Like(a.Uri, uriPattern))
            .Select(a => a.ArchiveLocation.Path)
            .FirstOrDefault();

        if (earcRelativePath == null)
        {
            return Array.Empty<byte>();
        }

        Unpacker unpacker;
        var location = DatasDirectory + "\\" + earcRelativePath;
        lock (_lock)
        {
            if (!_unpackers.TryGetValue(location, out unpacker))
            {
                unpacker = new Unpacker(location);
                _ = unpacker.Files; // Forces file headers to read
                _unpackers[location] = unpacker;
            }
        }

        return unpacker.UnpackReadableByUri(uri);
    }

    private static void PortTest()
    {
        using var pcContext = new FlagrumDbContext(_pcSettings);
        var basePath = _pcSettings.GameDataDirectory;
        var finalAssetList = new ConcurrentDictionary<string, byte[]>();

        var materialData =
            pcContext.GetFileByUri("data://environment/altissia/props/al_ar_port01/materials/al_ar_port01_01.gmtl");
        var material = new MaterialReader(materialData).Read();
        material.HighTexturePackAsset = "";
        material.Name = "al_pr_mogcho_gamegatea_ma";
        material.NameHash = Cryptography.Hash32(material.Name);
        var uriDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
        uriDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/materials/";
        var refDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
        refDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/materials/al_pr_mogcho_gamegatea_ma.gmtl";

        foreach (var dependency in material.Header.Dependencies.Where(d =>
                     d.PathHash is not "asset_uri" and not "ref" && !d.Path.Contains("/shader/")))
        {
            var textureData = pcContext.GetFileByUri(dependency.Path);

            if (!dependency.Path.Contains("data://environment/altissia/props/al_ar_port01"))
            {
                throw new Exception("That ain't gonna work!");
            }

            var textureUri = dependency.Path.Replace("/al_ar_port01/", "/al_pr_mogcho1/");
            var textureHash = Cryptography.HashFileUri64(textureUri);

            var matches = material.Textures.Where(t => t.Path == dependency.Path);
            foreach (var match in matches)
            {
                match.Path = textureUri;
                match.ResourceFileHash = textureHash;
                match.PathHash = Cryptography.Hash32(textureUri);
            }

            dependency.Path = textureUri;
            dependency.PathHash = textureHash.ToString();

            finalAssetList.TryAdd(textureUri, textureData);
        }

        material.Header.Hashes = material.Header.Dependencies
            .Where(d => ulong.TryParse(d.PathHash, out _))
            .Select(d => ulong.Parse(d.PathHash))
            .OrderBy(h => h)
            .ToList();

        materialData = new MaterialWriter(material).Write();
        var materialUri = "data://environment/altissia/props/al_pr_mogcho1/materials/al_pr_mogcho_gamegatea_ma.gmtl";
        finalAssetList.TryAdd(materialUri, materialData);

        var gmdl = "data://environment/altissia/props/al_ar_port01/models/al_ar_port01_boardA.gmdl";
        var gpubin = gmdl.Replace(".gmdl", ".gpubin");
        var gmdlData = pcContext.GetFileByUri(gmdl);
        var gpubinData = pcContext.GetFileByUri(gpubin);
        var model = new ModelReader(gmdlData, gpubinData).Read();

        for (var i = model.Header.Dependencies.Count - 1; i >= 0; i--)
        {
            var current = model.Header.Dependencies[i];
            if (current.Path.EndsWith(".gmtl"))
            {
                model.Header.Hashes.Remove(ulong.Parse(current.PathHash));
                model.Header.Dependencies.Remove(current);
            }
        }

        var materialHash = Cryptography.HashFileUri64(materialUri);
        model.Header.Dependencies.Insert(0,
            new DependencyPath {Path = materialUri, PathHash = materialHash.ToString()});
        model.Header.Hashes.Insert(0, materialHash);

        foreach (var meshObject in model.MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                mesh.DefaultMaterialHash = materialHash;
            }
        }

        uriDependency = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
        uriDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/";
        refDependency = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
        refDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl";
        var gpubinDependency = model.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith(".gpubin"))!;
        var originalGpubinHash = ulong.Parse(gpubinDependency.PathHash);
        gpubinDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gpubin";
        var gpubinHash = Cryptography.HashFileUri64(gpubinDependency.Path);
        gpubinDependency.PathHash = gpubinHash.ToString();
        model.Header.Hashes.Remove(originalGpubinHash);
        model.Header.Hashes.Add(gpubinHash);
        model.AssetHash = gpubinHash;

        (gmdlData, gpubinData) = new ModelWriter(model).Write();

        finalAssetList.TryAdd("data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl",
            gmdlData);
        finalAssetList.TryAdd("data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gpubin",
            gpubinData);

        var earcs = new Dictionary<string, Dictionary<string, byte[]>>();
        foreach (var (assetUri, data) in finalAssetList)
        {
            if (data == null)
            {
                continue;
            }

            var folder = assetUri[..assetUri.LastIndexOf('/')];
            earcs.TryGetValue(folder, out var dictionary);

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, byte[]>();
                earcs.Add(folder, dictionary);
            }

            dictionary.Add(assetUri, data);
        }

        foreach (var (folderUri, items) in earcs)
        {
            var earcPath = $@"{basePath}\{IOHelper.UriToRelativePath(folderUri)}\autoexternal.earc";

            if (earcPath.Contains("\\defaulttextures\\"))
            {
                System.Console.WriteLine($"File already exists at {earcPath}");
                continue;
            }

            IOHelper.EnsureDirectoriesExistForFilePath(earcPath);
            var packer = new Packer();
            packer.Header.Flags = ArchiveHeaderFlags.HasLooseData;

            foreach (var (fileUri, data) in items)
            {
                packer.AddCompressedFile(fileUri, data);
            }

            System.Console.WriteLine($"Writing EARC {earcPath}");
            packer.WriteToFile(earcPath);
        }
    }

    private static void PortPs4Asset(
        string uri = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl")
    {
        var finalAssetList = new ConcurrentDictionary<string, byte[]>();
        //var basePath = @"C:\Modding\Chocomog\Testing\datas";
        var basePath = _pcSettings.GameDataDirectory;

        using var outerContext = new FlagrumDbContext(_settings, Ps4DatabasePath);
        var gmdl = uri;
        var gpubin = gmdl.Replace(".gmdl", ".gpubin");
        var gmdlData = GetFileByUri(outerContext, gmdl);
        var gpubinData = GetFileByUri(outerContext, gpubin);
        var model = new ModelReader(gmdlData, gpubinData).Read();
        (gmdlData, gpubinData) = new ModelWriter(model).Write();
        finalAssetList.TryAdd(gmdl, gmdlData);
        finalAssetList.TryAdd(gpubin, gpubinData);

        var modelHeader = new GfxbinHeader();
        modelHeader.Read(new BinaryReader(gmdlData));

        Parallel.ForEach(modelHeader.Dependencies.Where(d => d.PathHash is not "asset_uri" and not "ref"),
            dependency =>
            {
                using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
                if (dependency.PathHash != "asset_uri" && dependency.PathHash != "ref")
                {
                    var data = GetFileByUri(context, dependency.Path);

                    if (dependency.Path.EndsWith(".gpubin"))
                    {
                        //modelPacker.AddCompressedFile(dependency.Path, data);
                        //finalAssetList.Add(dependency.Path);
                    }
                    else
                    {
                        var originalMaterial = new MaterialReader(data).Read();
                        var material = new MaterialReader(_materialTemplateData).Read();

                        foreach (var subdependency in originalMaterial.Header.Dependencies)
                        {
                            if (subdependency.PathHash != "asset_uri" && subdependency.PathHash != "ref")
                            {
                                if (!subdependency.Path.EndsWith(".sb") &&
                                    !finalAssetList.ContainsKey(subdependency.Path))
                                {
                                    var match = originalMaterial.Textures.FirstOrDefault(t =>
                                        t.Path == subdependency.Path)!;
                                    material.UpdateTexture(match.ShaderGenName, match.Path);
                                    var subdata = GetFileByUri(context, subdependency.Path);
                                    var btex = Btex.FromData(subdata);
                                    using var stream = new MemoryStream();
                                    btex.Bitmap.Save(stream, ImageFormat.Png);
                                    var converter = new TextureConverter();
                                    var btexData = converter.PngToBtex(btex, stream.ToArray());
                                    finalAssetList.TryAdd(subdependency.Path, btexData);
                                }
                                else if (subdependency.Path.EndsWith(".sb"))
                                {
                                    finalAssetList.TryAdd(subdependency.Path, null);
                                }
                            }
                        }

                        material.HighTexturePackAsset = string.Empty;
                        material.HighTexturePackAssetOffset = 0;
                        material.UpdateNameAndUri(originalMaterial.Name, dependency.Path);
                        material.RegenerateDependencyTable();

                        var materialBytes = new MaterialWriter(material).Write();
                        File.WriteAllBytes($@"{OutputDirectory}\Raw\materials\{originalMaterial.Name}.gmtl.gfxbin",
                            materialBytes);
                        finalAssetList.TryAdd(dependency.Path, materialBytes);
                    }
                }
            });

        var earcs = new Dictionary<string, Dictionary<string, byte[]>>();
        foreach (var (assetUri, data) in finalAssetList)
        {
            if (data == null)
            {
                continue;
            }

            var folder = assetUri[..assetUri.LastIndexOf('/')];
            earcs.TryGetValue(folder, out var dictionary);

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, byte[]>();
                earcs.Add(folder, dictionary);
            }

            dictionary.Add(assetUri, data);
        }

        var path = _pcSettings.GameDataDirectory + @"\environment\altissia\ebex\map_al_al_c_env.earc";
        var backupPath = path.Replace(".earc", ".backup");
        using var unpacker = new Unpacker(backupPath);
        var autoext = unpacker.UnpackReadableByUri("data://environment/altissia/ebex/map_al_al_c_env.autoext");
        var envPacker = unpacker.ToPacker();

        foreach (var (folderUri, items) in earcs)
        {
            var earcPath = $@"{basePath}\{IOHelper.UriToRelativePath(folderUri)}\autoexternal.earc";

            if (earcPath.Contains("\\defaulttextures\\"))
            {
                System.Console.WriteLine($"File already exists at {earcPath}");
                continue;
            }

            IOHelper.EnsureDirectoriesExistForFilePath(earcPath);
            var packer = new Packer();
            packer.Header.Flags = ArchiveHeaderFlags.HasLooseData;

            foreach (var (fileUri, data) in items)
            {
                packer.AddCompressedFile(fileUri, data);
            }

            packer.WriteToFile(earcPath);
            envPacker.AddReference($"{folderUri}/autoexternal.ebex@");
        }

        var originalAssetListString = Encoding.UTF8.GetString(autoext);
        var assetList = originalAssetListString.Split(' ').Select(s => s.Trim()).ToList();
        foreach (var (assetUri, _) in finalAssetList)
        {
            if (!assetList.Any(a => a.Equals(assetUri, StringComparison.OrdinalIgnoreCase)))
            {
                assetList.Add(assetUri);
            }
        }

        var result = string.Join(' ', assetList);
        var resultBytes = Encoding.UTF8.GetBytes(result);
        envPacker.UpdateFile("data://environment/altissia/ebex/map_al_al_c_env.autoext", resultBytes);
        envPacker.WriteToFile(path);
    }

    private static void PortTest2()
    {
        using var ps4Context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        using var pcContext = new FlagrumDbContext(_pcSettings);

        var basePath = _pcSettings.GameDataDirectory;
        var finalAssetList = new ConcurrentDictionary<string, byte[]>();

        var materialData =
            pcContext.GetFileByUri("data://environment/altissia/props/al_ar_port01/materials/al_ar_port01_01.gmtl");
        var material = new MaterialReader(materialData).Read();
        material.HighTexturePackAsset = "";
        material.Name = "al_pr_mogcho_gamegatea_ma";
        material.NameHash = Cryptography.Hash32(material.Name);
        var uriDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
        uriDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/materials/";
        var refDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
        refDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/materials/al_pr_mogcho_gamegatea_ma.gmtl";

        foreach (var dependency in material.Header.Dependencies.Where(d =>
                     d.PathHash is not "asset_uri" and not "ref" && !d.Path.Contains("/shader/")))
        {
            var textureData = pcContext.GetFileByUri(dependency.Path);

            var textureUri = dependency.Path.Replace("/al_ar_port01/", "/al_pr_mogcho1/");
            var textureHash = Cryptography.HashFileUri64(textureUri);

            var matches = material.Textures.Where(t => t.Path == dependency.Path);
            foreach (var match in matches)
            {
                match.Path = textureUri;
                match.ResourceFileHash = textureHash;
                match.PathHash = Cryptography.Hash32(textureUri);
            }

            dependency.Path = textureUri;
            dependency.PathHash = textureHash.ToString();

            finalAssetList.TryAdd(textureUri, textureData);
        }

        material.Header.Hashes = material.Header.Dependencies
            .Where(d => ulong.TryParse(d.PathHash, out _))
            .Select(d => ulong.Parse(d.PathHash))
            .OrderBy(h => h)
            .ToList();

        materialData = new MaterialWriter(material).Write();
        var materialUri = "data://environment/altissia/props/al_pr_mogcho1/materials/al_pr_mogcho_gamegatea_ma.gmtl";
        finalAssetList.TryAdd(materialUri, materialData);

        var gmdl = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl";
        var gpubin = gmdl.Replace(".gmdl", ".gpubin");
        var gmdlData = GetFileByUri(ps4Context, gmdl);
        var gpubinData = GetFileByUri(ps4Context, gpubin);
        var model = new ModelReader(gmdlData, gpubinData).Read();

        for (var i = model.Header.Dependencies.Count - 1; i >= 0; i--)
        {
            var current = model.Header.Dependencies[i];
            if (current.Path.EndsWith(".gmtl"))
            {
                model.Header.Hashes.Remove(ulong.Parse(current.PathHash));
                model.Header.Dependencies.Remove(current);
            }
        }

        var materialHash = Cryptography.HashFileUri64(materialUri);
        model.Header.Dependencies.Insert(0,
            new DependencyPath {Path = materialUri, PathHash = materialHash.ToString()});
        model.Header.Hashes.Insert(0, materialHash);

        foreach (var meshObject in model.MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                mesh.DefaultMaterialHash = materialHash;
            }
        }

        uriDependency = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
        uriDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/";
        refDependency = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
        refDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl";
        var gpubinDependency = model.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith(".gpubin"))!;
        var originalGpubinHash = ulong.Parse(gpubinDependency.PathHash);
        gpubinDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gpubin";
        var gpubinHash = Cryptography.HashFileUri64(gpubinDependency.Path);
        gpubinDependency.PathHash = gpubinHash.ToString();
        model.Header.Hashes.Remove(originalGpubinHash);
        model.Header.Hashes.Add(gpubinHash);
        model.AssetHash = gpubinHash;

        (gmdlData, gpubinData) = new ModelWriter(model).Write();

        finalAssetList.TryAdd("data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl",
            gmdlData);
        finalAssetList.TryAdd("data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gpubin",
            gpubinData);

        var earcs = new Dictionary<string, Dictionary<string, byte[]>>();
        foreach (var (assetUri, data) in finalAssetList)
        {
            if (data == null)
            {
                continue;
            }

            var folder = assetUri[..assetUri.LastIndexOf('/')];
            earcs.TryGetValue(folder, out var dictionary);

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, byte[]>();
                earcs.Add(folder, dictionary);
            }

            dictionary.Add(assetUri, data);
        }

        var envPath = @$"{_pcSettings.GameDataDirectory}\environment\altissia\ebex\map_al_al_c_env.earc";
        using var unpacker = new Unpacker(envPath);
        var envPacker = unpacker.ToPacker();

        foreach (var (folderUri, items) in earcs)
        {
            var earcPath = $@"{basePath}\{IOHelper.UriToRelativePath(folderUri)}\autoexternal.earc";

            if (earcPath.Contains("\\defaulttextures\\"))
            {
                System.Console.WriteLine($"File already exists at {earcPath}");
                continue;
            }

            IOHelper.EnsureDirectoriesExistForFilePath(earcPath);
            var packer = new Packer();
            packer.Header.Flags = ArchiveHeaderFlags.HasLooseData;

            foreach (var (fileUri, data) in items)
            {
                packer.AddCompressedFile(fileUri, data);
            }

            System.Console.WriteLine($"Writing EARC {earcPath}");
            packer.WriteToFile(earcPath);

            var referenceUri = $"{folderUri}\autoexternal.ebex@";
            if (!envPacker.HasFile(referenceUri))
            {
                envPacker.AddReference(referenceUri);
            }
        }

        System.Console.WriteLine($"Writing EARC {envPath}");
        envPacker.WriteToFile(envPath);
    }

    private static void PortTest3()
    {
        using var ps4Context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        using var pcContext = new FlagrumDbContext(_pcSettings);

        var basePath = _pcSettings.GameDataDirectory;
        var finalAssetList = new ConcurrentDictionary<string, byte[]>();

        var gmdl = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl";
        var gmdlData = GetFileByUri(ps4Context, gmdl);
        var modelHeader = new GfxbinHeader();
        modelHeader.Read(new BinaryReader(gmdlData));

        var materialData =
            pcContext.GetFileByUri(
                "data://environment/leide/props/le_ar_gqshop1/materials/le_ar_gqshop1_curtain_cloth.gmtl");

        foreach (var dependency in modelHeader.Dependencies.Where(d => d.Path.EndsWith(".gmtl")))
        {
            var originalMaterialData = GetFileByUri(ps4Context, dependency.Path);
            var originalMaterial = new MaterialReader(originalMaterialData).Read();

            if (!string.IsNullOrEmpty(originalMaterial.HighTexturePackAsset))
            {
                System.Console.WriteLine($"[W] High texture pack detected: {originalMaterial.HighTexturePackAsset}");
            }
            else
            {
                System.Console.WriteLine("[W] No high texture pack detected!");
            }

            var material = new MaterialReader(materialData).Read();
            material.HighTexturePackAsset = "";
            material.Name = originalMaterial.Name;
            material.NameHash = originalMaterial.NameHash;
            var originalUriDependency =
                originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
            var uriDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
            uriDependency.Path = originalUriDependency.Path;
            var originalRefDependency = originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
            var refDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
            refDependency.Path = originalRefDependency.Path;

            foreach (var subdependency in material.Header.Dependencies.Where(d =>
                         d.PathHash is not "asset_uri" and not "ref" && !d.Path.Contains("/shader/")))
            {
                var textureSlot = material.Textures.FirstOrDefault(t => t.Path == subdependency.Path)!.ShaderGenName;
                var originalUri = originalMaterial.Textures.FirstOrDefault(t => t.ShaderGenName == textureSlot)!.Path;
                var originalMatch = originalMaterial.Header.Dependencies.FirstOrDefault(d => d.Path == originalUri)!;

                if (originalMatch == null)
                {
                    throw new Exception("That ain't gonna work!");
                }

                var textureUri = originalMatch.Path;
                var textureHash = ulong.Parse(originalMatch.PathHash);
                var subdata = GetFileByUri(ps4Context, textureUri);
                var btex = Btex.FromData(subdata);
                using var stream = new MemoryStream();
                btex.Bitmap.Save(stream, ImageFormat.Png);
                var converter = new TextureConverter();
                var textureData = converter.PngToBtex(btex, stream.ToArray());

                var matches = material.Textures.Where(t => t.Path == subdependency.Path);
                foreach (var match in matches)
                {
                    match.Path = textureUri;
                    match.ResourceFileHash = textureHash;
                    match.PathHash = Cryptography.Hash32(textureUri);
                }

                subdependency.Path = textureUri;
                subdependency.PathHash = textureHash.ToString();

                finalAssetList.TryAdd(textureUri, textureData);
            }

            material.Header.Hashes = material.Header.Dependencies
                .Where(d => ulong.TryParse(d.PathHash, out _))
                .Select(d => ulong.Parse(d.PathHash))
                .OrderBy(h => h)
                .ToList();

            materialData = new MaterialWriter(material).Write();
            finalAssetList.TryAdd(originalRefDependency.Path, materialData);
        }

        var gpubin = gmdl.Replace(".gmdl", ".gpubin");
        var gpubinData = GetFileByUri(ps4Context, gpubin);
        // var model = new ModelReader(gmdlData, gpubinData).Read();
        //
        // for (var i = model.Header.Dependencies.Count - 1; i >= 0; i--)
        // {
        //     var current = model.Header.Dependencies[i];
        //     if (current.Path.EndsWith(".gmtl"))
        //     {
        //         model.Header.Hashes.Remove(ulong.Parse(current.PathHash));
        //         model.Header.Dependencies.Remove(current);
        //     }
        // }
        //
        // var materialHash = Cryptography.HashFileUri64(materialUri);
        // model.Header.Dependencies.Insert(0, new DependencyPath {Path = materialUri, PathHash = materialHash.ToString()});
        // model.Header.Hashes.Insert(0, materialHash);
        //
        // foreach (var meshObject in model.MeshObjects)
        // {
        //     foreach (var mesh in meshObject.Meshes)
        //     {
        //         mesh.DefaultMaterialHash = materialHash;
        //     }
        // }
        //
        // uriDependency = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
        // uriDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/";
        // refDependency = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
        // refDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl";
        // var gpubinDependency = model.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith(".gpubin"))!;
        // var originalGpubinHash = ulong.Parse(gpubinDependency.PathHash);
        // gpubinDependency.Path = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gpubin";
        // var gpubinHash = Cryptography.HashFileUri64(gpubinDependency.Path);
        // gpubinDependency.PathHash = gpubinHash.ToString();
        // model.Header.Hashes.Remove(originalGpubinHash);
        // model.Header.Hashes.Add(gpubinHash);
        // model.AssetHash = gpubinHash;
        //
        // (gmdlData, gpubinData) = new ModelWriter(model).Write();

        finalAssetList.TryAdd(gmdl, gmdlData);
        finalAssetList.TryAdd(gpubin, gpubinData);

        var earcs = new Dictionary<string, Dictionary<string, byte[]>>();
        foreach (var (assetUri, data) in finalAssetList)
        {
            if (data == null)
            {
                continue;
            }

            var folder = assetUri[..assetUri.LastIndexOf('/')];
            earcs.TryGetValue(folder, out var dictionary);

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, byte[]>();
                earcs.Add(folder, dictionary);
            }

            dictionary.Add(assetUri, data);
        }

        var envPath = @$"{_pcSettings.GameDataDirectory}\environment\altissia\ebex\map_al_al_c_env.earc";
        using var unpacker = new Unpacker(envPath);
        var envPacker = unpacker.ToPacker();

        foreach (var (folderUri, items) in earcs)
        {
            var earcPath = $@"{basePath}\{IOHelper.UriToRelativePath(folderUri)}\autoexternal.earc";

            if (earcPath.Contains("\\defaulttextures\\"))
            {
                System.Console.WriteLine($"File already exists at {earcPath}");
                continue;
            }

            IOHelper.EnsureDirectoriesExistForFilePath(earcPath);
            var packer = new Packer();
            packer.Header.Flags = ArchiveHeaderFlags.HasLooseData;

            foreach (var (fileUri, data) in items)
            {
                packer.AddCompressedFile(fileUri, data);
            }

            if (File.Exists(earcPath))
            {
                var backupPath = earcPath.Replace(".earc", ".backup");
                if (!File.Exists(backupPath))
                {
                    File.Move(earcPath, backupPath);
                }

                System.Console.WriteLine($"[E] {earcPath} already exists!");
            }

            System.Console.WriteLine($"Writing EARC {earcPath}");
            packer.WriteToFile(earcPath);

            var referenceUri = $"{folderUri}/autoexternal.ebex@";
            if (!envPacker.HasFile(referenceUri))
            {
                System.Console.WriteLine($"Adding reference {referenceUri}");
                envPacker.AddReference(referenceUri);
            }
        }

        System.Console.WriteLine($"Writing EARC {envPath}");
        envPacker.WriteToFile(envPath);
    }

    private static void PortTest4()
    {
        using var ps4Context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        using var pcContext = new FlagrumDbContext(_pcSettings);

        var basePath = _pcSettings.GameDataDirectory;
        var finalAssetList = new ConcurrentDictionary<string, byte[]>();

        var gmdl = "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho_gamegate.gmdl";
        var gmdlData = GetFileByUri(ps4Context, gmdl);
        var modelHeader = new GfxbinHeader();
        modelHeader.Read(new BinaryReader(gmdlData));

        var materialData =
            pcContext.GetFileByUri(
                "data://environment/leide/props/le_ar_gqshop1/materials/le_ar_gqshop1_curtain_cloth.gmtl");

        foreach (var dependency in modelHeader.Dependencies.Where(d => d.Path.EndsWith(".gmtl")))
        {
            var originalMaterialData = GetFileByUri(ps4Context, dependency.Path);
            var originalMaterial = new MaterialReader(originalMaterialData).Read();

            var material = new MaterialReader(materialData).Read();
            material.HighTexturePackAsset = originalMaterial.HighTexturePackAsset;
            material.Name = originalMaterial.Name;
            material.NameHash = originalMaterial.NameHash;
            var originalUriDependency =
                originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
            var uriDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
            uriDependency.Path = originalUriDependency.Path;
            var originalRefDependency = originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
            var refDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
            refDependency.Path = originalRefDependency.Path;
            var highTextures = new List<string>();

            foreach (var subdependency in material.Header.Dependencies.Where(d =>
                         d.PathHash is not "asset_uri" and not "ref" && !d.Path.Contains("/shader/")))
            {
                var textureSlot = material.Textures.FirstOrDefault(t => t.Path == subdependency.Path)!.ShaderGenName;
                var originalUri = originalMaterial.Textures.FirstOrDefault(t => t.ShaderGenName == textureSlot)!.Path;
                var originalMatch = originalMaterial.Header.Dependencies.FirstOrDefault(d => d.Path == originalUri)!;

                if (originalMatch == null)
                {
                    throw new Exception("That ain't gonna work!");
                }

                var textureUri = originalMatch.Path;
                var textureHash = ulong.Parse(originalMatch.PathHash);
                var subdata = GetFileByUri(ps4Context, textureUri);
                var btex = Btex.FromData(subdata);
                using var stream = new MemoryStream();
                btex.Bitmap.Save(stream, ImageFormat.Png);
                var converter = new TextureConverter();
                var textureData = converter.PngToBtex(btex, stream.ToArray());

                var highTextureUri = textureUri.Insert(textureUri.LastIndexOf('.'), "_$h");
                var highTextureData = GetFileByUri(ps4Context, highTextureUri);
                if (highTextureData.Length > 0)
                {
                    var highBtex = Btex.FromData(highTextureData);
                    using var highStream = new MemoryStream();
                    highBtex.Bitmap.Save(highStream, ImageFormat.Png);
                    var highConverter = new TextureConverter();
                    var highTexture = highConverter.PngToBtex(highBtex, highStream.ToArray());
                    highTextures.Add(highTextureUri);
                    finalAssetList.TryAdd(highTextureUri, highTexture);
                }

                var matches = material.Textures.Where(t => t.Path == subdependency.Path);
                foreach (var match in matches)
                {
                    match.Path = textureUri;
                    match.ResourceFileHash = textureHash;
                    match.PathHash = Cryptography.Hash32(textureUri);
                }

                subdependency.Path = textureUri;
                subdependency.PathHash = textureHash.ToString();

                finalAssetList.TryAdd(textureUri, textureData);
            }

            var htpk = Encoding.UTF8.GetBytes(string.Join(' ', highTextures));
            finalAssetList.TryAdd(originalMaterial.HighTexturePackAsset, htpk);

            material.Header.Hashes = material.Header.Dependencies
                .Where(d => ulong.TryParse(d.PathHash, out _))
                .Select(d => ulong.Parse(d.PathHash))
                .OrderBy(h => h)
                .ToList();

            materialData = new MaterialWriter(material).Write();
            finalAssetList.TryAdd(originalRefDependency.Path, materialData);
        }

        var gpubin = gmdl.Replace(".gmdl", ".gpubin");
        var gpubinData = GetFileByUri(ps4Context, gpubin);

        finalAssetList.TryAdd(gmdl, gmdlData);
        finalAssetList.TryAdd(gpubin, gpubinData);

        var earcs = new Dictionary<string, Dictionary<string, byte[]>>();
        foreach (var (assetUri, data) in finalAssetList)
        {
            if (data == null || assetUri.EndsWith("_$h.htpk"))
            {
                continue;
            }

            var folder = assetUri[..assetUri.LastIndexOf('/')];
            earcs.TryGetValue(folder, out var dictionary);

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, byte[]>();
                earcs.Add(folder, dictionary);
            }

            dictionary.Add(assetUri, data);
        }

        foreach (var (assetUri, data) in finalAssetList.Where(kvp => kvp.Key.EndsWith("_$h.htpk")))
        {
            var referenceUri = assetUri[..assetUri.LastIndexOf('/')];
            referenceUri = referenceUri[..referenceUri.LastIndexOf('/')] + "/sourceimages/autoexternal.ebex@";
            var earcPath = $@"{basePath}\{IOHelper.UriToRelativePath(assetUri).Replace(".htpk", ".earc")}";
            var packer = new Packer();
            packer.Header.Flags = 0;
            packer.AddReference(referenceUri);
            packer.AddAutoloadFile(assetUri.Replace(".htpk", ".autoext"), data);
            System.Console.WriteLine($"Writing EARC {earcPath}");
            packer.WriteToFile(earcPath);
        }

        var envPath = @$"{_pcSettings.GameDataDirectory}\environment\altissia\ebex\map_al_al_c_env.earc";
        using var unpacker = new Unpacker(envPath);
        var envPacker = unpacker.ToPacker();

        foreach (var (folderUri, items) in earcs)
        {
            var earcPath = $@"{basePath}\{IOHelper.UriToRelativePath(folderUri)}\autoexternal.earc";

            if (earcPath.Contains("\\defaulttextures\\"))
            {
                System.Console.WriteLine($"File already exists at {earcPath}");
                continue;
            }

            IOHelper.EnsureDirectoriesExistForFilePath(earcPath);
            var packer = new Packer();
            packer.Header.Flags = ArchiveHeaderFlags.HasLooseData;

            foreach (var (fileUri, data) in items)
            {
                packer.AddCompressedFile(fileUri, data);
            }

            if (File.Exists(earcPath))
            {
                var backupPath = earcPath.Replace(".earc", ".backup");
                if (!File.Exists(backupPath))
                {
                    File.Move(earcPath, backupPath);
                }

                System.Console.WriteLine($"[E] {earcPath} already exists!");
            }

            System.Console.WriteLine($"Writing EARC {earcPath}");
            packer.WriteToFile(earcPath);

            var referenceUri = $"{folderUri}/autoexternal.ebex@";
            if (!envPacker.HasFile(referenceUri))
            {
                System.Console.WriteLine($"Adding reference {referenceUri}");
                envPacker.AddReference(referenceUri);
            }
        }

        System.Console.WriteLine($"Writing EARC {envPath}");
        envPacker.WriteToFile(envPath);
    }
}