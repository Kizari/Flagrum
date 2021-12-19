using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmdl.Templates;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.ModLibrary.Data;

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

    public void Initialise(Binmod mod, byte[] previewImage, byte[] previewBtex, byte[] thumbnailBtex)
    {
        _mod = mod;
        _packer = new Packer();
        _packer.AddFile(_modmeta.Build(_mod), GetDataPath("index.modmeta"));

        var exml = _entityPackageBuilder.BuildExml(_mod);
        _packer.AddFile(exml, "data://$mod/temp.exml");

        _packer.AddFile(previewImage, GetDataPath("$preview.png.bin"));
        _packer.AddFile(previewBtex, GetDataPath("$preview.png"));

        if (mod.Type == (int)BinmodType.StyleEdit)
        {
            _packer.AddFile(thumbnailBtex, GetDataPath("default.png"));
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

    public void AddFmd(Gpubin gpubin, IEnumerable<FmdTexture> textures)
    {
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
                    var btexFileName =
                        $"{mesh.Name.ToSafeString()}_{textureId.ToSafeString()}{BtexHelper.GetSuffix(textureId)}.btex";

                    var uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{btexFileName}";

                    mesh.Material.TextureData.Add(new TextureData
                    {
                        Id = textureId,
                        Uri = uri,
                        Data = textures.First(t => t.Mesh == mesh.Name && t.TextureSlot == textureId).Data
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
}