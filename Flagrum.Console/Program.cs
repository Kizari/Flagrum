using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Console.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Core.Utilities;
using Newtonsoft.Json;

namespace Flagrum.Console;

public class Program
{
    private static void EnumerateFilesRecursively(string directory, List<(string path, string uri)> paths)
    {
        foreach (var subDirectory in Directory.EnumerateDirectories(directory))
        {
            EnumerateFilesRecursively(subDirectory, paths);
        }

        paths.AddRange(Directory.EnumerateFiles(directory).Select(f =>
        {
            var result = f.Replace("C:\\Testing\\ModelReplacements\\mo-sword\\", "data://").Replace('\\', '/');
            return (f, result.Contains("/wetness/") ? result.Replace(".btex", ".tga") : result.Replace(".btex", ".tif"));
        }));
    }
    
    public static void Main(string[] args)
    {
        var gfx = "C:\\Modding\\WeaponTesting\\mod\\24d5d6ab-e8f4-443f-a5e1-a8830aff7924\\angery_sword.gmdl.gfxbin";
        var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
        var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        var model = reader.Read();
        bool x = true;

        //GfxbinTests.BuildMod2();
        // var gfx = "C:\\Modding\\Extractions\\angery_sword\\mod\\0e664ae0-1baa-4f96-8518-ad16d11d1141\\angery_sword.gmdl.gfxbin";
        // var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
        // var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        // var model = reader.Read();
        // bool x = true;

        // var ebex = "C:\\Testing\\ModelReplacements\\SinglePlayerSword\\temp.ebex";
        // var previewBtex =
        //     "C:\\Testing\\ModelReplacements\\SwordExtract\\mod\\3b7e2ca5-1a23-4afb-af38-d5726c190841\\$preview.btex";
        // var previewPng =
        //     "C:\\Testing\\ModelReplacements\\SwordExtract\\mod\\3b7e2ca5-1a23-4afb-af38-d5726c190841\\$preview.png.bin";
        // var modmeta = "C:\\Testing\\ModelReplacements\\SwordExtract\\mod\\3b7e2ca5-1a23-4afb-af38-d5726c190841\\index.modmeta";
        // var unpacker = new Unpacker("C:\\Modding\\Extractions\\7dc925b8-aa5c-4a17-a20f-1d5ba9921f36.earc");
        // var packer = unpacker.ToPacker();
        //
        //
        // var shaderDirectory = "C:\\Testing\\ModelReplacements\\mo-sword\\shader";
        // var paths = new List<(string path, string uri)>();
        // EnumerateFilesRecursively(shaderDirectory, paths);
        // foreach (var (path, uri) in paths)
        // {
        //     packer.AddFile(File.ReadAllBytes(path), uri);
        // }
        //
        // var texturePath = "C:\\Testing\\ModelReplacements\\mo-sword\\mod\\sword_1\\khopesh_d.btex";
        // packer.AddFile(File.ReadAllBytes(texturePath), "data://mod/7dc925b8-aa5c-4a17-a20f-1d5ba9921f36/khopesh_d.png");
        //
        // var materialPath = "C:\\Testing\\ModelReplacements\\mo-sword\\mod\\sword_1\\khopesh.fbxgmtl\\material.gmtl.gfxbin";
        // var reader = new MaterialReader(materialPath);
        // var material = reader.Read();
        // material.Name = "body_ashape_mat";
        // material.NameHash = Cryptography.Hash32(material.Name);
        // var assetUri = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri");
        // var reference = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref");
        // assetUri.Path = $"data://mod/7dc925b8-aa5c-4a17-a20f-1d5ba9921f36/materials/";
        // reference.Path = $"data://mod/7dc925b8-aa5c-4a17-a20f-1d5ba9921f36/materials/body_ashape_mat.gmtl";
        // var texture = material.Textures.FirstOrDefault(t => t.Path.Contains("khopesh_d"));
        // texture.Path = $"data://mod/7dc925b8-aa5c-4a17-a20f-1d5ba9921f36/khopesh_d.png";
        // texture.PathHash = Cryptography.Hash32(texture.Path);
        // texture.ResourceFileHash = Cryptography.HashFileUri64(texture.Path);
        // var textureDependency = material.Header.Dependencies.FirstOrDefault(d => d.Path.Contains("khopesh_d.png"));
        // textureDependency.Path = texture.Path;
        // var index = material.Header.Hashes.IndexOf(ulong.Parse(textureDependency.PathHash));
        // textureDependency.PathHash = texture.ResourceFileHash.ToString();
        // material.Header.Hashes[index] = texture.ResourceFileHash;
        //
        // var writer = new MaterialWriter(material);
        // packer.UpdateFile("body_ashape_mat.gmtl", writer.Write());
        // // packer.UpdateFile("temp.ebex", File.ReadAllBytes(ebex));
        // // packer.UpdateFile("$preview.btex", File.ReadAllBytes(previewBtex));
        // // packer.UpdateFile("$preview.png.bin", File.ReadAllBytes(previewPng));
        // // packer.UpdateFile("index.modmeta", File.ReadAllBytes(modmeta));
        // packer.WriteToFile("C:\\Modding\\Extractions\\7dc925b8-aa5c-4a17-a20f-1d5ba9921f36.ffxvbinmod");


        // var reader =
        //     new MaterialReader(
        //         "C:\\Modding\\Extractions\\angery_sword\\mod\\96ec9564-657c-493a-8561-70f3989784b8\\materials\\body_ashape_mat.gmtl");
        //
        // var material = reader.Read();
        //
        // foreach (var dependency in material.Header.Dependencies)
        // {
        //     System.Console.WriteLine(dependency.Path);
        // }

        // var gfx = "C:\\Testing\\ModelReplacements\\mo-sword\\mod\\sword_1\\khopesh.gmdl.gfxbin";
        // var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
        // var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        // var model = reader.Read();
        // bool x = true;
        // var allIndices = model.MeshObjects[0].Meshes
        //     .SelectMany(m => m.WeightIndices.SelectMany(n => n.SelectMany(o => o.Select(p => p))))
        //     .Distinct();
        //bool x = true;
        // var templatePath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\Materials\\ENEMY_SKIN.json";
        // var json = File.ReadAllText(templatePath);
        // var material = JsonConvert.DeserializeObject<Material>(json);
        // bool x = true;

        //MaterialsToTemplates.Run();
        // var gfx = "C:\\Testing\\ModelReplacements\\mo-sword\\mod\\sword_1\\khopesh.gmdl.gfxbin";
        // var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
        // var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        // var model = reader.Read();
        // foreach (var node in model.NodeTable)
        // {
        //     System.Console.WriteLine(node.Name);
        //     for (int i = 0; i < node.Matrix.Rows.Count; i++)
        //     {
        //         System.Console.WriteLine($"{node.Matrix.Rows[i].X}, {node.Matrix.Rows[i].Y}, {node.Matrix.Rows[i].Z}");
        //     }
        // }
        //
        // return;

        // var gfx =
        //     "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.fbxgmtl\\chest.gmtl.gfxbin";
        //
        // var reader = new MaterialReader(gfx);
        // var material = reader.Read();
        // foreach (var texture in material.Textures)
        // {
        //     System.Console.WriteLine(texture.Name + ": " + texture.Path);
        // }
        //
        // System.Console.WriteLine(material.NameHash);
        // System.Console.WriteLine(material.Name);
        // System.Console.WriteLine(Cryptography.Hash32(material.Name));
        //
        // foreach (var texture in material.Textures)
        // {
        //     System.Console.WriteLine("\n");
        //     System.Console.WriteLine($"{texture.PathHash}: {texture.Path}");
        //     System.Console.WriteLine($"{Cryptography.Hash32(texture.Path)}: {texture.Path}");
        //     System.Console.WriteLine($"{texture.NameHash}: {texture.Name}");
        //     System.Console.WriteLine($"{Cryptography.Hash32(texture.Name)}: {texture.Name}");
        //     System.Console.WriteLine($"{texture.ResourceFileHash}");
        //     System.Console.WriteLine($"{Cryptography.HashFileUri64(texture.Path)}");
        //     System.Console.WriteLine($"{texture.ShaderGenNameHash}: {texture.ShaderGenName}");
        //     System.Console.WriteLine($"{Cryptography.Hash32(texture.ShaderGenName)}: {texture.ShaderGenName}");
        // }

        //var gfx = "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.fbxgmtl\\chest.gmtl.gfxbin";
        // var gfx =
        //      "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh00\\model_000\\materials\\nh00_000_skin_02_mat.gmtl.gfxbin";
        // var reader = new MaterialReader(gfx);
        // var material = reader.Read();
        //
        // var dependencies = new List<DependencyPath>();
        // dependencies.AddRange(material.ShaderBinaries.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath { Path = s.Path, PathHash = s.ResourceFileHash.ToString() }));
        // dependencies.AddRange(material.Textures.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath { Path = s.Path, PathHash = s.ResourceFileHash.ToString() }));
        // dependencies.Add(new DependencyPath { PathHash = "asset_uri", Path = $"data://character/nh/nh00/model_000/materials/"});
        // dependencies.Add(new DependencyPath { PathHash = "ref", Path = $"data://character/nh/nh00/model_000/materials/nh00_000_skin_02_mat.gmtl"});
        // material.Header.Dependencies = dependencies.DistinctBy(d => d.PathHash).ToList();
        //
        // var writer = new MaterialWriter(material);
        // File.WriteAllBytes(
        //     "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh00\\model_000\\materials\\nh00_000_skin_02_mat_copy.gmtl.gfxbin",
        //     writer.Write());



        // var gfx =
        //     "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.gmdl.gfxbin";
        // var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
        //
        // var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        // var model = reader.Read();
        // foreach (var dependency in model.Header.Dependencies)
        // {
        //     if (dependency.PathHash != "ref" && dependency.PathHash != "asset_uri")
        //     {
        //         dependency.PathHash = Cryptography.HashFileUri64(dependency.Path).ToString();
        //     }
        // }
        // var writer = new ModelWriter(model);
        // var (gfxData, gpuData) = writer.Write();
        // File.WriteAllBytes("C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini_copy.gmdl.gfxbin", gfxData);
        // File.WriteAllBytes("C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini_copy.gpubin", gpuData);

        // foreach (var mesh in model.MeshObjects[0].Meshes)
        // {
        //     System.Console.WriteLine(mesh.DefaultMaterialHash);
        //     System.Console.WriteLine(Cryptography.HashFileUri64($"data://mod/gladiolus_ardyn/ardynmankini.fbxgmtl/{mesh.Name}.gmtl"));
        // }
        // var builder = new StringBuilder();
        // builder.AppendLine("var dictionary = new Dictionary<ulong, string>");
        // builder.AppendLine("{");
        // foreach (var dependency in model.Header.Dependencies)
        // {
        //     if (ulong.TryParse(dependency.PathHash, out var pathHash))
        //     {
        //         builder.Append("    {" + pathHash + ", \"" + dependency.Path + "\"}");
        //         if (dependency != model.Header.Dependencies.Last())
        //         {
        //             builder.Append(",");
        //         }
        //
        //         builder.Append("\n");
        //     }
        // }
        //
        // builder.AppendLine("};");
        // File.WriteAllText("C:\\Modding\\Dependencies.cs", builder.ToString());



        //ModelReplacementTableToCs.Run();
        //GfxbinTests.GetBoneTable();
        //GfxbinTests.CheckMaterialDefaults();
        //var gfxbin = "C:\\Testing\\character\\nh\\nh01\\model_000\\materials\\nh01_000_skin_00_mat.gmtl.gfxbin";
        //var materialReader = new MaterialReader(gfxbin);
        //var material = materialReader.Read();

        // GfxbinToBoneDictionary.Run("C:\\Testing\\character\\nh\\nh03\\model_000\\nh03_000.gmdl.gfxbin");
        // var gfxbinData =
        //     File.ReadAllBytes(
        //         "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gmdl.gfxbin");
        // var gpubinData =
        //     File.ReadAllBytes(
        //         "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gpubin");
        //
        // var reader = new ModelReader(gfxbinData, gpubinData);
        // var model = reader.Read();
        //
        // var mesh = model.MeshObjects[0].Meshes.FirstOrDefault(m => m.Name == "bodyShape" && m.LodNear == 0);
        // var weights = mesh.WeightValues[1].Where(w => w[0] > 0).Select(w => mesh.WeightValues[1].IndexOf(w)).Take(10);
        //
        // foreach (var i in weights)
        // {
        //     var weight1 = mesh.WeightValues[0][i];
        //     var weight2 = mesh.WeightValues[1][i];
        //
        //     System.Console.WriteLine($"[{weight1[0]}, {weight1[1]}, {weight1[2]}, {weight1[3]}]");
        //     System.Console.WriteLine($"[{weight2[0]}, {weight2[1]}, {weight2[2]}, {weight2[3]}]");
        //     System.Console.WriteLine(
        //         $"{weight1.Sum(w => w)} + {weight2.Sum(w => w)} = {weight1.Sum(w => w) + weight2.Sum(w => w)}\n\n");
        // }

        // return;
        // MaterialToPython.Convert("C:\\Users\\Kieran\\Downloads\\nh02_000_skin_01_mat.gmtl.gfxbin",
        //     "C:\\Testing\\NewHumanSkin.py");
        // return;
        // //var reader = new MaterialReader("C:\\Users\\Kieran\\Downloads\\nh02_000_skin_01_mat.gmtl.gfxbin");
        // var material = reader.Read();
        // var json = JsonConvert.SerializeObject(material);
        // File.WriteAllText("C:\\Testing\\NewSkinTemplate.json", json);
        //BtexConverter.Convert("C:\\Testing\\Previews\\preview.png",
        //    "C:\\Testing\\Previews\\preview.btex", BtexConverter.TextureType.Color);
    }
}