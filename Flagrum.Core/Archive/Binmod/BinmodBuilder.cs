using System;
using System.Collections;
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

    public void AddFmd(string btexConverterPath, byte[] fmd, ILogger logger, string gameDataDirectory)
    {
        using var memoryStream = new MemoryStream(fmd);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        var dataEntry = archive.GetEntry("data.json");
        using var dataStream = new MemoryStream();
        var stream = dataEntry.Open();
        stream.CopyTo(dataStream);

        var json = Encoding.UTF8.GetString(dataStream.ToArray());
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);
        var dependencies = new List<string>();
        
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

            dependencies.AddRange(extras);
            // // TODO: Remove this!
            // foreach (var (extraUri, extraData) in extras)
            // {
            //     _packer.AddFile(extraData, extraUri);
            // }
            
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

        if (_mod.Type == (int)BinmodType.Weapon)
        {
            foreach (var bone in model.BoneHeaders)
            {
                bone.Name = "C_Body";
                bone.LodIndex = 4294967295;
            }

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
            }
        }
        
        // // TODO: Remove this!
        // foreach (var bone in model.BoneHeaders)
        // {
        //     bone.Name = "Bone";
        //     bone.LodIndex = 4294967295;
        // }
        //
        // // TODO: Remove this!
        // foreach (var mesh in model.MeshObjects[0].Meshes)
        // {
        //     mesh.BoneIds = new[] {0u};
        //     for (var index = 0; index < mesh.WeightIndices[0].Count; index++)
        //     {
        //         var weightArray = mesh.WeightIndices[0][index];
        //         for (var i = 0; i < weightArray.Length; i++)
        //         {
        //             weightArray[i] = 0;
        //         }
        //     }
        //     
        //     // TODO: Move this into weapon template
        //     // mesh.Flags = 262276;
        //     // mesh.PartsId = 318770505;
        //     // mesh.MeshParts = new List<MeshPart>
        //     // {
        //     //     new()
        //     //     {
        //     //         IndexCount = (uint)mesh.FaceIndices.Length,
        //     //         PartsId = 318770505,
        //     //         StartIndex = 0
        //     //     }
        //     // };
        // }
        
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
        
        // TODO: Remove this test code
        // var boneJson = File.ReadAllText("C:\\Modding\\ModelReplacementTesting\\bones.json");
        // var originalBones = JsonConvert.DeserializeObject<IEnumerable<BoneHeader>>(boneJson);
        // var boneMap = new Dictionary<uint, uint>();
        // foreach (var bone in model.BoneHeaders)
        // {
        //     var match = originalBones.FirstOrDefault(b => b.Name == bone.Name);
        //     if (match == null)
        //     {
        //         boneMap.Add(bone.LodIndex >> 16, bone.LodIndex >> 16);
        //     }
        //     else
        //     {
        //         boneMap.Add(bone.LodIndex >> 16, match.UniqueIndex);
        //     }
        // }
        //
        // model.BoneHeaders = originalBones.ToList();

        // model.Parts = new List<ModelPart>
        // {
        //     new()
        //     {
        //         Flags = false,
        //         Id = 318770505,
        //         Name = "Parts_Base",
        //         Unknown = ""
        //     }
        // };
        foreach (var mesh in model.MeshObjects[0].Meshes)
        {
            //mesh.BoneIds = Enumerable.Range(0, 2100).Select(i => (uint)i);
            // foreach (var weightMap in mesh.WeightIndices)
            // {
            //     foreach (var vertexWeights in weightMap)
            //     {
            //         for (var i = 0; i < vertexWeights.Length; i++)
            //         {
            //             vertexWeights[i] = (ushort)boneMap[vertexWeights[i]];
            //         }
            //     }
            // }
            mesh.ColorMaps = new List<ColorMap>();
            for (var i = 0; i < 4; i++)
            {
                var colorMap = new ColorMap();
                colorMap.Colors = new List<Color4>();
                for (var j = 0; j < mesh.VertexPositions.Count(); j++)
                {
                    colorMap.Colors.Add(new Color4 {R = 255, G = 255, B = 255, A = 255});
                }
                mesh.ColorMaps.Add(colorMap);
            }
            
            // mesh.Flags = 262276;
            // mesh.LodNear = 0.0f;
            // mesh.LodFar = 3.5f;
            // mesh.PartsId = 318770505;
            // mesh.VertexLayoutType = mesh.MaterialType switch
            // {
            //     MaterialType.OneWeight => VertexLayoutType.Skinning_1Bones,
            //     MaterialType.SixWeights => VertexLayoutType.Skinning_6Bones,
            //     _ => VertexLayoutType.Skinning_4Bones
            // };
            // mesh.MeshParts = new List<MeshPart>
            // {
            //     new()
            //     {
            //         IndexCount = (uint)mesh.FaceIndices.Length,
            //         PartsId = 318770505,
            //         StartIndex = 0
            //     }
            // };
            //
            // var meshCloneJson = JsonConvert.SerializeObject(mesh);
            // var lodNears = new[] {0.0f, 3.5f, 10f, 25f, 45f, 65f, 85f};
            // var lodFars = new[] {3.5f, 10f, 25f, 45f, 65f, 85f, 120f};
            // for (int i = 1; i < 7; i++)
            // {
            //     var meshClone = JsonConvert.DeserializeObject<Mesh>(meshCloneJson);
            //     meshClone.LodNear = lodNears[i];
            //     meshClone.LodFar = lodFars[i];
            //     meshClones.Add(meshClone);
            // }
        }
        
        //model.MeshObjects[0].Meshes.AddRange(meshClones);

        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        AddFile(GetDataPath($"{_mod.ModelName}.gmdl"), gfxData);
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
        
        //AddGameAssets(dependencies, gameDataDirectory);
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
    
    /// <summary>
    ///     Add a copy of a game asset to the archive
    ///     Asset will be read from the EARC and copied to the archive
    /// </summary>
    public void AddGameAssets(IEnumerable<string> paths, string dataDirectory)
    {
        dataDirectory += '\\';
        var archiveDictionary = new Dictionary<string, List<string>>();

        foreach (var uri in paths)
        {
            var path = uri.Replace("data://", dataDirectory).Replace('/', '\\');
            var fileName = path.Split('\\').Last();
            path = path.Replace(fileName, "autoexternal.earc");

            while (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                var newPath = "";
                var tokens = path.Split('\\');
                for (var i = 0; i < tokens.Length - 2; i++)
                {
                    newPath += tokens[i];

                    if (i != tokens.Length - 3)
                    {
                        newPath += '\\';
                    }
                }

                path = newPath + "\\autoexternal.earc";
            }

            if (!archiveDictionary.ContainsKey(path))
            {
                archiveDictionary.Add(path, new List<string>());
            }

            archiveDictionary[path].Add(uri);
        }

        foreach (var (archivePath, uriList) in archiveDictionary)
        {
            var unpacker = new Unpacker(archivePath);

            foreach (var uri in uriList)
            {
                var fileData = unpacker.UnpackFileByQuery(uri, out var flags);
                if (fileData.Length == 0)
                {
                    throw new InvalidOperationException($"URI {uri} must exist in game files!");
                }

                if (flags.HasFlag(ArchiveFileFlag.Compressed))
                {
                    _packer.AddCompressedFile(fileData, uri);
                }
                else
                {
                    _packer.AddFile(fileData, uri);
                }
            }
        }
    }
}