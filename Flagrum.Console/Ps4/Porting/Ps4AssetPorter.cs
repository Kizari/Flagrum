using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Core.Animation;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Ps4;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4AssetPorter
{
    private readonly ConcurrentBag<string> _modifiedEarcs2 = new();
    private readonly SettingsService _pcSettings = new();

    private readonly SettingsService _releaseSettings = new() {GamePath = Ps4PorterConfiguration.ReleaseGamePath};

    //private readonly ConcurrentDictionary<VertexLayoutType, byte[]> _materialTemplates = new();
    //private readonly ConcurrentDictionary<string, VertexLayoutType> _materialMap = new();
    private ConcurrentDictionary<string, string> _materialMap;
    private byte[] _materialTemplate;
    private readonly object _packerLock = new();

    private List<string> animationGraphs = new List<string>()
    {
        // These work
        "data://character/uw/common/anim/graph/uw_facial_mog_ref.anmgph",
        "data://character/uw/common/anim/graph/uw_facial_base_mog.anmgph",
        "data://character/um/common/anim/graph/um_04_401_facial_mog.anmgph",
        "data://character/ux/common/anim/graph/ux_facial_mog_ref.anmgph",
        "data://character/uw/common/anim/graph/uw_011_facial_mog.anmgph",
        "data://character/um/common/anim/graph/um_05_101_facial_mog.anmgph",
        "data://character/uc/common/anim/graph/uc_010_facial_mog.anmgph",
        "data://character/um/common/anim/graph/um_100_facial_mog.anmgph",
        "data://character/ux/common/anim/graph/ux_05_110_facial_mog.anmgph",
        "data://character/ux/common/anim/graph/ux_010_facial_mog.anmgph",
        "data://character/uy/common/anim/graph/uy_facial_mog.anmgph",
        "data://character/uw/common/anim/graph/uw_facial_mog.anmgph",
        
        // Does not work
        //"data://character/um/common/anim/graph/um_020_facial_mog.anmgph",
        //"data://character/uw/common/anim/graph/uw_020_facial_mog.anmgph",
        //"data://character/uw/common/anim/graph/uw_010_facial_mog.anmgph",
        //"data://character/uy/common/anim/graph/uy_facial_mog_ref.anmgph"
        
    };
    
    public void RunSingleEarc()
    {
        using var context = Ps4Utilities.NewContext();
        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map.json");
        _materialMap = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json)!;

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map2.json");
        var materialMap2 = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json)!;
        foreach (var (uri1, uri2) in materialMap2)
        {
            _materialMap.TryAdd(uri1, uri2);
        }

        var usAnis = assets
            .Where(a => a.EndsWith(".ani") && a.Contains("/jp/"))
            .Select(a => a.Replace("/jp/", "/us/"))
            .ToList();

        assets.AddRange(usAnis);

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

        var unpacker =
            new Unpacker(
                @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
        var packer = unpacker.ToPacker();

        Parallel.ForEach(earcs, kvp =>
        {
            var (folder, uris) = kvp;
            var earcPath =
                $@"{Ps4PorterConfiguration.OutputDirectory}\{IOHelper.UriToRelativePath(folder)}\autoexternal.earc";

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
                            $@"{Ps4PorterConfiguration.OutputDirectory}\{IOHelper.UriToRelativePath(uri).Replace(".htpk", ".earc")}";
                        IOHelper.EnsureDirectoriesExistForFilePath(htpkPath);
                        htpkPacker.WriteToFile(htpkPath);
                        _modifiedEarcs2.Add(htpkPath);
                    }
                }
                else
                {
                    foreach (var (finalUri, data, isReference) in GetConvertedAssets(uri))
                    {
                        lock (_packerLock)
                        {
                            if (!packer.HasFile(finalUri))
                            {
                                if (isReference)
                                {
                                    if (data == null)
                                    {
                                        packer.AddReference(finalUri, true);
                                    }
                                    else
                                    {
                                        var directPath = uri.Replace("data://", "").Replace('/', '\\');
                                        if (!uri.EndsWith(".bk2"))
                                        {
                                            directPath = directPath.Insert(directPath.LastIndexOf('.'), ".win");
                                            directPath = directPath[..^1] + 'b'; // Change sax/max to sab/mab
                                        }

                                        directPath = Ps4PorterConfiguration.OutputDirectory + '\\' + directPath;
                                        IOHelper.EnsureDirectoriesExistForFilePath(directPath);
                                        File.WriteAllBytes(directPath, data);
                                    }
                                }
                                else
                                {
                                    packer.AddCompressedFile(finalUri, data, true);
                                }
                            }
                        }
                    }
                }
            }
        });
        
        packer.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
    }
    
    public void Run()
    {
        using var context = Ps4Utilities.NewContext();
        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map.json");
        _materialMap = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json)!;

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map2.json");
        var materialMap2 = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json)!;
        foreach (var (uri1, uri2) in materialMap2)
        {
            _materialMap[uri1] = uri2;
        }
        
        var unpacker =
            new Unpacker(
                @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
        var packer = unpacker.ToPacker();

        foreach (var asset in assets.Where(a => a.EndsWith(".anmgph")))
        {
            if (!packer.HasFile(asset))
            {
                var data = Ps4Utilities.GetFileByUri(context, asset);
                packer.AddCompressedFile(asset, data, true);
            }
        }
        
        packer.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");

        //var allAnis = JsonConvert.DeserializeObject<List<string>>(json)!.Where(a => a.EndsWith(".ani")).ToList();
        assets = assets.Where(a => !a.EndsWith(".anmgph"))// || animationGraphs.Contains(a)) // && (!a.EndsWith(".ani") || animations.Contains(a)))
            .ToList();

        var usAnis = assets
            .Where(a => a.EndsWith(".ani") && a.Contains("/jp/"))
            .Select(a => a.Replace("/jp/", "/us/"))
            .ToList();

        assets.AddRange(usAnis);

        // var armatures = assets.Where(a => a.EndsWith(".amdl")).Select(a => a[..a.LastIndexOf('/')]).ToList();
        // foreach (var armature in armatures)
        // {
        //     var anis = allAnis.Where(a => a.StartsWith(armature)).ToList();
        //     assets.AddRange(anis);
        // }
        //
        // var result = assets.Where(a => a.EndsWith(".ani")).ToList();
        //
        // using var pcContext = new FlagrumDbContext(_pcSettings);
        // _materialTemplates.TryAdd(VertexLayoutType.NULL,
        //     pcContext.GetFileByUri(
        //         "data://environment/insomnia/props/in_ar_build05/materials/in_ar_build05_typa_fence_alpha_mat.gmtl"));
        // _materialTemplates.TryAdd(VertexLayoutType.Skinning_1Bones,
        //     pcContext.GetFileByUri(
        //         "data://character/uw/uw00/model_100/materials/uw00_100_uheye_00_mat.gmtl"));
        // _materialTemplates.TryAdd(VertexLayoutType.Skinning_4Bones,
        //     pcContext.GetFileByUri(
        //         "data://character/uw/uw00/model_100/materials/uw00_100_uhcloth_00_mat.gmtl"));
        // _materialTemplates.TryAdd(VertexLayoutType.Skinning_6Bones,
        //     pcContext.GetFileByUri(
        //         "data://character/nh/nh02/model_010/materials/nh02_010_cloth_00_mat.gmtl"));
        // _materialTemplates.TryAdd(VertexLayoutType.Skinning_8Bones,
        //     pcContext.GetFileByUri(
        //         "data://character/nh/nh02/model_000/materials/nh02_000_skin_00_mat.gmtl"));
        //
        // _materialTemplate = File.ReadAllBytes(@"C:\Modding\Chocomog\Testing\ex_pr_illumi_mt.gmtl.gfxbin");

        //_materialData = pcContext.GetFileByUri(
        //    "data://environment/leide/props/le_ar_gqshop1/materials/le_ar_gqshop1_curtain_cloth.gmtl");

        // foreach (var modelUri in assets.Where(a => a.EndsWith(".gmdl")))
        // {
        //     var gmdl = Ps4Utilities.GetFileByUri(context, modelUri);
        //     var gpubin = Ps4Utilities.GetFileByUri(context, modelUri.Replace(".gmdl", ".gpubin"));
        //
        //     if (gmdl.Length == 0)
        //     {
        //         continue;
        //     }
        //     
        //     var model = new ModelReader(gmdl, gpubin).Read();
        //
        //     foreach (var meshObject in model.MeshObjects)
        //     {
        //         foreach (var mesh in meshObject.Meshes)
        //         {
        //             var materialUri = model.Header.Dependencies
        //                 .FirstOrDefault(d => d.PathHash == mesh.DefaultMaterialHash.ToString())
        //                 !.Path;
        //
        //             _materialMap.TryAdd(materialUri, mesh.VertexLayoutType);
        //         }
        //     }
        // }

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
            var earcPath =
                $@"{Ps4PorterConfiguration.OutputDirectory}\{IOHelper.UriToRelativePath(folder)}\autoexternal.earc";

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
                            $@"{Ps4PorterConfiguration.OutputDirectory}\{IOHelper.UriToRelativePath(uri).Replace(".htpk", ".earc")}";
                        IOHelper.EnsureDirectoriesExistForFilePath(htpkPath);
                        htpkPacker.WriteToFile(htpkPath);
                        _modifiedEarcs2.Add(htpkPath);
                    }
                }
                else
                {
                    foreach (var (finalUri, data, isReference) in GetConvertedAssets(uri))
                    {
                        if (!packer.HasFile(finalUri))
                        {
                            if (isReference)
                            {
                                if (data == null)
                                {
                                    packer.AddReference(finalUri, true);
                                }
                                else
                                {
                                    var directPath = uri.Replace("data://", "").Replace('/', '\\');
                                    if (!uri.EndsWith(".bk2"))
                                    {
                                        directPath = directPath.Insert(directPath.LastIndexOf('.'), ".win");
                                        directPath = directPath[..^1] + 'b'; // Change sax/max to sab/mab
                                    }

                                    directPath = Ps4PorterConfiguration.OutputDirectory + '\\' + directPath;
                                    IOHelper.EnsureDirectoriesExistForFilePath(directPath);
                                    File.WriteAllBytes(directPath, data);
                                }
                            }
                            else
                            {
                                packer.AddCompressedFile(finalUri, data);
                            }
                        }
                    }
                }
            }

            if (packer.Files.Any())
            {
                IOHelper.EnsureDirectoriesExistForFilePath(earcPath);
                packer.WriteToFile(earcPath);
                _modifiedEarcs2.Add(earcPath);
            }
        });

        var modifiedJson = JsonConvert.SerializeObject(_modifiedEarcs2);
        File.WriteAllText($@"{Ps4PorterConfiguration.StagingDirectory}\modified_asset_earcs.json", modifiedJson);
    }

    private IEnumerable<(string Uri, byte[] Data, bool IsReference)> GetConvertedAssets(string uri)
    {
        byte[] data;
        var isReference = false;
        using var context = Ps4Utilities.NewContext();

        if (uri.EndsWith(".sax") || uri.EndsWith(".max") || uri.EndsWith(".bk2"))
        {
            if (!context.Ps4AssetUris.Any(a => a.Uri == uri))
            {
                isReference = true;
            }

            if (uri.EndsWith(".bk2"))
            {
                var path = Ps4PorterConfiguration.GameDirectory + @"\CUSA01633-patch_115\CUSA01633-patch\" +
                           uri.Replace("data://", "").Replace('/', '\\');
                data = File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
            }
            else
            {
                var extension = uri.Split('.').Last().Replace('x', 'b');
                var path =
                    $@"{Ps4PorterConfiguration.StagingDirectory}\Audio\Output\{Cryptography.HashFileUri64(uri)}.orb.{extension}";
                data = File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
            }
        }
        else
        {
            data = Ps4Utilities.GetFileByUri(context, uri);
        }

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
                yield return (uri, converter.PngToBtex(btex, stream.ToArray()), isReference);

                var highTextureUri = uri.Insert(uri.LastIndexOf('.'), "_$h");
                var highTextureData = Ps4Utilities.GetFileByUri(context, highTextureUri);

                if (highTextureData.Length > 0)
                {
                    var highBtex = Btex.FromData(highTextureData);
                    using var highStream = new MemoryStream();
                    highBtex.Bitmap.Save(highStream, ImageFormat.Png);
                    var highConverter = new TextureConverter();
                    yield return (highTextureUri, highConverter.PngToBtex(highBtex, highStream.ToArray()), isReference);
                }
            }

            if (uri.EndsWith(".gmtl"))
            {
                var originalMaterial = new MaterialReader(data).Read();

                // Material material;
                // if (originalMaterial.Interfaces[0].Name == "ts_ENV_BMNROE_anim4_blink1_Material")
                // {
                //     material = new MaterialReader(_materialTemplate).Read();
                // }
                // else
                // {
                //     var vertexLayoutType = _materialMap[uri];
                //     material = new MaterialReader(GetMaterialTemplate(vertexLayoutType)).Read();
                // }
                using var pcContext = new FlagrumDbContext(_releaseSettings, Ps4Constants.DatabasePath);
                using var debugContext = new FlagrumDbContext(_pcSettings);
                var materialData = pcContext.GetFileByUri(_materialMap[uri]);
                var material = new MaterialReader(materialData).Read();
                foreach (var shader in material.Header.Dependencies
                             .Where(d => d.Path.EndsWith(".sb")))
                {
                    // if (!debugContext.AssetUris.Any(a => a.Uri == shader.Path))
                    // {
                    //     var shaderData = pcContext.GetFileByUri(shader.Path);
                    //     yield return (shader.Path, shaderData, false);
                    // }
                    // else
                    // {
                    //     var relativePath = debugContext.GetArchiveRelativeLocationByUri(shader.Path);
                    //     if (relativePath != @"shader\shadergen\autoexternal.earc")
                    //     {
                    //         relativePath = "data://" + relativePath.Replace('\\', '/').Replace(".earc", ".ebex@");
                    //         yield return (relativePath, null, true);
                    //     }
                    // }
                    var relativePath = debugContext.GetArchiveRelativeLocationByUri(shader.Path);
                    if (relativePath != @"shader\shadergen\autoexternal.earc")
                    {
                        var shaderData = pcContext.GetFileByUri(shader.Path);
                        yield return (shader.Path, shaderData, false);
                    }
                }

                material.HighTexturePackAsset = originalMaterial.HighTexturePackAsset;
                material.Name = originalMaterial.Name;
                material.NameHash = originalMaterial.NameHash;
                material.Uri = uri;

                // var originalUriDependency =
                //     originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
                // var uriDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri")!;
                // uriDependency.Path = originalUriDependency.Path;
                // var originalRefDependency =
                //     originalMaterial.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
                // var refDependency = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref")!;
                // refDependency.Path = originalRefDependency.Path;

                foreach (var input in originalMaterial.InterfaceInputs)
                {
                    var match = material.InterfaceInputs.FirstOrDefault(i => i.ShaderGenName == input.ShaderGenName);
                    if (match != null)
                    {
                        match.Values = input.Values;
                    }
                }

                // Update all the texture slots with the PS4 textures
                foreach (var texture in originalMaterial.Textures.Where(t => !t.Path.EndsWith(".sb")))
                {
                    var match = material.Textures.FirstOrDefault(t => t.ShaderGenName == texture.ShaderGenName);
                    if (match == null)
                    {
                        continue;
                    }

                    match.Path = texture.Path;
                    match.PathHash = texture.PathHash;
                    match.ResourceFileHash = texture.ResourceFileHash;
                }

                material.RegenerateDependencyTable();

                // material.Header.Hashes = material.Header.Dependencies
                //     .Where(d => ulong.TryParse(d.PathHash, out _))
                //     .Select(d => ulong.Parse(d.PathHash))
                //     .OrderBy(h => h)
                //     .ToList();

                yield return (uri, new MaterialWriter(material).Write(), isReference);
            }

            if (uri.EndsWith(".amdl"))
            {
                data = AnimationModel.ToPC(data);
            }

            // if (uri.EndsWith(".ani") && !animations.Contains(uri))
            // {
            //     //var relativePath = uri.Replace("data://", "").Replace('/', '\\');
            //     //var relativeEarcPath = relativePath[..relativePath.LastIndexOf('/')] + "\\autoexternal.earc";
            //     //var earcPath = $@"{_pcSettings.GameDataDirectory}\{relativeEarcPath}";
            //     using var pcContext = new FlagrumDbContext(_releaseSettings, Ps4Constants.DatabasePath);
            //
            //     string sample = null;
            //     var uriDirectory = uri;
            //     while (sample == null)
            //     {
            //         uriDirectory = uriDirectory[..uriDirectory.LastIndexOf('/')];
            //         sample = pcContext.AssetUris
            //             .Where(a => a.Uri.StartsWith(uriDirectory) && a.Uri.EndsWith(".ani"))
            //             .Select(a => a.Uri)
            //             .FirstOrDefault();
            //     }
            //
            //     data = pcContext.GetFileByUri(sample);
            // }
            // else if (uri.EndsWith(".ani"))
            // {
            //     data = File.ReadAllBytes(@"C:\Modding\EarcMods\TalkBasicFacialTest\talk_basic_facial_modified.ani");
            //     //data = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\r_cc_c_lp_um02_410_0200a_facial.ani");
            // }

            if (uri.EndsWith(".swf"))
            {
                var listbUri = uri.Replace(".swf", ".listb");
                var listb = Ps4Utilities.GetFileByUri(context, listbUri);
                if (listb.Length > 0)
                {
                    yield return (listbUri, listb, false);
                }

                var folder = uri[..uri.LastIndexOf('/')];
                var folderName = folder[(folder.LastIndexOf('/') + 1)..];
                var swfUri = $"{folder}/{folderName}.btex";
                var swf = Ps4Utilities.GetFileByUri(context, swfUri);
                if (swf.Length > 0)
                {
                    var btex = Btex.FromData(swf);
                    using var stream = new MemoryStream();
                    btex.Bitmap.Save(stream, ImageFormat.Png);
                    var converter = new TextureConverter();
                    yield return (swfUri, converter.PngToBtex(btex, stream.ToArray()), isReference);
                }
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

            yield return (uri, data, isReference);
        }
    }

    // private byte[] GetMaterialTemplate(VertexLayoutType type)
    // {
    //     if (type.HasFlag(VertexLayoutType.Skinning_8Bones))
    //     {
    //         return _materialTemplates[VertexLayoutType.Skinning_8Bones];
    //     }
    //     
    //     if (type.HasFlag(VertexLayoutType.Skinning_6Bones))
    //     {
    //         return _materialTemplates[VertexLayoutType.Skinning_6Bones];
    //     }
    //     
    //     if (type.HasFlag(VertexLayoutType.Skinning_4Bones))
    //     {
    //         return _materialTemplates[VertexLayoutType.Skinning_4Bones];
    //     }
    //     
    //     if (type.HasFlag(VertexLayoutType.Skinning_1Bones))
    //     {
    //         return _materialTemplates[VertexLayoutType.Skinning_1Bones];
    //     }
    //
    //     return _materialTemplates[VertexLayoutType.NULL];
    // }
}