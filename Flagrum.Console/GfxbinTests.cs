using System.IO;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Console;

public static class GfxbinTests
{
    public static void ImportThenExportModel()
    {
        const string gfxbin = "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh02\\model_000\\nh02_000.gmdl.gfxbin";
        const string gpubin = "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh02\\model_000\\nh02_000.gpubin";
        //const string gfxbin = "C:\\Testing\\Gfxbin\\original.gmdl.gfxbin";
        //const string gpubin = "C:\\Testing\\Gfxbin\\original.gpubin";
        const string output = "C:\\Testing\\Gfxbin\\export.gmdl.gfxbin";

        var gfxbinBuffer = File.ReadAllBytes(gfxbin);
        var gpubinBuffer = File.ReadAllBytes(gpubin);

        var reader = new ModelReader(gfxbinBuffer, gpubinBuffer);
        var model = reader.Read();

        var writer = new ModelWriter(model, gpubinBuffer);
        var data = writer.Write();
        File.WriteAllBytes(output, data);
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