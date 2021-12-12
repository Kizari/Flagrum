using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Console.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmdl;

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

        var gfx =
            "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.gmdl.gfxbin";
        var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");

        var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        var model = reader.Read();
        var builder = new StringBuilder();
        builder.AppendLine("var dictionary = new Dictionary<ulong, string>");
        builder.AppendLine("{");
        foreach (var dependency in model.Header.Dependencies)
        {
            if (ulong.TryParse(dependency.PathHash, out var pathHash))
            {
                builder.Append("    {" + pathHash + ", \"" + dependency.Path + "\"}");
                if (dependency != model.Header.Dependencies.Last())
                {
                    builder.Append(",");
                }

                builder.Append("\n");
            }
        }

        builder.AppendLine("};");
        File.WriteAllText("C:\\Modding\\Dependencies.cs", builder.ToString());

        // var unpacker = new Unpacker("C:\\Modding\\ardyn_gladio.ffxvbinmod");
        // var packer = unpacker.ToPacker();
        // // var material =
        // //     File.ReadAllBytes(
        // //         "C:\\Testing\\ModelReplacements\\Extract3\\mod\\gladiolus_ardyn\\ardynmankini.fbxgmtl\\chest.gmtl.gfxbin");
        // //
        // // packer.UpdateFile("chest.gmtl", material);
        // packer.WriteToFile("C:\\Modding\\0e828ff5-d9dd-4b9b-b6f5-c26a1ed6ad43.ffxvbinmod");

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