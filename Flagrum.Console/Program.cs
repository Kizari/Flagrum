using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Console.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Utilities;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
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
        
        // var gfx = "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.fbxgmtl\\chest.gmtl.gfxbin";
        // var reader = new MaterialReader(gfx);
        // var material = reader.Read();
        //
        // foreach (var dependency in material.Header.Dependencies)
        // {
        //     if (dependency.PathHash != "ref" && dependency.PathHash != "asset_uri")
        //     {
        //         dependency.PathHash = Cryptography.HashFileUri64(dependency.Path).ToString();
        //     }
        // }
        //
        // var writer = new MaterialWriter(material);
        // File.WriteAllBytes(
        //     "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.fbxgmtl\\chest_copy.gmtl.gfxbin",
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

        var unpacker = new Unpacker("C:\\Modding\\ardyn_gladio.earc");
        var packer = unpacker.ToPacker();
        // var material =
        //     File.ReadAllBytes(
        //         "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.fbxgmtl\\chest.gmtl.gfxbin");
        //
        // packer.UpdateFile("chest.gmtl", material);
        packer.WriteToFile("C:\\Modding\\0e828ff5-d9dd-4b9b-b6f5-c26a1ed6ad43.earc");

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