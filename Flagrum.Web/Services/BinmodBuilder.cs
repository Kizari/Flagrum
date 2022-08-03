using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmdl.Templates;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.ModLibrary.Data;

namespace Flagrum.Web.Services;

public class BinmodBuilder
{
    private readonly EntityPackageBuilder _entityPackageBuilder;
    private readonly Modmeta _modmeta;

    private Binmod _mod;
    private Packer _packer;

    public BinmodBuilder(
        EntityPackageBuilder entityPackageBuilder,
        Modmeta modmeta)
    {
        _entityPackageBuilder = entityPackageBuilder;
        _modmeta = modmeta;
    }

    public void Initialise(Binmod mod, BuildContext context)
    {
        _mod = mod;
        _packer = new Packer();
        _packer.AddFile(_modmeta.Build(_mod), GetDataPath("index.modmeta"));

        var exml = _entityPackageBuilder.BuildExml(_mod);
        _packer.AddFile(exml, "data://$mod/temp.exml");

        _packer.AddFile(context.PreviewImage, GetDataPath("$preview.png.bin"));
        _packer.AddFile(context.PreviewBtex, GetDataPath("$preview.png"));

        if (mod.Type == (int)BinmodType.StyleEdit)
        {
            _packer.AddFile(context.ThumbnailImage, GetDataPath("default.png.bin"));
            _packer.AddFile(context.ThumbnailBtex, GetDataPath("default.png"));
        }
    }

    public void WriteToFile(string outPath)
    {
        _packer.WriteToFile(outPath);
    }

    public void AddFile(string uri, byte[] data)
    {
        _packer.AddFile(data, uri);
    }

    public void AddModelFromExisting(Binmod mod, int index)
    {
        using var unpacker = new Unpacker(mod.Path);
        var modelName = index == 0 ? mod.Model1Name : mod.Model2Name;

        var gmdl = unpacker.UnpackFileByQuery($"{modelName}.gmdl", out var gmdlUri);
        var gpubin = unpacker.UnpackFileByQuery($"{modelName}.gpubin", out var gpubinUri);
        var reader = new ModelReader(gmdl, gpubin);
        var model = reader.Read();

        var materialPaths = model.Header.Dependencies
            .Where(d => d.Path.EndsWith(".gmtl"))
            .Select(d => d.Path);

        foreach (var material in materialPaths)
        {
            var data = unpacker.UnpackFileByQuery(material, out var materialUri);
            var materialReader = new MaterialReader(data);
            var materialObject = materialReader.Read();
            var texturePaths = materialObject.Textures
                .Where(t => t.Path.StartsWith("data://mod"))
                .Select(t => t.Path);

            foreach (var texture in texturePaths)
            {
                var textureData = unpacker.UnpackFileByQuery(texture, out var textureUri);
                _packer.AddFile(textureData, textureUri);
            }

            _packer.AddFile(data, materialUri);
        }

        _packer.AddFile(gmdl, gmdlUri);
        _packer.AddFile(gpubin, gpubinUri);
    }

    public void AddFmd(int modelIndex, FmdData fmd)
    {
        var modelNamePrefix = modelIndex switch
        {
            0 => _mod.Model1Name + "_",
            1 => _mod.Model2Name + "_",
            _ => string.Empty
        };

        foreach (var mesh in fmd.Gpubin.Meshes)
        {
            mesh.UpdateForPartsSystem();
            var replacements = new Dictionary<string, string>();
            foreach (var (textureId, filePath) in mesh.Material.Textures)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    if (MaterialBuilder.DefaultTextures.TryGetValue(textureId, out var replacement))
                    {
                        string replacementUri = null;
                        switch (replacement)
                        {
                            case "white.btex" or "white-color.btex":
                                replacementUri = "data://shader/defaulttextures/white.tif";
                                break;
                            case "blue.btex":
                                replacementUri = "data://shader/defaulttextures/lightblue.tif";
                                break;
                            case "black.btex":
                                replacementUri = "data://shader/defaulttextures/black.tif";
                                break;
                            default:
                                mesh.Material.TextureData.Add(new TextureData
                                {
                                    Id = textureId,
                                    Uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{replacement}",
                                    Data = MaterialBuilder.GetDefaultTextureData(replacement)
                                });
                                break;
                        }

                        if (replacementUri != null)
                        {
                            replacements.Add(textureId, replacementUri);
                        }
                    }
                }
                else
                {
                    var btexFileName =
                        $"{modelNamePrefix}{mesh.Name.ToSafeString()}_{textureId.ToSafeString()}{BtexHelper.GetSuffix(textureId)}.btex";

                    var uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{btexFileName}";

                    mesh.Material.TextureData.Add(new TextureData
                    {
                        Id = textureId,
                        Uri = uri,
                        Data = fmd.Textures.First(t => t.Mesh == mesh.Name && t.TextureSlot == textureId).Data
                    });
                }
            }

            // This is called now as the original name is used to locate files in the previous step
            mesh.Name = mesh.Name.ToSafeString();

            var material = MaterialBuilder.FromTemplate(
                mesh.Material.Id,
                $"{modelNamePrefix}{mesh.Name}_mat",
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
                fmd.Materials);

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

        var modelName = modelIndex switch
        {
            0 => _mod.Model1Name,
            1 => _mod.Model2Name,
            _ => _mod.ModelName
        };

        var model = OutfitTemplate.Build(_mod.ModDirectoryName, modelName, modelNamePrefix, fmd.Gpubin);
        var replacer = new ModelReplacer(model, fmd.Gpubin);
        model = replacer.Replace(_mod.Type == (int)BinmodType.Character);

        var parts = model.MeshObjects.SelectMany(mo => mo.Meshes
                .SelectMany(m => m.MeshParts
                    .Select(p => p.PartsId)))
            .Distinct()
            .OrderBy(p => p);

        model.Parts = parts.Select(p => new ModelPart
        {
            Name = PartsId.Dictionary.FirstOrDefault(kvp => kvp.Value == p).Key,
            Id = p,
            Flags = false,
            Unknown = string.Empty
        });

        if (_mod.Type is (int)BinmodType.Weapon or (int)BinmodType.Multi_Weapon)
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
            if (_mod.Type != (int)BinmodType.Weapon && _mod.Type != (int)BinmodType.Multi_Weapon &&
                mesh.ColorMaps.Count < 3)
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

        File.WriteAllBytes(@"C:\Users\Kieran\Desktop\parts.gmdl.gfxbin", gfxData);
        File.WriteAllBytes(@"C:\Users\Kieran\Desktop\parts.gpubin", gpuData);

        AddFile(GetDataPath($"{modelName}.gmdl"), gfxData);
        AddFile(GetDataPath($"{modelName}.gpubin"), gpuData);
    }

    private string GetDataPath(string relativePath)
    {
        return $"data://mod/{_mod.ModDirectoryName}/{relativePath}";
    }
}