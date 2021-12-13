using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmdl.Templates;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Services.Logging;
using Flagrum.Core.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Flagrum.Core.Archive.Binmod;

public enum Boye
{
    Noctis,
    Prompto,
    Ignis,
    Gladiolus
}

public class BinmodBuilder
{
    private readonly Binmod _mod;
    private readonly Packer _packer;

    public BinmodBuilder(string btexConverterPath, Binmod mod, byte[] previewImage)
    {
        _mod = mod;
        _packer = new Packer();
        _packer.AddFile(Modmeta.Build(_mod), GetDataPath("index.modmeta"));

        var exml = EntityPackageBuilder.BuildExml(_mod);
        _packer.AddFile(exml, "data://$mod/temp.exml");

        var tempFile = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, previewImage);
        BtexConverter.Convert(btexConverterPath, tempFile, tempFile2, BtexConverter.TextureType.Thumbnail);
        var btex = File.ReadAllBytes(tempFile2);

        _packer.AddFile(previewImage, GetDataPath("$preview.png.bin"));
        _packer.AddFile(btex, GetDataPath("$preview.btex"));
    }

    public void WriteToFile(string outPath)
    {
        _packer.WriteToFile(outPath);
    }

    public void AddFile(string uri, byte[] data)
    {
        _packer.AddFile(data, uri);
    }

    public void AddFmd(string btexConverterPath, byte[] fmd, ILogger logger)
    {
        using var memoryStream = new MemoryStream(fmd);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        var dataEntry = archive.GetEntry("data.json");
        using var dataStream = new MemoryStream();
        var stream = dataEntry.Open();
        stream.CopyTo(dataStream);

        var json = Encoding.UTF8.GetString(dataStream.ToArray());
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

        foreach (var mesh in gpubin.Meshes)
        {
            var replacements = new Dictionary<string, string>();
            foreach (var (textureId, filePath) in mesh.Material.Textures)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    if (MaterialBuilder.DefaultTextures.TryGetValue(textureId, out var replacement))
                    {
                        string replacementUri;
                        if (replacement == "white.btex" || replacement == "white-color.btex")
                        {
                            replacementUri = "data://shader/defaulttextures/white.tif";
                        }
                        else if (replacement == "blue.btex")
                        {
                            replacementUri = "data://shader/defaulttextures/lightblue.tif";
                        }
                        else if (replacement == "black.btex")
                        {
                            replacementUri = "data://shader/defaulttextures/black.tif";
                        }
                        else
                        {
                            replacementUri = "data://shader/defaulttextures/gray.tif";
                        }
                        replacements.Add(textureId, replacementUri);
                        // mesh.Material.TextureData.Add(new TextureData
                        // {
                        //     Id = textureId,
                        //     Uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{replacement}",
                        //     Data = MaterialBuilder.GetDefaultTextureData(replacement)
                        // });
                    }
                }
                else
                {
                    var query = $"{mesh.Name}/{textureId}";
                    var entry = archive.Entries.FirstOrDefault(e => e.FullName.Contains(query));
                    using var textureStream = new MemoryStream();
                    var tempStream = entry.Open();
                    tempStream.CopyTo(textureStream);

                    var extension = filePath.Split('\\', '/').Last().Split('.').Last();
                    var btexFileName = $"{mesh.Name.ToSafeString()}_{textureId.ToSafeString()}{GetTextureSuffix(textureId)}.btex";
                    
                    byte[] btexData;

                    if (extension.ToLower() == "btex")
                    {
                        btexData = textureStream.ToArray();
                    }
                    else
                    {
                        var tempPathOriginal = Path.GetTempFileName();
                        File.WriteAllBytes(tempPathOriginal, textureStream.ToArray());

                        BtexConverter.TextureType textureType;
                        if (textureId.Contains("normal", StringComparison.OrdinalIgnoreCase))
                        {
                            textureType = BtexConverter.TextureType.Normal;
                        }
                        else if (textureId.Contains("basecolor", StringComparison.OrdinalIgnoreCase)
                                 || textureId.Contains("mrs", StringComparison.OrdinalIgnoreCase)
                                 || textureId.Contains("emissive", StringComparison.OrdinalIgnoreCase))
                        {
                            textureType = BtexConverter.TextureType.Color;
                        }
                        else
                        {
                            textureType = BtexConverter.TextureType.Greyscale;
                        }

                        var tempPath = Path.GetTempFileName();
                        BtexConverter.Convert(btexConverterPath, tempPathOriginal, tempPath, textureType);

                        btexData = File.ReadAllBytes(tempPath);
                        File.Delete(tempPathOriginal);
                        File.Delete(tempPath);
                    }

                    var uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{btexFileName}";

                    mesh.Material.TextureData.Add(new TextureData
                    {
                        Id = textureId,
                        Uri = uri,
                        Data = btexData
                    });
                }
            }

            // This is called now as the original name is used to locate files in the previous step
            mesh.Name = mesh.Name.ToSafeString();

            var material = MaterialBuilder.FromTemplate(
                mesh.Material.Id,
                $"{mesh.Name}_mat",
                _mod.ModDirectoryName,
                mesh.Material.Inputs.Select(p => new MaterialInputData
                {
                    Name = p.Key,
                    Values = p.Value
                }).ToList(),
                mesh.Material.TextureData.Select(p => new MaterialTextureData
                {
                    Name = p.Id,
                    Path = p.Uri
                }).ToList(),
                replacements);
            
            // var textureMatch = material.Textures.FirstOrDefault(t => t.Path == "data://character/nh/nh00/model_000/sourceimages/nh00_000_skin_02_b.tif");
            // if (textureMatch != null)
            // {
            //     var dependencyMatch =
            //         material.Header.Dependencies.FirstOrDefault(d => d.PathHash == textureMatch.ResourceFileHash.ToString());
            //
            //     var hashIndex = material.Header.Hashes.IndexOf(textureMatch.ResourceFileHash);
            //     
            //     textureMatch.Path = $"data://mod/{_mod.ModDirectoryName}/sourceimages/bodyshape_basecolor0_texture_b.btex";
            //     textureMatch.PathHash = Cryptography.Hash32(textureMatch.Path);
            //     textureMatch.ResourceFileHash = Cryptography.HashFileUri64(textureMatch.Path);
            //
            //     dependencyMatch.Path = textureMatch.Path;
            //     dependencyMatch.PathHash = textureMatch.ResourceFileHash.ToString();
            //     material.Header.Hashes[hashIndex] = textureMatch.ResourceFileHash;
            // }
            
            logger.LogInformation($"Material Name: {material.Name} ({Cryptography.Hash32(material.Name)})");
            logger.LogInformation($"Material Hash: {material.NameHash}");
            logger.LogInformation($"Material Uri: {material.Uri}");

            foreach (var dependency in material.Header.Dependencies)
            {
                logger.LogInformation($"{dependency.PathHash}: {dependency.Path}");
            }

            foreach (var texture in material.Textures)
            {
                logger.LogInformation($"{texture.NameHash}: {texture.Name}");
                logger.LogInformation($"{texture.PathHash}: {texture.Path}");
                logger.LogInformation($"{texture.ShaderGenNameHash}: {texture.ShaderGenName}");
                logger.LogInformation($"{texture.ResourceFileHash}");
                var path =
                    $"data://mod/{_mod.Uuid}/sourceimages/{texture.ShaderGenName.ToLower()}{GetTextureSuffix(texture.ShaderGenName)}.btex";
                logger.LogInformation($"{Cryptography.HashFileUri64(path)}: {path}");
            }
            
            var materialWriter = new MaterialWriter(material);
            AddFile(material.Uri, materialWriter.Write());

            foreach (var texture in mesh.Material.TextureData)
            {
                if (!_packer.HasFile(texture.Uri))
                {
                    AddFile(texture.Uri, texture.Data);
                }
            }
        }

        var model = OutfitTemplate.Build(_mod.ModDirectoryName, _mod.ModelName, gpubin);
        var replacer = new ModelReplacer(model, gpubin, BinmodTypeHelper.GetModmetaTargetName(_mod.Type, _mod.Target));
        model = replacer.Replace();

        logger.LogInformation("Model Name: " + model.Name);
        logger.LogInformation($"Model Hash: {model.AssetHash}");
        logger.LogInformation("\nModel Dependencies:");
        foreach (var dependency in model.Header.Dependencies)
        {
            logger.LogInformation($"{dependency.PathHash}: {dependency.Path}");
        }

        logger.LogInformation("\nModel Materials:");
        foreach (var mesh in model.MeshObjects[0].Meshes)
        {
            logger.LogInformation($"{mesh.DefaultMaterialHash}");

            var materialPath = $"data://mod/{_mod.Uuid}/materials/{mesh.Name}_mat.gmtl";
            logger.LogInformation($"{Cryptography.HashFileUri64(materialPath)}: {materialPath}");
        }
        
        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        AddFile(GetDataPath($"{_mod.ModelName}.gmdl.gfxbin"), gfxData);
        AddFile(GetDataPath($"{_mod.ModelName}.gpubin"), gpuData);
    }

    private string GetDataPath(string relativePath)
    {
        return $"data://mod/{_mod.ModDirectoryName}/{relativePath}";
    }

    private string GetTextureSuffix(string textureId)
    {
        if (textureId.ToLower().Contains("normal"))
        {
            return "_n";
        }

        if (textureId.ToLower().Contains("basecolor"))
        {
            return "_b";
        }

        if (textureId.ToLower().Contains("mrs"))
        {
            return "_mrs";
        }

        if (textureId.ToLower().Contains("occlusion"))
        {
            return "_o";
        }

        if (textureId.ToLower().Contains("opacity"))
        {
            return "_a";
        }

        if (textureId.ToLower().Contains("emissive"))
        {
            return "_e";
        }

        return "";
    }
}