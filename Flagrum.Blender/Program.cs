using System.Text;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Newtonsoft.Json;

namespace Flagrum.Blender;

public static class Program
{
    public static void Main(string[] args)
    {
        var command = args[0];
        var parameterInput = args[1];
        var inputPath = args[2];
        var parameterOutput = args[3];
        var outputPath = args[4];

        switch (command)
        {
            case "import":
                Import(inputPath, outputPath);
                break;
            case "material":
                Material(inputPath, outputPath);
                break;
        }
    }

    private static void Material(string inputPath, string outputPath)
    {
        var reader = new MaterialReader(inputPath);
        var material = reader.Read();

        var result = material.InterfaceInputs
            .Where(i => i.InterfaceIndex == 0)
            .ToDictionary(i => i.ShaderGenName, i => i.Values);

        var json = JsonConvert.SerializeObject(result);
        File.WriteAllText(outputPath, json);
    }

    private static void Import(string inputPath, string outputPath)
    {
        var rootDirectory = string.Join('\\', inputPath.Split('\\')[..^1]);
        var gfxbin = File.ReadAllBytes(inputPath);
        var gpubin = File.ReadAllBytes(inputPath.Replace(".gmdl.gfxbin", ".gpubin"));
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
            boneTable = model.BoneHeaders.ToDictionary(b => (int)(b.UniqueIndex == 65535 ? 0 : b.UniqueIndex),
                b => b.Name);
        }

        var meshData = new Gpubin
        {
            BoneTable = boneTable,
            Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                .Where(m => m.LodNear == 0)
                .Select(m =>
                {
                    var materialPath = model.Header.Dependencies
                        .FirstOrDefault(d => d.PathHash == m.DefaultMaterialHash.ToString())
                        !.Path
                        .Split('/')
                        .Last() + ".gfxbin";

                    Material material = null;
                    var materialFilePath = $"{rootDirectory}\\materials\\{materialPath}";
                    if (File.Exists(materialFilePath))
                    {
                        material = new MaterialReader(materialFilePath).Read();
                    }
                    else
                    {
                        Console.WriteLine($"Couldn't find {materialFilePath}");
                    }

                    var mesh = new GpubinMesh
                    {
                        Name = m.Name,
                        FaceIndices = m.FaceIndices,
                        VertexPositions = m.VertexPositions,
                        ColorMaps = m.ColorMaps,
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
                        WeightValues = m.WeightValues
                            .Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
                            .ToList()
                    };

                    if (material != null)
                    {
                        var highResTextures = GetHighResTextures(rootDirectory, material);

                        mesh.BlenderMaterial = new BlenderMaterialData
                        {
                            Hash = m.DefaultMaterialHash.ToString(),
                            Name = material.Name,
                            Textures = material.Textures
                                .Where(t => t.ResourceFileHash > 0 && t.Path != null)
                                .Select(t =>
                                {
                                    var fileName = t.Path.Split('/').Last();
                                    var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];
                                    var highResPath = highResTextures
                                        .FirstOrDefault(t =>
                                            t.Contains(fileNameWithoutExtension + "_$h.") && t.Contains("highimages"));

                                    if (highResPath == null)
                                    {
                                        highResPath = highResTextures
                                            .FirstOrDefault(t => t.Contains(fileNameWithoutExtension + "_$h."));
                                    }

                                    if (highResPath != null)
                                    {
                                        fileNameWithoutExtension += "_$h";
                                    }

                                    var highImagesDirectory = $"{rootDirectory}\\highimages";
                                    var sourceImagesDirectory = $"{rootDirectory}\\sourceimages";
                                    var highImagesFiles = Directory.Exists(highImagesDirectory)
                                        ? Directory.EnumerateFiles(highImagesDirectory)
                                            .Where(f => !f.EndsWith(".btex"))
                                        : new List<string>();
                                    var sourceImagesFiles = Directory.Exists(sourceImagesDirectory)
                                        ? Directory.EnumerateFiles(sourceImagesDirectory)
                                            .Where(f => !f.EndsWith(".btex"))
                                        : new List<string>();

                                    var finalPath = highImagesFiles.FirstOrDefault(f =>
                                                        f.Contains(fileNameWithoutExtension,
                                                            StringComparison.OrdinalIgnoreCase)) ??
                                                    sourceImagesFiles.FirstOrDefault(f =>
                                                        f.Contains(fileNameWithoutExtension,
                                                            StringComparison.OrdinalIgnoreCase));

                                    return new BlenderTextureData
                                    {
                                        Hash = t.ResourceFileHash.ToString(),
                                        Name = fileNameWithoutExtension,
                                        Path = finalPath,
                                        Slot = t.ShaderGenName
                                    };
                                })
                        };
                    }

                    return mesh;
                }))
        };

        var json = JsonConvert.SerializeObject(meshData);
        File.WriteAllText(outputPath, json);
    }

    private static IEnumerable<string> GetHighResTextures(string rootDirectory, Material material)
    {
        if (!string.IsNullOrWhiteSpace(material.HighTexturePackAsset))
        {
            var highTexturePackUri = material.HighTexturePackAsset.Replace(".htpk", ".autoext");

            for (var i = 0; i < 2; i++)
            {
                // Check for 4K pack for this material, will be handled by the following code if not present
                if (i > 0)
                {
                    highTexturePackUri = highTexturePackUri.Insert(highTexturePackUri.LastIndexOf('.'), "2");
                }

                var fileName = highTexturePackUri.Split('/').Last();
                var path = $"{rootDirectory}\\materials\\{fileName}";
                if (File.Exists(path))
                {
                    var highTexturePack = File.ReadAllBytes(path);
                    foreach (var highTexture in Encoding.UTF8.GetString(highTexturePack)
                                 .Split(' ')
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Select(s =>
                                 {
                                     var result = s.Trim();
                                     if (result.Last() == 0x00)
                                     {
                                         result = result[..^1];
                                     }

                                     return result;
                                 }))
                    {
                        yield return highTexture;
                    }
                }
            }
        }
    }
}