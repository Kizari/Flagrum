using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

public class Ps4AssetAggregator
{
    private const string StagingDirectory = @"C:\Modding\Chocomog\Staging";
    private const string OutputDirectory = @"C:\Modding\Chocomog\Staging\PortedAssets";

    private List<ulong> _existingFiles;
    private readonly SettingsService _releaseSettings = new() {GamePath = Ps4PorterConfiguration.ReleaseGamePath};
    private ConcurrentDictionary<string, string> _materialMap;
    private byte[] _magenta;

    public void Run()
    {
        _magenta = File.ReadAllBytes($@"{StagingDirectory}\magenta.png");
        
        _existingFiles = Directory.EnumerateFiles(OutputDirectory)
            .Select(f => ulong.Parse(f.Split('\\').Last().Split('_').First()))
            .ToList();
        
        var json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map.json");
        _materialMap = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json)!;

        json = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\material_map2.json");
        var materialMap2 = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json)!;
        foreach (var (uri1, uri2) in materialMap2)
        {
            _materialMap.TryAdd(uri1, uri2);
        }

        using var context = Ps4Utilities.NewContext();

        Parallel.ForEach(context.FestivalDependencies, dependency => DumpFile(dependency.Uri, null));

        Parallel.ForEach(context.FestivalSubdependencies, dependency =>
        {
            using var innerContext = Ps4Utilities.NewContext();

            if (dependency.Uri.EndsWith(".swf"))
            {
                var listbUri = dependency.Uri.Replace(".swf", ".listb");
                if (innerContext.Ps4AssetUris.Any(a => a.Uri == listbUri))
                {
                    DumpFile(listbUri, null);
                }

                var folder = dependency.Uri[..dependency.Uri.LastIndexOf('/')];
                var folderName = folder[(folder.LastIndexOf('/') + 1)..];
                var swfUri = $"{folder}/{folderName}.btex";
                if (innerContext.Ps4AssetUris.Any(a => a.Uri == swfUri))
                {
                    DumpFile(swfUri, GetDataOverride(swfUri));
                }
            }

            DumpFile(dependency.Uri, GetDataOverride(dependency.Uri));
        });

        Parallel.ForEach(context.FestivalModelDependencies, dependency => DumpFile(dependency.Uri, null));

        Parallel.ForEach(context.FestivalMaterialDependencies, dependency =>
        {
            using var innerContext = Ps4Utilities.NewContext();

            if (dependency.Uri.EndsWith(".tif") || dependency.Uri.EndsWith(".dds") || dependency.Uri.EndsWith(".png") ||
                dependency.Uri.EndsWith(".btex"))
            {
                var highTextureUri = dependency.Uri.Insert(dependency.Uri.LastIndexOf('.'), "_$h");
                if (innerContext.Ps4AssetUris.Any(a => a.Uri == highTextureUri))
                {
                    DumpFile(highTextureUri, GetDataOverride(highTextureUri));
                }
            }

            DumpFile(dependency.Uri, GetDataOverride(dependency.Uri));
        });
    }

    private Func<FlagrumDbContext, string, (byte[] data, ArchiveFile file)> GetDataOverride(string fileUri)
    {
        if (fileUri.EndsWith(".amdl"))
        {
            return (context, uri) =>
            {
                var data = Ps4Utilities.GetFileByUri(context, uri, out var file);
                
                try
                {
                    data = AnimationModel.ToPC(data);
                    file.SetRawData(data);
                    return (file.GetDataForExport(), file);
                }
                catch
                {
                    System.Console.WriteLine($"Failed to convert {uri}");
                    return (data, file);
                }
            };
        }

        if (fileUri.EndsWith(".tif") || fileUri.EndsWith(".dds") || fileUri.EndsWith(".png") ||
            fileUri.EndsWith(".btex"))
        {
            return (context, uri) =>
            {
                var data = Ps4Utilities.GetFileByUri(context, uri, out var file);

                if (file == null)
                {
                    return (null, null);
                }
                
                var converter = new TextureConverter();

                try
                {
                    var btex = Btex.FromData(data);
                    using var stream = new MemoryStream();
                    btex.Bitmap.Save(stream, ImageFormat.Png);
                    data = converter.PngToBtex(btex, stream.ToArray());
                }
                catch
                {
                    var btex = Btex.FromDataHeaderOnly(data);

                    if (btex.ImageData.Format == 0)
                    {
                        btex.ImageData.Format = BtexFormat.B8G8R8A8_UNORM;
                    }
                    
                    data = converter.PngToBtex(btex, _magenta);
                }
                
                file.SetRawData(data);
                return (file.GetDataForExport(), file);
            };
        }

        if (fileUri.EndsWith(".gmtl"))
        {
            return (context, uri) =>
            {
                if (!_materialMap.ContainsKey(uri))
                {
                    return (null, null);
                }
                
                var data = Ps4Utilities.GetFileByUri(context, uri, out var file);

                if (file == null)
                {
                    return (null, null);
                }
                
                var originalMaterial = new MaterialReader(data).Read();

                using var pcContext = new FlagrumDbContext(_releaseSettings, Ps4Constants.DatabasePath);
                var materialData = pcContext.GetFileByUri(_materialMap[uri]);
                var material = new MaterialReader(materialData).Read();
                // foreach (var shader in material.Header.Dependencies
                //              .Where(d => d.Path.EndsWith(".sb")))
                // {
                //     var relativePath = debugContext.GetArchiveRelativeLocationByUri(shader.Path);
                //     if (relativePath != @"shader\shadergen\autoexternal.earc")
                //     {
                //         var shaderData = pcContext.GetFileByUri(shader.Path);
                //         yield return (shader.Path, shaderData, false);
                //     }
                // }

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
                data = new MaterialWriter(material).Write();
                file.SetRawData(data);
                return (file.GetDataForExport(), file);
            };
        }

        return null;
    }

    private void DumpFile(string uri, Func<FlagrumDbContext, string, (byte[], ArchiveFile)> getDataOverride)
    {
        try
        {
            var uriHash = Cryptography.HashFileUri64(uri);

            if (!_existingFiles.Contains(uriHash))
            {
                using var context = Ps4Utilities.NewContext();
                byte[] data;
                ArchiveFile file;

                if (getDataOverride == null)
                {
                    data = Ps4Utilities.GetFileByUriRaw(context, uri, out file);
                }
                else
                {
                    (data, file) = getDataOverride(context, uri);
                }

                if (file == null)
                {
                    return;
                }

                var relativePathBase64 = file.RelativePath.ToBase64();
                var originalSize = file.Size;
                var flags = (int)file.Flags;
                var localisationType = file.LocalizationType;
                var locale = file.Locale;
                var key = file.Key;

                var fileName =
                    $"{uriHash}_{relativePathBase64}_{originalSize}_{flags}_{localisationType}_{locale}_{key}{file.RelativePath[file.RelativePath.LastIndexOf('.')..]}";
                File.WriteAllBytes($@"{OutputDirectory}\{fileName}", data);
            }
        }
        catch
        {
            System.Console.WriteLine($"Failed to dump {uri}");
        }
    }
}