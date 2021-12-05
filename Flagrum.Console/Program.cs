using System.IO;
using System.Linq;
using Flagrum.Console.Utilities;
using Flagrum.Gfxbin.Gmdl;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
        GfxbinToBoneDictionary.Run("C:\\Testing\\character\\nh\\nh03\\model_000\\nh03_000.gmdl.gfxbin");
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