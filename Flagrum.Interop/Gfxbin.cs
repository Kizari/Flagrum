using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Flagrum.Archiver.Binmod;
using Flagrum.Gfxbin.Btex;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Constructs;
using Flagrum.Gfxbin.Gmdl.Templates;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Interop;

public static class Gfxbin
{
    [UnmanagedCallersOnly]
    public static IntPtr Import(IntPtr pathPointer)
    {
        var gfxbinPath = Marshal.PtrToStringUni(pathPointer);
        var gpubinPath = gfxbinPath.Replace(".gmdl.gfxbin", ".gpubin");
        var gfxbin = File.ReadAllBytes(gfxbinPath);
        var gpubin = File.ReadAllBytes(gpubinPath);
        var reader = new ModelReader(gfxbin, gpubin);
        var model = reader.Read();

        Dictionary<int, string> boneTable;
        if (model.BoneHeaders.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
        {
            // Probably a broken MO gfxbin with all IDs set to this value
            var arbitraryIndex = 0;
            boneTable = model.BoneHeaders.ToDictionary(b => arbitraryIndex++, b => b.Name);
        }
        else
        {
            boneTable = model.BoneHeaders.ToDictionary(b => (int)b.UniqueIndex, b => b.Name);
        }

        var meshData = new Gpubin
        {
            BoneTable = boneTable,
            Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                .Where(m => m.LodNear == 0)
                .Select(m => new GpubinMesh
                {
                    Name = m.Name,
                    FaceIndices = m.FaceIndices,
                    VertexPositions = m.VertexPositions,
                    Normals = m.Normals,
                    UVMaps = m.UVMaps.Select(m => new UVMap32
                    {
                        UVs = m.UVs.Select(uv => new UV32
                        {
                            U = (float)uv.U,
                            V = (float)uv.V
                        }).ToList()
                    }).ToList(),
                    WeightIndices = m.WeightIndices,
                    WeightValues = m.WeightValues.Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
                        .ToList()
                }))
        };

        var json = JsonConvert.SerializeObject(meshData);
        var temp = Path.GetTempFileName();
        File.WriteAllText(temp, json);

        var pointer = Marshal.StringToHGlobalAnsi(temp);
        return pointer;
    }

    [UnmanagedCallersOnly]
    public static IntPtr Export(IntPtr pathPointer)
    {
        try
        {
            var jsonPath = Marshal.PtrToStringUni(pathPointer);
            var json = File.ReadAllText(jsonPath);
            var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);
            File.Delete(jsonPath);

            var builder = new BinmodBuilder(
                gpubin.Title,
                gpubin.Title.ToLower().Replace(" ", "_"),
                gpubin.Title.ToLower().Replace(" ", ""),
                gpubin.Uuid,
                gpubin.Target switch
                {
                    "NOCTIS" => Boye.Noctis,
                    "PROMPTO" => Boye.Prompto,
                    "IGNIS" => Boye.Ignis,
                    "GLADIOLUS" => Boye.Gladiolus,
                    _ => Boye.Noctis
                });

            var gameAssets = new List<string>();

            foreach (var mesh in gpubin.Meshes)
            {
                foreach (var (textureId, filePath) in mesh.Material.Textures)
                {
                    if (string.IsNullOrEmpty(filePath))
                    {
                        if (MaterialBuilder.DefaultTextures.TryGetValue(textureId, out var replacement))
                        {
                            mesh.Material.TextureData.Add(new TextureData
                            {
                                Id = textureId,
                                Uri = $"data://mod/{builder.ModDirectoryName}/sourceimages/{replacement}",
                                Data = MaterialBuilder.GetDefaultTextureData(replacement)
                            });
                        }
                    }
                    else
                    {
                        var tempPath = Path.GetTempFileName();
                        BtexConverter.Convert(filePath, tempPath,
                            textureId.ToLower().Contains("normal")
                                ? BtexConverter.TextureType.Normal
                                : BtexConverter.TextureType.Color);

                        var btexData = File.ReadAllBytes(tempPath);
                        File.Delete(tempPath);
                        var fileName = filePath.Split('/', '\\').Last();
                        var extension = fileName.Split('.').Last();
                        var btexFileName = fileName.Replace($".{extension}", ".btex");
                        var uri = $"data://mod/{builder.ModDirectoryName}/sourceimages/{btexFileName}";

                        mesh.Material.TextureData.Add(new TextureData
                        {
                            Id = textureId,
                            Uri = uri,
                            Data = btexData
                        });
                    }
                }

                var material = MaterialBuilder.FromTemplate(
                    mesh.Material.Id,
                    $"{mesh.Name.ToLower()}_mat",
                    builder.ModDirectoryName,
                    mesh.Material.Inputs.Select(p => new MaterialInputData
                    {
                        Name = p.Key,
                        Values = p.Value
                    }).ToList(),
                    mesh.Material.TextureData.Select(p => new MaterialTextureData
                    {
                        Name = p.Id,
                        Path = p.Uri
                    }).ToList());

                var materialWriter = new MaterialWriter(material);

                builder.AddFile(material.Uri, materialWriter.Write());

                foreach (var texture in mesh.Material.TextureData)
                {
                    builder.AddFile(texture.Uri, texture.Data);
                }

                // TODO: Update the EARC file puller to operate quicker
                // One improvement may be to only read the data for the files we need
                gameAssets.AddRange(material.ShaderBinaries.Select(s => s.Path));
                gameAssets.AddRange(material.Textures
                    .Where(t => !t.Path.StartsWith("data://mod"))
                    .Select(t => t.Path));
            }

            var model = OutfitTemplate.Build(builder.ModDirectoryName, builder.ModelName, gpubin);
            var replacer = new ModelReplacer(model, gpubin);
            model = replacer.Replace();
            var writer = new ModelWriter(model);
            var (gfxData, gpuData) = writer.Write();
            builder.AddModel(builder.ModelName, gfxData, gpuData);

            builder.AddGameAssets(gameAssets.Distinct());
            builder.WriteToFile(jsonPath.Replace(".json", ""));
        }
        catch (Exception e)
        {
            Console.WriteLine("[E] Failed to export FFXVBINMOD!");
            Console.WriteLine(e.Message);
        }

        var pointer = Marshal.StringToHGlobalAnsi("Done");
        return pointer;
    }
}