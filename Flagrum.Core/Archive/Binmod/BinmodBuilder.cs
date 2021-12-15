using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
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
                replacements,
                out var materialType,
                out var extras);

            // TODO: Remove this!
            foreach (var (extraUri, extraData) in extras)
            {
                _packer.AddFile(extraData, extraUri);
            }
            
            mesh.MaterialType = materialType;

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
        
        // TODO: Remove this!
        foreach (var bone in model.BoneHeaders)
        {
            bone.Name = "Bone";
            bone.LodIndex = 4294967295;
        }
        
        // TODO: Remove this!
        foreach (var mesh in model.MeshObjects[0].Meshes)
        {
            mesh.BoneIds = new[] {0u};
            for (var index = 0; index < mesh.WeightIndices[0].Count; index++)
            {
                var weightArray = mesh.WeightIndices[0][index];
                for (var i = 0; i < weightArray.Length; i++)
                {
                    weightArray[i] = 0;
                }
            }
            
            // TODO: Move this into weapon template
            // mesh.Flags = 262276;
            // mesh.PartsId = 318770505;
            // mesh.MeshParts = new List<MeshPart>
            // {
            //     new()
            //     {
            //         IndexCount = (uint)mesh.FaceIndices.Length,
            //         PartsId = 318770505,
            //         StartIndex = 0
            //     }
            // };
        }
        
        // TODO: Move this into weapon template
        // model.Parts = new List<ModelPart>
        // {
        //     new()
        //     {
        //         Flags = false,
        //         Id = 318770505,
        //         Name = "Base_Parts",
        //         Unknown = ""
        //     }
        // };

        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        AddFile(GetDataPath($"{_mod.ModelName}.gmdl.gfxbin"), gfxData);
        AddFile(GetDataPath($"{_mod.ModelName}.gpubin"), gpuData);

        // foreach (var path in shaders)
        // {
        //     var location = path.Replace("data://",
        //             "C:\\Users\\Kieran\\AppData\\Local\\SquareEnix\\FFXVMODTool\\LuminousStudio\\bin\\rt2\\pc\\")
        //         .Replace('/', '\\')
        //         .Replace(".tif", ".btex");
        //     
        //     AddFile(path, File.ReadAllBytes(location));
        // }
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