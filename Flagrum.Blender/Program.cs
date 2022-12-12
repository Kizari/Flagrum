using System.Text;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Newtonsoft.Json;

namespace Flagrum.Blender;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            args = new[]
            {
                "import", "", @"C:\Modding\datas\character\uw\uw04\model_401\uw04_401.gmdl.gfxbin", "",
                @"C:\Modding\datas\test.json"
            };
        }
        
        var command = args[0];
        var parameterInput = args[1];
        var inputPath = args[2];
        var parameterOutput = args[3];
        var outputPath = args[4];

        switch (command)
        {
            case "import":
                Import(inputPath, outputPath);
                break;
            case "material":
                Material(inputPath, outputPath);
                break;
        }
    }

    private static void Material(string inputPath, string outputPath)
    {
        var reader = new MaterialReader(inputPath);
        var material = reader.Read();

        var result = material.InterfaceInputs
            .Where(i => i.InterfaceIndex == 0)
            .ToDictionary(i => i.ShaderGenName, i => i.Values);

        var json = JsonConvert.SerializeObject(result);
        File.WriteAllText(outputPath, json);
    }

    private static void Import(string inputPath, string outputPath)
    {
        var importer = new Importer(inputPath);

        // Write the temp file for Blender to access
        var json = JsonConvert.SerializeObject(importer.GetData());
        File.WriteAllText(outputPath, json);
    }
}