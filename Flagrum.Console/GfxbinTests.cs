using System.IO;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Console;

public static class GfxbinTests
{
    public static void PrintOffsets()
    {
        const string gfxbin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
        const string gpubin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gpubin";
        var gfxbinBuffer = File.ReadAllBytes(gfxbin);
        var gpubinBuffer = File.ReadAllBytes(gpubin);

        var reader = new ModelReader(gfxbinBuffer, gpubinBuffer);
        var model = reader.Read();

        foreach (var meshObject in model.MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                System.Console.WriteLine("\n" + mesh.Name);
                System.Console.WriteLine("Draw priority offset: " + mesh.DrawPriorityOffset);
                System.Console.WriteLine("Face indices buffer offset: " + mesh.FaceIndicesBufferOffset);
                System.Console.WriteLine("Face indices buffer size: " + mesh.FaceIndicesBufferSize);
                System.Console.WriteLine("Vertex buffer offset: " + mesh.VertexBufferOffset);
                System.Console.WriteLine("Vertex buffer size: " + mesh.VertexBufferSize);
            }
        }
    }

    public static void ImportThenExportModel()
    {
        const string gfxbin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
        const string gpubin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gpubin";
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