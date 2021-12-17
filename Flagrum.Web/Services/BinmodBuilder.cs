using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmdl.Templates;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Utilities;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class BinmodBuilder
{
    private readonly BinmodTypeHelper _binmodType;
    private readonly EntityPackageBuilder _entityPackageBuilder;
    private readonly Modmeta _modmeta;
    private readonly Settings _settings;

    private Binmod _mod;
    private Packer _packer;

    public BinmodBuilder(
        Settings settings,
        EntityPackageBuilder entityPackageBuilder,
        Modmeta modmeta,
        BinmodTypeHelper binmodType)
    {
        _settings = settings;
        _entityPackageBuilder = entityPackageBuilder;
        _modmeta = modmeta;
        _binmodType = binmodType;
    }

    public void Initialise(Binmod mod, byte[] previewImage)
    {
        _mod = mod;
        _packer = new Packer();
        _packer.AddFile(_modmeta.Build(_mod), GetDataPath("index.modmeta"));

        var exml = _entityPackageBuilder.BuildExml(_mod);
        _packer.AddFile(exml, "data://$mod/temp.exml");

        var converter = new TextureConverter();
        var btex = converter.Convert("$preview", "png", TextureType.Color, previewImage);

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

    public void AddFmd(byte[] fmd)
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
                        string replacementUri = null;
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
                            mesh.Material.TextureData.Add(new TextureData
                            {
                                Id = textureId,
                                Uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{replacement}",
                                Data = MaterialBuilder.GetDefaultTextureData(replacement)
                            });
                        }

                        if (replacementUri != null)
                        {
                            replacements.Add(textureId, replacementUri);
                        }
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
                    var btexFileName =
                        $"{mesh.Name.ToSafeString()}_{textureId.ToSafeString()}{GetTextureSuffix(textureId)}.btex";

                    byte[] btexData;

                    if (extension.ToLower() == "btex")
                    {
                        btexData = textureStream.ToArray();
                    }
                    else
                    {
                        var textureBytes = textureStream.ToArray();

                        TextureType textureType;
                        if (textureId.Contains("normal", StringComparison.OrdinalIgnoreCase))
                        {
                            textureType = TextureType.Normal;
                        }
                        else if (textureId.Contains("basecolor", StringComparison.OrdinalIgnoreCase)
                                 || textureId.Contains("mrs", StringComparison.OrdinalIgnoreCase)
                                 || textureId.Contains("emissive", StringComparison.OrdinalIgnoreCase))
                        {
                            textureType = TextureType.Color;
                        }
                        else
                        {
                            textureType = TextureType.Greyscale;
                        }
                        
                        var converter = new TextureConverter();
                        btexData = converter.Convert(btexFileName.Replace(".btex", ""), extension, textureType, textureBytes);
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
                out var materialType);

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
        var replacer = new ModelReplacer(model, gpubin, _binmodType.GetModmetaTargetName(_mod.Type, _mod.Target));
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

        foreach (var mesh in model.MeshObjects[0].Meshes)
        {
            // Ensure a white color AO color layer if no AO layer exists
            // to avoid vantablack model replacements and snapshots
            if (_mod.Type != (int)BinmodType.Weapon && mesh.ColorMaps.Count < 3)
            {
                while (mesh.ColorMaps.Count < 3)
                {
                    mesh.ColorMaps.Add(new ColorMap());
                }

                mesh.ColorMaps[2].Colors = new List<Color4>();
                for (var j = 0; j < mesh.VertexPositions.Count; j++)
                {
                    mesh.ColorMaps[2].Colors.Add(new Color4 {R = 255, G = 255, B = 255, A = 255});
                }
            }
        }

        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        AddFile(GetDataPath($"{_mod.ModelName}.gmdl"), gfxData);
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