using System.Collections.Generic;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Archive;
using Flagrum.Core.Archive.Mod;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Templates;
using Flagrum.Core.Graphics.Materials;
using Flagrum.Core.Graphics.Models;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.WorkshopMods.Data;
using Flagrum.Application.Features.WorkshopMods.Data.Model;

namespace Flagrum.Application.Services;

public class BinmodBuilder(
    EntityPackageBuilder entityPackageBuilder,
    Modmeta modmeta)
{
    private Binmod _mod;
    private EbonyArchive _packer;

    public void Initialise(Binmod mod, WorkshopModBuildContext context)
    {
        _mod = mod;
        _packer = new EbonyArchive(true);
        _packer.AddFile(modmeta.Build(_mod), GetDataPath("index.modmeta"));

        var exml = entityPackageBuilder.BuildExml(_mod);
        _packer.AddFile(exml, "data://$mod/temp.exml");

        _packer.AddFile(context.PreviewImage, GetDataPath("$preview.png.bin"));
        _packer.AddFile(context.PreviewBtex, GetDataPath("$preview.png"));

        if (mod.Type == (int)WorkshopModType.StyleEdit)
        {
            _packer.AddFile(context.ThumbnailImage, GetDataPath("default.png.bin"));
            _packer.AddFile(context.ThumbnailBtex, GetDataPath("default.png"));
        }
    }

    public void WriteToFile(string outPath)
    {
        _packer.WriteToFile(outPath, LuminousGame.FFXV);
    }

    public void AddFile(string uri, byte[] data)
    {
        _packer.AddFile(data, uri);
    }

    public void AddModelFromExisting(Binmod mod, int index)
    {
        using var unpacker = new EbonyArchive(mod.Path);
        var modelName = index == 0 ? mod.Model1Name : mod.Model2Name;

        var gmdlFile = unpacker.Files
            .First(f => f.Value.Uri.EndsWith($"{modelName}.gmdl")).Value;

        var gpubinFile = unpacker.Files
            .First(f => f.Value.Uri.EndsWith($"{modelName}.gpubin")).Value;

        var gmdl = gmdlFile.GetReadableData();
        var gpubin = gpubinFile.GetReadableData();
        var gmdlUri = gmdlFile.Uri;
        var gpubinUri = gpubinFile.Uri;

        var model = new GameModel();
        model.Read(gmdl);
        model.ReadVertexData(gpubin);

        var materialPaths = model.Dependencies
            .Where(d => d.Value.EndsWith(".gmtl"))
            .Select(d => d.Value);

        foreach (var material in materialPaths)
        {
            var data = unpacker[material].GetReadableData();

            var materialObject = new GameMaterial();
            materialObject.Read(data);

            var texturePaths = materialObject.Textures
                .Where(t => t.Uri.StartsWith("data://mod"))
                .Select(t => t.Uri);

            foreach (var texture in texturePaths)
            {
                var textureData = unpacker[texture].GetReadableData();
                _packer.AddFile(textureData, texture);
            }

            _packer.AddFile(data, material);
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
            var replacements = new Dictionary<string, string>();
            foreach (var (textureId, filePath) in mesh.Material.Textures)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    if (BinmodMaterialBuilder.DefaultTextures.TryGetValue(textureId, out var replacement))
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
                                    Data = BinmodMaterialBuilder.GetDefaultTextureData(replacement)
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
                        $"{modelNamePrefix}{mesh.Name.ToSafeString()}_{textureId.ToSafeString()}{BinmodTextureHelper.GetSuffix(textureId)}.btex";

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

            var material = BinmodMaterialBuilder.FromTemplate(
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

            material.UpdateValueBuffer();
            AddFile(material.Dependencies["ref"], material.Write());

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
        model = replacer.Replace((WorkshopModType)_mod.Type);

        AddFile(GetDataPath($"{modelName}.gpubin"), model.WriteGpubin());
        AddFile(GetDataPath($"{modelName}.gmdl"), model.Write());
    }

    private string GetDataPath(string relativePath) => $"data://mod/{_mod.ModDirectoryName}/{relativePath}";
}