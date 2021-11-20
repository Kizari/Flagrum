using System.IO;
using System.Linq;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Constructs;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Console;

public static class GfxbinTests
{
    public static void Export()
    {
        const string jsonPath = "C:\\Testing\\Gfxbin\\untitled5.gmdl.gfxbin.json";
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
        //const string gfxbin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gmdl.gfxbin";
        //const string gpubin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gpubin";
        const string gfxbin = "C:\\Testing\\Gfxbin\\original.gmdl.gfxbin";
        const string gpubin = "C:\\Testing\\Gfxbin\\original.gpubin";
        var gfxbinBuffer = File.ReadAllBytes(gfxbin);
        var gpubinBuffer = File.ReadAllBytes(gpubin);

        var reader = new ModelReader(gfxbinBuffer, gpubinBuffer);
        var model = reader.Read();

        foreach (var bone in model.BoneHeaders.OrderBy(b => b.UniqueIndex))
        {
            System.Console.WriteLine($"[{bone.LodIndex}] {bone.Name}");
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