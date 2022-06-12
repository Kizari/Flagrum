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
using Flagrum.Core.Animation;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Ps4;
using Flagrum.Core.Utilities;
using Flagrum.Core.Vfx;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SQEX.Ebony.Framework.Entity;
using BinaryReader = Flagrum.Core.Gfxbin.Serialization.BinaryReader;

namespace Flagrum.Console.Ps4.Festivals;

public static class ChocomogPorter
{
    private const string Ps4DatabasePath = @"C:\Users\Kieran\AppData\Local\Flagrum-PS4\flagrum.db";
    private const string GameDirectory = @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\dummy.exe";

    private const string DatasDirectory = @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\datas";

    //private const string OutputDirectory = @"C:\Modding\Chocomog\Testing\datas";
    private const string OutputDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas";

    private static readonly SettingsService _settings = new();
    private static readonly object _lock = new();
    private static SettingsService _pcSettings;
    private static readonly ConcurrentDictionary<string, bool> _assets = new();
    private static byte[] _materialData;
    private static readonly ConcurrentDictionary<string, bool> _checked = new();
    private static readonly List<string> _modifiedEarcs = new();
    private static readonly ConcurrentBag<string> _modifiedEarcs2 = new();
    private static readonly ConcurrentDictionary<string, string> _existingAssets = new();

    public static async Task Run()
    {
        _settings.GamePath = GameDirectory;
        _pcSettings = new SettingsService();

        //OutputFileByUri("data://character/ds/ds35/ds35_000.amdl");
        //return;

        //OutputXmlForMatchingDependency("al_pr_candy01_type.gmdl");
        //return;

        //Flagrum.Console.Utilities.FileFinder.FindStringInExml("pr81.amdl");
        //return;

        //var counter = 0;
        //new Utilities.FileFinder().FindByQuery(file => file.Uri.Equals("data://character/pr/pr81/pr81.amdl", StringComparison.OrdinalIgnoreCase),
        //    (earc, file) =>
        //    {
        //        var i = counter++;
        //        File.WriteAllBytes(@$"C:\Modding\Chocomog\Testing\XML\pr81_000[{i}].amdl", file.GetReadableData());
        //        System.Console.WriteLine($"{i}: {earc}");
        //    },
        //    true);
        //return;

        await using var pcContext = new FlagrumDbContext(_pcSettings);
        _materialData =
            pcContext.GetFileByUri(
                "data://environment/leide/props/le_ar_gqshop1/materials/le_ar_gqshop1_curtain_cloth.gmtl");

        //await using var ps4Context = Ps4Utilities.NewContext();
        //foreach (var amdl in ps4Context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".amdl")))
        //{
        //    if (!pcContext.AssetUris.Any(a => EF.Functions.Like(a.Uri, amdl.Uri)))
        //    {
        //        File.WriteAllBytes(@$"C:\Modding\Chocomog\Testing\XML\{amdl.Uri.Split('/').Last()}", Ps4Utilities.GetFileByUri(ps4Context, amdl.Uri));
        //    }
        //}
        //return;

        //Step1BuildUriMapFromPs4Files();
        //Step2BuildDependencyList();
        //Step3BuildSubdependencyList();
        //Step4GetModelDependencies();
        //Step5GetMaterialDependencies();
        ResetFiles();
        Step6CreateEarcs();
        Step7PortAssets();
        Step9FixWeirdEarcs();
    }

    private static void OutputFileByUri(string uri)
    {
        using var context = Ps4Utilities.NewContext();
        var data = Ps4Utilities.GetFileByUri(context, uri);
        if (uri.EndsWith(".ebex"))
        {
            var output = new StringBuilder();
            Xmb2Document.Dump(data, output);
            data = Encoding.UTF8.GetBytes(output.ToString());
        }
        File.WriteAllBytes($@"C:\Modding\Chocomog\Testing\XML\{uri.Split('/').Last()}", data);
    }

    private static void OutputXmlForMatchingDependency(string query)
    {
        foreach (var file in Directory.EnumerateFiles(@"C:\Modding\Chocomog\Testing\XML"))
        {
            File.Delete(file);
        }

        using var ps4Context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        foreach (var ebex in ps4Context.FestivalDependencies)
        {
            var exml = Ps4Utilities.GetFileByUri(ps4Context, ebex.Uri);
            var sb = new StringBuilder();
            Xmb2Document.Dump(exml, sb);
            var xml = sb.ToString();
            if (xml.Contains(query))
            {
                File.WriteAllText($@"C:\Modding\Chocomog\Testing\XML\{ebex.Uri.Split('/').Last()}.xml", xml);
            }
        }
    }

    private static void Step1BuildUriMapFromPs4Files()
    {
        new Ps4Indexer().RegenerateMap();
    }

    private static void Step2BuildDependencyList()
    {
        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);

        context.ClearTable(nameof(context.FestivalDependencies));
        const string uri = "data://level/dlc_ex/mog/area_ravettrice_mog.ebex";
        var data = Ps4Utilities.GetFileByUri(context, uri);

        var root = TraverseEbexTree(uri, data, null);

        context.FestivalDependencies.Add(root);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var items = context.FestivalDependencies
            .AsNoTracking()
            .ToList()
            .DistinctBy(i => i.Uri.ToLower())
            .Select(i => i.Id)
            .ToList();

        foreach (var item in context.FestivalDependencies.AsNoTracking().ToList())
        {
            if (!items.Contains(item.Id))
            {
                context.Remove(item);
            }
            else
            {
                item.Uri = item.Uri.ToLower();
            }
        }
    }

    private static void Step3BuildSubdependencyList()
    {
        using var outerContext = new FlagrumDbContext(_settings, Ps4DatabasePath);
        outerContext.ClearTable(nameof(outerContext.FestivalSubdependencies));
        var dependencies = outerContext.FestivalDependencies.ToList();
        var subdependencies = new ConcurrentDictionary<string, int>();

        Parallel.ForEach(dependencies, dependency =>
        {
            using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
            var xmb2 = Ps4Utilities.GetFileByUri(context, dependency.Uri);
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
                    //var path = element.GetElementByName("sourcePath_")?.GetTextValue();
                    var subelements = element.GetElements();
                    var paths = subelements
                        .Where(e => e.Name.Contains("path", StringComparison.OrdinalIgnoreCase) || e.Name == "LmAnimMdlData")
                        .Select(p => p.GetTextValue());
                    foreach (var path in paths)
                    {
                        if (path != null && !path.EndsWith(".ebex"))
                        {
                            string combinedUriString;
                            if (path.StartsWith('.'))
                            {
                                var uriUri = new Uri(dependency.Uri.Replace("data://", "data://data/"));
                                var combinedUri = new Uri(uriUri, path);
                                combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                            }
                            else
                            {
                                combinedUriString = $"data://{path}";
                            }

                            subdependencies.TryAdd(combinedUriString, dependency.Id);
                        }
                    }

                    var lists = subelements
                        .Where(e => e.Name.Contains("List", StringComparison.OrdinalIgnoreCase) || e.Name == "ExtraSoundFileNames");

                    foreach (var list in lists)
                    {
                        foreach (var item in list.GetElements())
                        {
                            AddUriFromSubdependency(item, "Value", dependency, subdependencies);
                        }
                    }

                    var differences = subelements.FirstOrDefault(e => e.Name == "differences");
                    if (differences != null)
                    {
                        foreach (var difference in differences.GetElements())
                        {
                            AddUriFromSubdependency(difference, "sourcePath_", dependency, subdependencies);
                        }
                    }

                    if (type.IsAssignableTo(typeof(StaticModelEntity)))
                    {
                        AddUriFromSubdependency(element, "MeshCollision_", dependency, subdependencies);
                    }
                }
            }
        });

        var result = subdependencies.Select(s => new FestivalSubdependency
        {
            Uri = s.Key,
            ParentId = s.Value
        });

        outerContext.FestivalSubdependencies.AddRange(result);
        outerContext.SaveChanges();
    }

    private static void AddUriFromSubdependency(Xmb2Element element, string pathElementName,
        FestivalDependency dependency, ConcurrentDictionary<string, int> subdependencies)
    {
        var animPath = element.GetElementByName(pathElementName)?.GetTextValue();
        if (animPath != null)
        {
            string combinedUriString;
            if (animPath.StartsWith('.'))
            {
                var uriUri = new Uri(dependency.Uri.Replace("data://", "data://data/"));
                var combinedUri = new Uri(uriUri, animPath);
                combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
            }
            else
            {
                combinedUriString = $"data://{animPath}";
            }

            subdependencies.TryAdd(combinedUriString, dependency.Id);
        }
    }

    private static void Step4GetModelDependencies()
    {
        var dependencies = new ConcurrentDictionary<string, int>();

        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        context.ClearTable(nameof(context.FestivalModelDependencies));

        Parallel.ForEach(context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".gmdl")), dependency =>
        {
            using var innerContext = new FlagrumDbContext(_settings, Ps4DatabasePath);
            var data = Ps4Utilities.GetFileByUri(innerContext, dependency.Uri);

            if (data.Length == 0)
            {
                return;
            }

            var header = new GfxbinHeader();
            header.Read(new BinaryReader(data));
            foreach (var subdependency in header.Dependencies.Where(subdependency =>
                         subdependency.PathHash != "asset_uri" && subdependency.PathHash != "ref"))
            {
                dependencies.TryAdd(subdependency.Path, dependency.Id);
            }
        });

        foreach (var dependency in context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".vlink")))
        {
            var data = Ps4Utilities.GetFileByUri(context, dependency.Uri);
            if (data.Length == 0)
            {
                continue;
            }

            var uri = Vlink.GetVfxUriFromData(data);
            dependencies.TryAdd(uri, dependency.Id);
        }

        context.FestivalModelDependencies.AddRange(dependencies.Select(kvp => new FestivalModelDependency
        {
            Uri = kvp.Key,
            ParentId = kvp.Value
        }));
        
        context.SaveChanges();
    }

    private static void Step5GetMaterialDependencies()
    {
        var dependencies = new ConcurrentDictionary<string, int>();

        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        context.ClearTable(nameof(context.FestivalMaterialDependencies));

        Parallel.ForEach(context.FestivalModelDependencies.Where(d => d.Uri.EndsWith(".gmtl")), dependency =>
        {
            using var innerContext = new FlagrumDbContext(_settings, Ps4DatabasePath);
            var data = Ps4Utilities.GetFileByUri(innerContext, dependency.Uri);
            var material = new MaterialReader(data).Read();
            foreach (var subdependency in material.Header.Dependencies.Where(subdependency =>
                         subdependency.PathHash is not "asset_uri" and not "ref" &&
                         !subdependency.Path.EndsWith(".sb")))
            {
                dependencies.TryAdd(subdependency.Path, dependency.Id);
            }

            if (!string.IsNullOrEmpty(material.HighTexturePackAsset))
            {
                dependencies.TryAdd(material.HighTexturePackAsset, dependency.Id);
            }
        });

        context.FestivalMaterialDependencies.AddRange(dependencies.Select(kvp => new FestivalMaterialDependency
        {
            Uri = kvp.Key,
            ParentId = kvp.Value
        }));

        context.SaveChanges();
    }

    private static void Step6CreateEarcs()
    {
        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        var pcContext = new FlagrumDbContext(_pcSettings);
        CreateEarcRecursively(context.FestivalDependencies.Find(1));

        // Uniqueness is handled in recursive function so this isn't needed
        var assets = _assets.Where(kvp => !pcContext.AssetUris
                .Any(a => EF.Functions.Like(a.Uri, kvp.Key)))
            .Select(kvp => kvp.Key)
            .ToList();

        //var assets = _assets.Select(kvp => kvp.Key).ToList();
        var json = JsonConvert.SerializeObject(assets, Formatting.Indented);
        File.WriteAllText(@"C:\Modding\Chocomog\Testing\assets.json", json);

        json = JsonConvert.SerializeObject(_modifiedEarcs);
        File.WriteAllText(@"C:\Modding\Chocomog\Testing\modified_ebex_earcs.json", json);
    }

    private static void Step7PortAssets()
    {
        var json = File.ReadAllText(@"C:\Modding\Chocomog\Testing\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;
        assets = assets.Where(a => !a.EndsWith(".ani") && !a.EndsWith(".pka") && !a.EndsWith(".anmgph")).ToList();

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
            var earcPath = $@"{OutputDirectory}\{IOHelper.UriToRelativePath(folder)}\autoexternal.earc";

            Packer packer;
            if (File.Exists(earcPath))
            {
                var backupPath = earcPath.Replace(".earc", ".backup");
                if (!File.Exists(backupPath))
                {
                    File.Copy(earcPath, backupPath);
                }

                using var unpacker = new Unpacker(backupPath);
                packer = unpacker.ToPacker();
            }
            else
            {
                packer = new Packer();
                packer.Header.Flags = ArchiveHeaderFlags.HasLooseData;
            }

            foreach (var uri in uris)
            {
                if (uri.EndsWith(".htpk"))
                {
                    var htpkPacker = new Packer();
                    htpkPacker.Header.Flags = 0;

                    var htpkFolder = uri[..uri.LastIndexOf('/')];
                    var sourceimagesFolder = htpkFolder[..htpkFolder.LastIndexOf('/')] + "/sourceimages";

                    if (earcs.ContainsKey(sourceimagesFolder))
                    {
                        var htpk = Encoding.UTF8.GetBytes(string.Join(' ', earcs[sourceimagesFolder]));

                        htpkPacker.AddReference($"{sourceimagesFolder}/autoexternal.ebex@", true);
                        htpkPacker.AddAutoloadFile(uri.Replace(".htpk", ".autoext"), htpk);

                        var htpkPath =
                            $@"{OutputDirectory}\{IOHelper.UriToRelativePath(uri).Replace(".htpk", ".earc")}";
                        IOHelper.EnsureDirectoriesExistForFilePath(htpkPath);
                        htpkPacker.WriteToFile(htpkPath);
                        _modifiedEarcs2.Add(htpkPath);
                    }
                }
                else
                {
                    foreach (var (finalUri, data) in GetConvertedAssets(uri))
                    {
                        if (!packer.HasFile(finalUri))
                        {
                            packer.AddCompressedFile(finalUri, data);
                        }
                    }
                }
            }

            IOHelper.EnsureDirectoriesExistForFilePath(earcPath);
            packer.WriteToFile(earcPath);
            _modifiedEarcs2.Add(earcPath);
        });

        var modifiedJson = JsonConvert.SerializeObject(_modifiedEarcs2);
        File.WriteAllText(@"C:\Modding\Chocomog\Testing\modified_asset_earcs.json", modifiedJson);
    }

    private static void Step8CopyToGameDirectoryNoOverwrite()
    {
        var jsonPath = @"C:\Modding\Chocomog\Testing\existing_files.json";
        List<string> existingFiles;

        if (File.Exists(jsonPath))
        {
            var json = File.ReadAllText(jsonPath);
            existingFiles = JsonConvert.DeserializeObject<List<string>>(json);
        }
        else
        {
            existingFiles = Directory.GetFiles(OutputDirectory, "*.earc", SearchOption.AllDirectories)
                .Select(file => file.Replace(OutputDirectory, _pcSettings.GameDataDirectory))
                .Where(File.Exists)
                .ToList();

            var json = JsonConvert.SerializeObject(existingFiles);
            File.WriteAllText(jsonPath, json);
        }

        foreach (var file in Directory.GetFiles(OutputDirectory, "*.earc", SearchOption.AllDirectories)
                     .Select(file => file.Replace(OutputDirectory, _pcSettings.GameDataDirectory))
                     .Except(existingFiles!))
        {
            var sourcePath = file.Replace(_pcSettings.GameDataDirectory, OutputDirectory);
            IOHelper.EnsureDirectoriesExistForFilePath(file);
            File.Copy(sourcePath, file, true);
        }
    }

    private static void Step9FixWeirdEarcs()
    {
        //var path = $@"{_pcSettings.GameDataDirectory}\character\sm\sm05\entry\sm05_000_carbuncle_mogchoco.earc";
        //var unpacker = new Unpacker(path);
        //var packer = unpacker.ToPacker();
        //packer.AddReference("data://character/sm/sm05/entry/sm05_000_carbuncle_platinum.ebex@", true);
        //packer.WriteToFile(path);

        var context = Ps4Utilities.NewContext();
        var data = Ps4Utilities.GetFileByUri(context, "data://celltool_dlc_mog.mid");
        var path = $@"{_pcSettings.GameDataDirectory}\autoexternal.earc";
        var unpacker = new Unpacker(path);
        var packer = unpacker.ToPacker();
        packer.AddCompressedFile("data://celltool_dlc_mog.mid", data);
        packer.WriteToFile(path);

        data = Ps4Utilities.GetFileByUri(context, "data://navimesh/level/dlc_ex/mog/worldshare/worldshare_mog/data.nav_world");
        path = $@"{_pcSettings.GameDataDirectory}\level\dlc_ex\mog\worldshare\worldshare_mog.earc";
        unpacker = new Unpacker(path);
        packer = unpacker.ToPacker();
        packer.AddCompressedFile("data://navimesh/level/dlc_ex/mog/worldshare/worldshare_mog/data.nav_world", data);
        packer.WriteToFile(path);
    }

    private static void ResetFiles()
    {
        var ebexEarcsJson = File.ReadAllText(@"C:\Modding\Chocomog\Testing\modified_ebex_earcs.json");
        var assetEarcsJson = File.ReadAllText(@"C:\Modding\Chocomog\Testing\modified_asset_earcs.json");
        var ebexEarcs = JsonConvert.DeserializeObject<List<string>>(ebexEarcsJson)!;
        var assetEarcs = JsonConvert.DeserializeObject<List<string>>(assetEarcsJson)!;
        var earcs = ebexEarcs.Union(assetEarcs).ToList();
        earcs.Add($@"{_pcSettings.GameDataDirectory}\autoexternal.earc");
        foreach (var earc in earcs)
        {
            if (File.Exists(earc))
            {
                File.Delete(earc);
            }

            var backupPath = earc.Replace("\\FINAL FANTASY XV\\", "\\");
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, earc);
            }
        }
    }

    private static IEnumerable<(string Uri, byte[] Data)> GetConvertedAssets(string uri)
    {
        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        var data = Ps4Utilities.GetFileByUri(context, uri);
        if (data.Length == 0)
        {
            System.Console.WriteLine($"[E] {uri} had no data!");
        }
        else if (data[0] == 100 && data[1] == 101)
        {
            System.Console.WriteLine($"[E] {uri} was deleted!");
        }
        else
        {
            if (uri.EndsWith(".tif") || uri.EndsWith(".dds") || uri.EndsWith(".png") || uri.EndsWith(".btex"))
            {
                var btex = Btex.FromData(data);
                using var stream = new MemoryStream();
                btex.Bitmap.Save(stream, ImageFormat.Png);
                var converter = new TextureConverter();
                yield return (uri, converter.PngToBtex(btex, stream.ToArray()));

                var highTextureUri = uri.Insert(uri.LastIndexOf('.'), "_$h");
                var highTextureData = Ps4Utilities.GetFileByUri(context, highTextureUri);

                if (highTextureData.Length > 0)
                {
                    var highBtex = Btex.FromData(highTextureData);
                    using var highStream = new MemoryStream();
                    highBtex.Bitmap.Save(highStream, ImageFormat.Png);
                    var highConverter = new TextureConverter();
                    yield return (highTextureUri, highConverter.PngToBtex(highBtex, highStream.ToArray()));
                }
            }

            if (uri.EndsWith(".gmtl"))
            {
                var originalMaterial = new MaterialReader(data).Read();
                var material = new MaterialReader(_materialData).Read();

                material.HighTexturePackAsset = originalMaterial.HighTexturePackAsset;
                material.Name = originalMaterial.Name;
                material.NameHash = originalMaterial.NameHash;
                var originalUriDependency =
                    originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
                var uriDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
                uriDependency.Path = originalUriDependency.Path;
                var originalRefDependency =
                    originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
                var refDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
                refDependency.Path = originalRefDependency.Path;

                foreach (var subdependency in material.Header.Dependencies.Where(d =>
                             d.PathHash is not "asset_uri" and not "ref" && !d.Path.Contains("/shader/")))
                {
                    var textureSlot = material.Textures.FirstOrDefault(t => t.Path == subdependency.Path)!
                        .ShaderGenName;
                    var originalUri = originalMaterial.Textures.FirstOrDefault(t => t.ShaderGenName == textureSlot)
                        ?.Path;

                    if (originalUri == null)
                    {
                        continue;
                    }

                    var originalMatch =
                        originalMaterial.Header.Dependencies.FirstOrDefault(d => d.Path == originalUri)!;

                    if (originalMatch == null)
                    {
                        throw new Exception("That ain't gonna work!");
                    }

                    var textureUri = originalMatch.Path;
                    var textureHash = ulong.Parse(originalMatch.PathHash);

                    var matches = material.Textures.Where(t => t.Path == subdependency.Path);
                    foreach (var match in matches)
                    {
                        match.Path = textureUri;
                        match.ResourceFileHash = textureHash;
                        match.PathHash = Cryptography.Hash32(textureUri);
                    }

                    subdependency.Path = textureUri;
                    subdependency.PathHash = textureHash.ToString();
                }

                material.Header.Hashes = material.Header.Dependencies
                    .Where(d => ulong.TryParse(d.PathHash, out _))
                    .Select(d => ulong.Parse(d.PathHash))
                    .OrderBy(h => h)
                    .ToList();

                yield return (uri, new MaterialWriter(material).Write());
            }

            if (uri.EndsWith(".amdl"))
            {
                System.Console.WriteLine($"[I] Porting {uri}");
                data = AnimationModel.ToPC(data);
            }

            // if (uri.EndsWith(".ani"))
            // {
            //     var clip = AnimationClip.FromData(data);
            //     data = AnimationClip.ToData(clip);
            // }
            //
            // if (uri.EndsWith(".pka"))
            // {
            //     var pack = AnimationPackage.FromData(data);
            //     foreach (var item in pack.Items)
            //     {
            //         var clip = AnimationClip.FromData(item.Ani);
            //         item.Ani = AnimationClip.ToData(clip);
            //     }
            //
            //     data = AnimationPackage.ToData(pack);
            // }

            yield return (uri, data);
        }
    }

    private static void CreateEarcRecursively(FestivalDependency ebex)
    {
        ebex.Uri = ebex.Uri.ToLower();

        using var context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        var pcContext = new FlagrumDbContext(_pcSettings);

        var isDuplicate = pcContext.AssetUris.Any(a => EF.Functions.Like(a.Uri, ebex.Uri));
        if (isDuplicate)
        {
            return;
        }

        var relativePath = IOHelper.UriToRelativePath(ebex.Uri).Replace(".ebex", ".earc").Replace(".prefab", ".earc");
        var outputPath = $@"{OutputDirectory}\{relativePath}";
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
        }
        else
        {
            packer = new Packer();
            packer.Header.Flags = 0;
            var exml = Ps4Utilities.GetFileByUri(context, ebex.Uri);
            packer.AddCompressedFile(ebex.Uri, exml, true);
        }

        var locker = new object();
        Parallel.ForEach(context.FestivalDependencies.Where(d => d.ParentId == ebex.Id), child =>
        {
            lock (locker)
            {
                if (!packer.HasFile(child.Uri + "@"))
                {
                    packer.AddReference(child.Uri + "@", true);
                }
            }

            CreateEarcRecursively(child);
        });

        // if (ebex.Uri.EndsWith("_mog.ebex"))
        // {
        //     packer.AddReference(ebex.Uri.Replace("_mog.ebex", ".ebex@"), true);
        // }

        var referenceDependencies = new Dictionary<string, bool>();
        foreach (var subdependency in context.FestivalSubdependencies.Where(d => d.ParentId == ebex.Id))
        {
            _assets.TryAdd(subdependency.Uri.ToLower(), true);

            var referenceUri = GetExistingReferenceUri(pcContext, subdependency.Uri);
            var reference = referenceUri ?? subdependency.Uri[..subdependency.Uri.LastIndexOf('/')].ToLower() +
                "/autoexternal.ebex@";
            referenceDependencies.TryAdd(reference, true);
            foreach (var modelDependency in
                     context.FestivalModelDependencies.Where(d => d.ParentId == subdependency.Id))
            {
                _assets.TryAdd(modelDependency.Uri.ToLower(), true);
                var modelReferenceUri = GetExistingReferenceUri(pcContext, modelDependency.Uri);
                var modelReference = modelReferenceUri ??
                                     modelDependency.Uri[..modelDependency.Uri.LastIndexOf('/')].ToLower() +
                                     "/autoexternal.ebex@";
                referenceDependencies.TryAdd(modelReference, true);
                foreach (var materialDependency in context.FestivalMaterialDependencies.Where(d =>
                             d.ParentId == modelDependency.Id))
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
            if (!packer.HasFile(uri))
            {
                packer.AddReference(uri, !uri.EndsWith(".htpk"));
            }
        }

        // TODO: Create and add autoext file

        IOHelper.EnsureDirectoriesExistForFilePath(outputPath);
        packer.WriteToFile(outputPath);
        _modifiedEarcs.Add(outputPath);
    }

    private static string GetExistingReferenceUri(FlagrumDbContext pcContext, string uri)
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

    private static FestivalDependency TraverseEbexTree(string uri, byte[] xmb2, FestivalDependency parent)
    {
        uri = uri.ToLower();
        if (_checked.ContainsKey(uri))
        {
            return null;
        }

        _checked.TryAdd(uri, true);

        if (xmb2[0] == 'd')
        {
            System.Console.WriteLine($"[W] {uri} was deleted!");
            return null;
        }

        var current = new FestivalDependency {Uri = uri};
        parent?.Children.Add(current);

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
                string combinedUriString;
                if (relativeUri.StartsWith('.'))
                {
                    var uriUri = new Uri(uri.Replace("data://", "data://data/"));
                    var combinedUri = new Uri(uriUri, relativeUri);
                    combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                }
                else
                {
                    combinedUriString = $"data://{relativeUri}";
                }

                byte[] innerXmb2;
                using (var context = new FlagrumDbContext(_settings, Ps4DatabasePath))
                {
                    innerXmb2 = Ps4Utilities.GetFileByUri(context, combinedUriString);
                }

                if (innerXmb2.Length > 0)
                {
                    _ = TraverseEbexTree(combinedUriString, innerXmb2, current);
                }
            }

            if (type == typeof(ActorEntity))
            {
                var path = element.GetElementByName("charaEntry_");

                if (path != null)
                {
                    var relativeUri = path.GetTextValue();
                    string combinedUriString;
                    if (relativeUri.StartsWith('.'))
                    {
                        var uriUri = new Uri(uri.Replace("data://", "data://data/"));
                        var combinedUri = new Uri(uriUri, relativeUri);
                        combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                    }
                    else
                    {
                        combinedUriString = $"data://{relativeUri}";
                    }

                    byte[] innerXmb2;
                    using (var context = new FlagrumDbContext(_settings, Ps4DatabasePath))
                    {
                        innerXmb2 = Ps4Utilities.GetFileByUri(context, combinedUriString);
                    }

                    if (innerXmb2.Length > 0)
                    {
                        _ = TraverseEbexTree(combinedUriString, innerXmb2, current);
                    }
                }
            }
        });

        GC.Collect();
        return current;
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
        var gmdlData = Ps4Utilities.GetFileByUri(ps4Context, gmdl);
        var gpubinData = Ps4Utilities.GetFileByUri(ps4Context, gpubin);
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
                envPacker.AddReference(referenceUri, true);
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
        var gmdlData = Ps4Utilities.GetFileByUri(ps4Context, gmdl);
        var modelHeader = new GfxbinHeader();
        modelHeader.Read(new BinaryReader(gmdlData));

        var materialData =
            pcContext.GetFileByUri(
                "data://environment/leide/props/le_ar_gqshop1/materials/le_ar_gqshop1_curtain_cloth.gmtl");

        foreach (var dependency in modelHeader.Dependencies.Where(d => d.Path.EndsWith(".gmtl")))
        {
            var originalMaterialData = Ps4Utilities.GetFileByUri(ps4Context, dependency.Path);
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
                var subdata = Ps4Utilities.GetFileByUri(ps4Context, textureUri);
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
        var gpubinData = Ps4Utilities.GetFileByUri(ps4Context, gpubin);
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
                envPacker.AddReference(referenceUri, true);
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
        var gmdlData = Ps4Utilities.GetFileByUri(ps4Context, gmdl);
        var modelHeader = new GfxbinHeader();
        modelHeader.Read(new BinaryReader(gmdlData));

        var materialData =
            pcContext.GetFileByUri(
                "data://environment/leide/props/le_ar_gqshop1/materials/le_ar_gqshop1_curtain_cloth.gmtl");

        foreach (var dependency in modelHeader.Dependencies.Where(d => d.Path.EndsWith(".gmtl")))
        {
            var originalMaterialData = Ps4Utilities.GetFileByUri(ps4Context, dependency.Path);
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
                var subdata = Ps4Utilities.GetFileByUri(ps4Context, textureUri);
                var btex = Btex.FromData(subdata);
                using var stream = new MemoryStream();
                btex.Bitmap.Save(stream, ImageFormat.Png);
                var converter = new TextureConverter();
                var textureData = converter.PngToBtex(btex, stream.ToArray());

                var highTextureUri = textureUri.Insert(textureUri.LastIndexOf('.'), "_$h");
                var highTextureData = Ps4Utilities.GetFileByUri(ps4Context, highTextureUri);
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
        var gpubinData = Ps4Utilities.GetFileByUri(ps4Context, gpubin);

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
            packer.AddReference(referenceUri, true);
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
                envPacker.AddReference(referenceUri, true);
            }
        }

        System.Console.WriteLine($"Writing EARC {envPath}");
        envPacker.WriteToFile(envPath);
    }
}