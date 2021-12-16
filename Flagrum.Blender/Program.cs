using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
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

        Gpubin meshData;

        if (inputPath.Contains("_$fcnd"))
        {
            meshData = new Gpubin
            {
                BoneTable = boneTable,
                Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                    .Where(m => m.LodNear == 0)
                    .Select(m => new GpubinMesh
                    {
                        Name = m.Name,
                        FaceIndices = m.FaceIndices,
                        VertexPositions = m.VertexPositions,
                        ColorMaps = m.ColorMaps,
                        Normals = m.Normals,
                        Tangents = m.Tangents,
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
                    }))
            };
        }
        else
        {
            meshData = new Gpubin
            {
                BoneTable = boneTable,
                Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                    .Where(m => m.LodNear == 0)
                    .Select(m => new GpubinMesh
                    {
                        Name = m.Name,
                        FaceIndices = m.FaceIndices,
                        VertexPositions = m.VertexPositions,
                        ColorMaps = m.ColorMaps,
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
                    }))
            };
        }

        var json = JsonConvert.SerializeObject(meshData);
        File.WriteAllText(outputPath, json);
    }
}