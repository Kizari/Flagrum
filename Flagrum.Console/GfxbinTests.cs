using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmtl;

namespace Flagrum.Console;

public static class GfxbinTests
{
    public static void GetBoneTable()
    {
        var gfx = "C:\\Testing\\Exineris\\mod\\b376d00b-e6ae-497d-a004-485914158a9b\\ignis.gmdl.gfxbin";
        var gpu = "C:\\Testing\\Exineris\\mod\\b376d00b-e6ae-497d-a004-485914158a9b\\ignis.gpubin";

        var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        var model = reader.Read();

        foreach (var bone in model.BoneHeaders)
        {
            System.Console.WriteLine($"{bone.Name}: {bone.LodIndex >> 16}");
        }
    }

    public static void CheckMaterialDefaults()
    {
        //var path =
            //C:\\Testing\\character\\nh\\nh01\\model_000\\materials\\nh01_000_skin_00_mat.gmtl.gfxbin";
        var path = "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh02\\model_000\\materials\\nh02_000_eye_00_mat.gmtl.gfxbin";
        var reader = new MaterialReader(path);
        var material = reader.Read();

        // var builder = new StringBuilder();
        // foreach (var input in material.InterfaceInputs.Where(i => i.InterfaceIndex == 0))
        // {
        //     builder.AppendLine($"{input.ShaderGenName}: {string.Join(", ", input.Values)}");
        // }

        var builder = new StringBuilder();
        foreach (var texture in material.Textures.Where(t => !t.Path.EndsWith(".sb")))
        {
            builder.AppendLine($"{texture.Name}\n{texture.Path}\n\n");
        }

        File.WriteAllText("C:\\Testing\\nh02_000_eye_00_mat_texture_defaults.txt", builder.ToString());
    }

    public static void Compare()
    {
        var gfxbin = "C:\\Testing\\extract\\mod\\7594c633-8944-4542-bc12-627b444fdcc4\\prom_shirtless.gmdl.gfxbin";
        var gpubin = "C:\\Testing\\extract\\mod\\7594c633-8944-4542-bc12-627b444fdcc4\\prom_shirtless.gpubin";
        //var gfxbin = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gmdl.gfxbin";
        //var gpubin = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gpubin";
        //var moGfxbin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
        //var moGpubin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gpubin";
        //var moGfxbin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gmdl.gfxbin";
        //var moGpubin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gpubin";

        //var reader = new ModelReader(File.ReadAllBytes(moGfxbin), File.ReadAllBytes(moGpubin));
        //var mo = reader.Read();
        var reader = new ModelReader(File.ReadAllBytes(gfxbin), File.ReadAllBytes(gpubin));
        var model = reader.Read();

        // foreach (var dependency in model.Header.Dependencies)
        // {
        //     System.Console.WriteLine($"{dependency.PathHash} - {dependency.Path}");
        // }
        //
        // System.Console.WriteLine($"\n{model.AssetHash}");

        System.Console.WriteLine($"[OG Asset] AABB-min: {model.Aabb.Min.X}, {model.Aabb.Min.Y}, {model.Aabb.Min.Z}");
        // System.Console.WriteLine($"[MO Asset] AABB-min: {mo.Aabb.Min.X}, {mo.Aabb.Min.Y}, {mo.Aabb.Min.Z}");
        //
        System.Console.WriteLine($"[OG Asset] AABB-max: {model.Aabb.Max.X}, {model.Aabb.Max.Y}, {model.Aabb.Max.Z}");
        // System.Console.WriteLine($"[MO Asset] AABB-max: {mo.Aabb.Max.X}, {mo.Aabb.Max.Y}, {mo.Aabb.Max.Z}\n");
        //
        System.Console.WriteLine($"[OG Asset] Name: {model.Name}");
        // System.Console.WriteLine($"[MO Asset] Name: {mo.Name}\n");
        //
        System.Console.WriteLine($"[OG Asset] Unknown1: {model.Unknown1}");
        //System.Console.WriteLine($"[MO Asset] Unknown1: {mo.Unknown1}\n");
        //
        System.Console.WriteLine($"[OG Asset] Asset Hash: {model.AssetHash}");
        // System.Console.WriteLine($"[MO Asset] Asset Hash: {mo.AssetHash}\n");
        //
        System.Console.WriteLine($"[OG Asset] Child Class Format: {model.ChildClassFormat}");
        // System.Console.WriteLine($"[MO Asset] Child Class Format: {mo.ChildClassFormat}\n");
        //
        System.Console.WriteLine($"[OG Asset] Instance Name Format: {model.InstanceNameFormat}");
        // System.Console.WriteLine($"[MO Asset] Instance Name Format: {mo.InstanceNameFormat}\n");
        //
        System.Console.WriteLine($"[OG Asset] Shader Class Format: {model.ShaderClassFormat}");
        // System.Console.WriteLine($"[MO Asset] Shader Class Format: {mo.ShaderClassFormat}\n");
        //
        System.Console.WriteLine($"[OG Asset] Shader Parameter List Format: {model.ShaderParameterListFormat}");
        // System.Console.WriteLine($"[MO Asset] Shader Parameter List Format: {mo.ShaderParameterListFormat}\n");
        //
        System.Console.WriteLine(
            $"[OG Asset] Shader Sampler Description Format: {model.ShaderSamplerDescriptionFormat}");
        // System.Console.WriteLine(
        //     $"[MO Asset] Shader Sampler Description Format: {mo.ShaderSamplerDescriptionFormat}\n");
        //
        System.Console.WriteLine($"[OG Asset] Has PSD Path: {model.HasPsdPath}");
        // System.Console.WriteLine($"[MO Asset] Has PSD Path: {mo.HasPsdPath}\n");
        //
        System.Console.WriteLine($"[OG Asset] PSD Path Hash: {model.PsdPathHash}");
        // System.Console.WriteLine($"[MO Asset] PSD Path Hash: {mo.PsdPathHash}\n");

        foreach (var node in model.NodeTable)
        {
            System.Console.WriteLine($"Node name: {node.Name}");
            System.Console.WriteLine($"{node.Matrix.Rows[0].X}, {node.Matrix.Rows[0].Y}, {node.Matrix.Rows[0].Z}");
            System.Console.WriteLine($"{node.Matrix.Rows[1].X}, {node.Matrix.Rows[1].Y}, {node.Matrix.Rows[1].Z}");
            System.Console.WriteLine($"{node.Matrix.Rows[2].X}, {node.Matrix.Rows[2].Y}, {node.Matrix.Rows[2].Z}");
            System.Console.WriteLine($"{node.Matrix.Rows[3].X}, {node.Matrix.Rows[3].Y}, {node.Matrix.Rows[3].Z}");
        }

        foreach (var meshObject in model.MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                System.Console.WriteLine($"{mesh.Name}");

                // foreach (var geometry in mesh.SubGeometries)
                // {
                //     System.Console.WriteLine($"AABB: {geometry.Aabb.Min}, {geometry.Aabb.Max}");
                //     System.Console.WriteLine($"Draw order: {geometry.DrawOrder}");
                //     System.Console.WriteLine($"Primitive count: {geometry.PrimitiveCount}");
                //     System.Console.WriteLine($"Start index: {geometry.StartIndex}");
                //     System.Console.WriteLine($"Cluster index bit flag: {geometry.ClusterIndexBitFlag}");
                // }

                System.Console.WriteLine($"AABB Min: {mesh.Aabb.Min.X}, {mesh.Aabb.Min.Y}, {mesh.Aabb.Min.Z}");
                System.Console.WriteLine($"AABB Max: {mesh.Aabb.Max.X}, {mesh.Aabb.Max.Y}, {mesh.Aabb.Max.Z}");

                System.Console.WriteLine($"Flag: {mesh.Flag}");
                System.Console.WriteLine($"Flags: {mesh.Flags}");
                System.Console.WriteLine($"Instance Number: {mesh.InstanceNumber}");
                System.Console.WriteLine($"LodNear: {mesh.LodNear}");
                System.Console.WriteLine($"LodFar: {mesh.LodFar}");
                System.Console.WriteLine($"LodFade: {mesh.LodFade}");
                System.Console.WriteLine($"Breakable bone index: {mesh.BreakableBoneIndex}");
                System.Console.WriteLine($"Default Material Hash: {mesh.DefaultMaterialHash}");
                System.Console.WriteLine($"Draw priority offset: {mesh.DrawPriorityOffset}");
                System.Console.WriteLine($"Vertex layout type: {mesh.VertexLayoutType}");
                System.Console.WriteLine($"Low Lod Shadow Cascade No: {mesh.LowLodShadowCascadeNo}");
                System.Console.WriteLine($"Is Oriented BB: {mesh.IsOrientedBB}");
                System.Console.WriteLine($"Primitive Type: {mesh.PrimitiveType}");
                System.Console.WriteLine($"Unknown1: {mesh.Unknown1}");
                System.Console.WriteLine($"Unknown2: {mesh.Unknown2}");
                System.Console.WriteLine($"Unknown3: {mesh.Unknown3}");
                System.Console.WriteLine($"Unknown4: {mesh.Unknown4}");
                System.Console.WriteLine($"Unknown5: {mesh.Unknown5}");
                System.Console.WriteLine($"Unknown6: {mesh.Unknown6}");

                System.Console.WriteLine(
                    $"OBB: {mesh.OrientedBB.XHalfExtent}, {mesh.OrientedBB.YHalfExtent}, {mesh.OrientedBB.ZHalfExtent}, {mesh.OrientedBB.Center}");
            }
        }
        //
        // foreach (var part in model.Parts)
        // {
        //     System.Console.WriteLine($"[OG Asset] Flags: {part.Flags}");
        //     System.Console.WriteLine($"[OG Asset] Id: {part.Id}");
        //     System.Console.WriteLine($"[OG Asset] Name: {part.Name}");
        //     System.Console.WriteLine($"[OG Asset] Unknown: {part.Unknown}\n");
        // }
        //
        // foreach (var part in mo.Parts)
        // {
        //     System.Console.WriteLine($"[MO Asset] Flags: {part.Flags}");
        //     System.Console.WriteLine($"[MO Asset] Id: {part.Id}");
        //     System.Console.WriteLine($"[MO Asset] Name: {part.Name}");
        //     System.Console.WriteLine($"[MO Asset] Unknown: {part.Unknown}\n");
        // }

        System.Console.WriteLine(
            $"Mesh count: {model.MeshObjects[0].Meshes.Count}, {model.MeshObjects[0].Meshes.Count}");
        for (var i = 0; i < model.MeshObjects[0].Meshes.Count; i++)
        {
            //var moMesh = mo.MeshObjects[0].Meshes[i];
            // var mesh = model.MeshObjects[0].Meshes[i];
            //
            // System.Console.WriteLine($"Mesh name: {mesh.Name}");
            // foreach (var stream in mesh.VertexStreamDescriptions)
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

            // for (var j = 0; j < moMesh.WeightValues[0].Count; j++)
            // {
            //     var map1 = moMesh.WeightValues[0][j];
            //     var map2 = moMesh.WeightValues[1][j];
            //     var sum = map1.Sum(s => s) + map2.Sum(s => s);
            //     System.Console.WriteLine(
            //         $"[{map1[0]}, {map1[1]}, {map1[2]}, {map1[3]}], [{map2[0]}, {map2[1]}, {map2[2]}, {map2[3]}], Sum: {sum}");
            // }

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
}