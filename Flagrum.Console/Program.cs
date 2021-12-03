using System.IO;
using Flagrum.Console.Utilities;
using Flagrum.Gfxbin.Gmdl;
using Newtonsoft.Json;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
        var gfxbinData =
            File.ReadAllBytes(
                "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh02\\model_000\\nh02_000.gmdl.gfxbin");
        var gpubinData =
            File.ReadAllBytes(
                "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh02\\model_000\\nh02_000.gpubin");

        var reader = new ModelReader(gfxbinData, gpubinData);
        var model = reader.Read();

        return;
        MaterialToPython.Convert("C:\\Users\\Kieran\\Downloads\\nh02_000_skin_01_mat.gmtl.gfxbin",
            "C:\\Testing\\NewHumanSkin.py");
        return;
        //var reader = new MaterialReader("C:\\Users\\Kieran\\Downloads\\nh02_000_skin_01_mat.gmtl.gfxbin");
        var material = reader.Read();
        var json = JsonConvert.SerializeObject(material);
        File.WriteAllText("C:\\Testing\\NewSkinTemplate.json", json);
        //BtexConverter.Convert("C:\\Testing\\Previews\\preview.png",
        //    "C:\\Testing\\Previews\\preview.btex", BtexConverter.TextureType.Color);
    }
}