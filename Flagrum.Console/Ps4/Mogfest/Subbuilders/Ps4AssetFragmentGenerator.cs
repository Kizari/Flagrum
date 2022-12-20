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
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4AssetFragmentGenerator
{
    private const string StagingDirectory = @"C:\Modding\Chocomog\Staging";

    private readonly SettingsService _pcSettings = new();
    private readonly SettingsService _releaseSettings = new() {GamePath = Ps4PorterConfiguration.ReleaseGamePath};
    private readonly object _shaderLock = new();
    private ConcurrentDictionary<string, string> _materialMap;
    private ConcurrentDictionary<string, ArchiveFile> _shaderMap = new();
    private ConcurrentDictionary<string, bool> _shadersToIgnore = new();

    public void Run()
    {
        using var pcContext = new FlagrumDbContext(_pcSettings);
        _shadersToIgnore = new ConcurrentDictionary<string, bool>(pcContext.AssetUris
            .Where(a => a.ArchiveLocation.Path == @"shader\shadergen\autoexternal.earc")
            .ToDictionary(a => a.Uri, a => true));
        
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

        Parallel.ForEach(earcs, kvp =>
        {
            var (folder, uris) = kvp;
            foreach (var uri in uris)
            {
                if (uri.EndsWith(".htpk"))
                {
                    // var htpkFolder = uri[..uri.LastIndexOf('/')];
                    // var sourceimagesFolder = htpkFolder[..htpkFolder.LastIndexOf('/')] + "/sourceimages";
                    //
                    // if (earcs.ContainsKey(sourceimagesFolder))
                    // {
                    //     var htpk = Encoding.UTF8.GetBytes(string.Join(' ',
                    //         earcs[sourceimagesFolder].Select(u => u.Insert(u.LastIndexOf('.'), "_$h"))) + (char)0x00);
                    //
                    //     var fragment = new FmodFragment
                    //     {
                    //         OriginalSize = (uint)htpk.Length,
                    //         ProcessedSize = (uint)htpk.Length,
                    //         Flags = ArchiveFileFlag.Autoload,
                    //         Key = 0,
                    //         RelativePath = uri.Replace("data://", ""),
                    //         Data = htpk
                    //     };
                    //
                    //     var hash = Cryptography.HashFileUri64(uri.Replace(".htpk", ".autoext"));
                    //     fragment.Write($@"{StagingDirectory}\Fragments\{hash}.ffg");
                    // }
                }
                else
                {
                    foreach (var (finalUri, data) in GetConvertedAssets(uri))
                    {
                        var hash = Cryptography.HashFileUri64(finalUri);
                        var path = $@"{StagingDirectory}\Fragments\{hash}.ffg";

                        if (finalUri.EndsWith(".sb"))
                        {
                            lock (_shaderLock)
                            {
                                if (!File.Exists(path))
                                {
                                    data.Write(path);
                                }
                            }
                        }
                        else
                        {
                            data.Write(path);
                        }
                    }
                }
            }
        });
    }

    private IEnumerable<(string Uri, FmodFragment Data)> GetConvertedAssets(string uri)
    {
        FmodFragment fragment = null;
        var isReference = false;
        using var context = Ps4Utilities.NewContext();
        ArchiveFile file = null;

        if (!(uri.EndsWith(".sax") || uri.EndsWith(".max") || uri.EndsWith(".bk2")))
        {
            file = Ps4Utilities.GetArchiveFileByUri(context, uri);
        }

        if (file == null)
        {
            System.Console.WriteLine($"[E] {uri} had no data!");
        }
        else
        {
            if (uri.EndsWith(".tif") || uri.EndsWith(".dds") || uri.EndsWith(".png") || uri.EndsWith(".btex") ||
                uri.EndsWith(".exr"))
            {
                var btex = Btex.FromData(file.GetReadableData());
                using var stream = new MemoryStream();
                btex.Bitmap.Save(stream, ImageFormat.Png);
                var converter = new TextureConverter();
                var lowData = converter.PngToBtex(btex, stream.ToArray());
                file.SetRawData(lowData);
                var lowDataProcessed = file.GetDataForExport();
                fragment = new FmodFragment
                {
                    OriginalSize = (uint)lowData.Length,
                    ProcessedSize = (uint)lowDataProcessed.Length,
                    Flags = ArchiveFileFlag.None,
                    Key = file.Key,
                    RelativePath = file.RelativePath,
                    Data = lowDataProcessed
                };

                var highTextureUri = uri.Insert(uri.LastIndexOf('.'), "_$h");
                var highFile = Ps4Utilities.GetArchiveFileByUri(context, highTextureUri);

                if (highFile != null)
                {
                    var highBtex = Btex.FromData(highFile.GetReadableData());
                    using var highStream = new MemoryStream();
                    highBtex.Bitmap.Save(highStream, ImageFormat.Png);
                    var highConverter = new TextureConverter();
                    var highData = highConverter.PngToBtex(highBtex, highStream.ToArray());
                    highFile.SetRawData(highData);
                    var highDataProcessed = highFile.GetDataForExport();
                    var highFragment = new FmodFragment
                    {
                        OriginalSize = (uint)highData.Length,
                        ProcessedSize = (uint)highDataProcessed.Length,
                        Flags = ArchiveFileFlag.None,
                        Key = highFile.Key,
                        RelativePath = highFile.RelativePath,
                        Data = highDataProcessed
                    };

                    yield return (highTextureUri, highFragment);
                }
            }

            if (uri.EndsWith(".gmtl"))
            {
                var originalMaterial = new MaterialReader(file.GetReadableData()).Read();

                using var pcContext = new FlagrumDbContext(_releaseSettings, Ps4Constants.DatabasePath);
                var materialData = pcContext.GetFileByUri(_materialMap[uri]);
                var material = new MaterialReader(materialData).Read();

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
                    
                    yield return (shader.Path, shaderFragment);
                }

                material.HighTexturePackAsset = originalMaterial.HighTexturePackAsset;
                material.Name = originalMaterial.Name;
                material.NameHash = originalMaterial.NameHash;
                material.Uri = uri;

                foreach (var input in originalMaterial.InterfaceInputs)
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
                    var match = originalMaterial.Textures.FirstOrDefault(t => t.ShaderGenName == texture.ShaderGenName);
                    if (match == null)
                    {
                        texture.Path = "data://shader/defaulttextures/black.tif";
                        texture.PathHash = Cryptography.Hash32("data://shader/defaulttextures/black.tif");
                        texture.ResourceFileHash =
                            Cryptography.HashFileUri64("data://shader/defaulttextures/black.tif");
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
                file.SetRawData(materialResult);
                var processedMaterial = file.GetDataForExport();
                fragment = new FmodFragment
                {
                    OriginalSize = (uint)materialResult.Length,
                    ProcessedSize = (uint)processedMaterial.Length,
                    Flags = ArchiveFileFlag.None,
                    Key = file.Key,
                    RelativePath = file.RelativePath,
                    Data = processedMaterial
                };
            }

            if (uri.EndsWith(".amdl"))
            {
                var amdlData = AnimationModel.ToPC(file.GetReadableData());
                file.SetRawData(amdlData);
                var processedAmdl = file.GetDataForExport();
                fragment = new FmodFragment
                {
                    OriginalSize = (uint)amdlData.Length,
                    ProcessedSize = (uint)processedAmdl.Length,
                    Flags = ArchiveFileFlag.None,
                    Key = file.Key,
                    RelativePath = file.RelativePath,
                    Data = processedAmdl
                };
            }

            if (uri.EndsWith(".swf"))
            {
                var listbUri = uri.Replace(".swf", ".listb");
                var listb = Ps4Utilities.GetArchiveFileByUri(context, listbUri);
                if (listb != null)
                {
                    var listbFragment = new FmodFragment
                    {
                        OriginalSize = listb.Size,
                        ProcessedSize = listb.ProcessedSize,
                        Flags = ArchiveFileFlag.None,
                        Key = listb.Key,
                        RelativePath = listb.RelativePath,
                        Data = listb.GetRawData()
                    };

                    yield return (listbUri, listbFragment);
                }

                var folder = uri[..uri.LastIndexOf('/')];
                var folderName = folder[(folder.LastIndexOf('/') + 1)..];
                var swfUri = $"{folder}/{folderName}.btex";
                var swf = Ps4Utilities.GetArchiveFileByUri(context, swfUri);
                if (swf != null)
                {
                    var btex = Btex.FromData(swf.GetReadableData());
                    using var stream = new MemoryStream();
                    btex.Bitmap.Save(stream, ImageFormat.Png);
                    var converter = new TextureConverter();
                    var swfData = converter.PngToBtex(btex, stream.ToArray());
                    swf.SetRawData(swfData);
                    var processedSwf = swf.GetDataForExport();
                    var swfFragment = new FmodFragment
                    {
                        OriginalSize = (uint)swfData.Length,
                        ProcessedSize = (uint)processedSwf.Length,
                        Flags = ArchiveFileFlag.None,
                        Key = swf.Key,
                        RelativePath = swf.RelativePath,
                        Data = processedSwf
                    };

                    yield return (swfUri, swfFragment);
                }
            }

            fragment ??= new FmodFragment
            {
                OriginalSize = file.Size,
                ProcessedSize = file.ProcessedSize,
                Flags = ArchiveFileFlag.None,
                Key = file.Key,
                RelativePath = file.RelativePath,
                Data = file.GetRawData()
            };

            yield return (uri, fragment);
        }
    }
}