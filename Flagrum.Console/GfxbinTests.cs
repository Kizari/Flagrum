using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Constructs;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Console;

public static class GfxbinTests
{
    public static void Compare()
    {
        var moGfxbin = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gmdl.gfxbin";
        var moGpubin = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gpubin";
        //var moGfxbin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
        //var moGpubin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gpubin";
        var gfxbin = "C:\\Testing\\Gfxbin\\untitled5.gmdl.gfxbin";
        var gpubin = "C:\\Testing\\Gfxbin\\untitled5.gpubin";

        var reader = new ModelReader(File.ReadAllBytes(moGfxbin), File.ReadAllBytes(moGpubin));
        var mo = reader.Read();
        reader = new ModelReader(File.ReadAllBytes(gfxbin), File.ReadAllBytes(gpubin));
        var model = reader.Read();

        System.Console.WriteLine($"Mesh count: {mo.MeshObjects[0].Meshes.Count}, {model.MeshObjects[0].Meshes.Count}");
        for (var i = 0; i < mo.MeshObjects[0].Meshes.Count; i++)
        {
            var moMesh = mo.MeshObjects[0].Meshes[i];
            var mesh = model.MeshObjects[0].Meshes[i];

            System.Console.WriteLine($"Mesh name: {moMesh.Name}, {mesh.Name}");
            // foreach (var stream in moMesh.VertexStreamDescriptions)
            // {
            //     System.Console.WriteLine($"Slot: {stream.Slot}");
            //     System.Console.WriteLine($"Stride: {stream.Stride}");
            //     System.Console.WriteLine($"Type: {stream.Type}");
            //
            //     foreach (var desc in stream.VertexElementDescriptions)
            //     {
            //         System.Console.WriteLine($"\tSemantic: {desc.Semantic}");
            //         System.Console.WriteLine($"\tFormat: {desc.Format}");
            //         System.Console.WriteLine($"\tOffset: {desc.Offset}");
            //     }
            // }

            // System.Console.WriteLine($"Vertex count: {moMesh.VertexCount}, {mesh.VertexCount}");
            // System.Console.WriteLine($"Face index count: {moMesh.FaceIndices.Length}, {mesh.FaceIndices.Length}");

            // for (var j = 0; j < Math.Max(moMesh.FaceIndices.Length, mesh.FaceIndices.Length); j++)
            // {
            //     var moFaces = new[] {moMesh.FaceIndices[j, 0], moMesh.FaceIndices[j, 1], moMesh.FaceIndices[j, 2]};
            //     var faces = new[] {mesh.FaceIndices[j, 0], mesh.FaceIndices[j, 1], mesh.FaceIndices[j, 2]};
            //     System.Console.WriteLine($"MO: [{moFaces[0]}, {moFaces[1]}, {moFaces[2]}] O: [{faces[0]}, {faces[1]}, {faces[2]}]");
            // }

            for (var j = 0; j < moMesh.WeightValues[0].Count; j++)
            {
                var map1 = moMesh.WeightValues[0][j];
                var map2 = moMesh.WeightValues[1][j];
                var sum = map1.Sum(s => s) + map2.Sum(s => s);
                System.Console.WriteLine(
                    $"[{map1[0]}, {map1[1]}, {map1[2]}, {map1[3]}], [{map2[0]}, {map2[1]}, {map2[2]}, {map2[3]}], Sum: {sum}");
            }

            // for (var j = 0; j < Math.Max(moMesh.WeightIndices[0].Count, mesh.WeightIndices[0].Count); j++)
            // {
            //     var moWeight = moMesh.WeightValues[0][j];
            //     var moIndex = moMesh.WeightIndices[0][j];
            //     var weight = mesh.WeightValues[0][j];
            //     var index = mesh.WeightIndices[0][j];
            //     
            //     System.Console.WriteLine($"Indices: [{moIndex[0]}, {moIndex[1]}, {moIndex[2]}, {moIndex[3]}], [{index[0]}, {index[1]}, {index[2]}, {index[3]}]");
            //     System.Console.WriteLine($"Weights: [{moWeight[0]}, {moWeight[1]}, {moWeight[2]}, {moWeight[3]}], [{weight[0]}, {weight[1]}, {weight[2]}, {weight[3]}]");
            // }
        }
    }

    public static void Import()
    {
        var gfxbinPath = "C:\\Testing\\Gfxbin\\magiccube.gmdl.gfxbin";
        var gpubinPath = gfxbinPath.Replace(".gmdl.gfxbin", ".gpubin");
        var gfxbin = File.ReadAllBytes(gfxbinPath);
        var gpubin = File.ReadAllBytes(gpubinPath);
        var reader = new ModelReader(gfxbin, gpubin);
        var model = reader.Read();

        Dictionary<int, string> boneTable;
        if (model.BoneHeaders.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
        {
            // Probably a broken MO gfxbin with all IDs set to this value, let Blender handle it
            boneTable = new Dictionary<int, string>();
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
    }

    public static void Export2()
    {
        const string jsonPath = "C:\\Testing\\Gfxbin\\mod\\magic_cube\\magiccube.gmdl.gfxbin.json";
        var json = File.ReadAllText(jsonPath);
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

        var exportPath = "C:\\Testing\\Gfxbin\\mod\\magic_cube\\magiccube.gmdl.gfxbin";
        const string gfx = "C:\\Testing\\Gfxbin\\magiccube.gmdl.gfxbin";
        const string gpu = "C:\\Testing\\Gfxbin\\magiccube.gpubin";

        var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        var model = reader.Read();

        // Replace the mesh data in the template mod with the export mesh data
        var replacer = new ModelReplacer(model, gpubin);
        model = replacer.Replace();

        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        File.WriteAllBytes(exportPath, gfxData);
        File.WriteAllBytes(exportPath.Replace(".gmdl.gfxbin", ".gpubin"), gpuData);
    }

    public static void Export1()
    {
        const string jsonPath = "C:\\Testing\\Gfxbin\\mod\\noctis_custom\\clean.gmdl.gfxbin.json";
        var json = File.ReadAllText(jsonPath);
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

        var exportPath = "C:\\Testing\\Gfxbin\\mod\\noctis_custom\\clean.gmdl.gfxbin";
        const string gfx = "C:\\Testing\\Gfxbin\\clean.gmdl.gfxbin";
        const string gpu = "C:\\Testing\\Gfxbin\\clean.gpubin";

        var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        var model = reader.Read();

        // Replace the mesh data in the template mod with the export mesh data
        var replacer = new ModelReplacer(model, gpubin);
        model = replacer.Replace();

        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        File.WriteAllBytes(exportPath, gfxData);
        File.WriteAllBytes(exportPath.Replace(".gmdl.gfxbin", ".gpubin"), gpuData);
    }

    public static void Export()
    {
        const string jsonPath = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin.json";
        var json = File.ReadAllText(jsonPath);
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

        var exportPath = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
        const string gfx = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gmdl.gfxbin";
        const string gpu = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gpubin";

        var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        var model = reader.Read();

        // Replace the mesh data in the template mod with the export mesh data
        var replacer = new ModelReplacer(model, gpubin);
        model = replacer.Replace();

        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        File.WriteAllBytes(exportPath, gfxData);
        File.WriteAllBytes(exportPath.Replace(".gmdl.gfxbin", ".gpubin"), gpuData);
    }

    public static void PrintData()
    {
        const string gfxbin = "C:\\Testing\\Gfxbin\\mo-export.gmdl.gfxbin";
        const string gpubin = "C:\\Testing\\Gfxbin\\mo-export.gpubin";
        var gfxbinBuffer = File.ReadAllBytes(gfxbin);
        var gpubinBuffer = File.ReadAllBytes(gpubin);
        const string gfx = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
        const string gpu = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gpubin";
        var gfxbinBuffer2 = File.ReadAllBytes(gfx);
        var gpubinBuffer2 = File.ReadAllBytes(gpu);

        var reader = new ModelReader(gfxbinBuffer, gpubinBuffer);
        var model = reader.Read();
        reader = new ModelReader(gfxbinBuffer2, gpubinBuffer2);
        var model2 = reader.Read();

        System.Console.WriteLine($"MO Count: {model.BoneHeaders.Count}");
        System.Console.WriteLine($"Original Count: {model2.BoneHeaders.Count}");

        foreach (var bone in model.BoneHeaders)
        {
            var match = model2.BoneHeaders.FirstOrDefault(b => b.Name == bone.Name);
            if (match == null)
            {
                System.Console.WriteLine($"No match for {bone.Name}!");
            }
            else if (match.LodIndex != bone.LodIndex)
            {
                System.Console.WriteLine($"{bone.Name}: {bone.LodIndex >> 16}, {match.LodIndex >> 16}");
            }
        }
    }

    public static void ImportThenExportModel()
    {
        const string gfxbin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gmdl.gfxbin";
        const string gpubin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gpubin";
        //const string gfxbin = "C:\\Testing\\Gfxbin\\original.gmdl.gfxbin";
        //const string gpubin = "C:\\Testing\\Gfxbin\\original.gpubin";
        const string outGfxbin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
        const string outGpubin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gpubin";

        var gfxbinBuffer = File.ReadAllBytes(gfxbin);
        var gpubinBuffer = File.ReadAllBytes(gpubin);

        var reader = new ModelReader(gfxbinBuffer, gpubinBuffer);
        var model = reader.Read();

        var writer = new ModelWriter(model);
        var (gfxbinData, gpubinData) = writer.Write();
        File.WriteAllBytes(outGfxbin, gfxbinData);
        File.WriteAllBytes(outGpubin, gpubinData);
    }

    public static void ImportThenExportMaterial()
    {
        var input = "C:\\Testing\\Gfxbin\\material.gmtl.gfxbin";
        var output = "C:\\Testing\\Gfxbin\\export.gmtl.gfxbin";

        var reader = new MaterialReader(input);
        var material = reader.Read();

        var writer = new MaterialWriter(material);
        var data = writer.Write();

        File.WriteAllBytes(output, data);
    }

    public static void ReadMaterialToJson()
    {
        var input = "C:\\Testing\\Gfxbin\\material.gmtl.gfxbin";
        var output = "C:\\Testing\\Gfxbin\\material.json";

        var reader = new MaterialReader(input);
        var material = reader.Read();

        File.WriteAllText(output, JsonConvert.SerializeObject(material, Formatting.Indented));
    }
}