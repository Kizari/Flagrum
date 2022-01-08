using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Console.Utilities;

public class MaterialFinder
{
    private const string DataDirectory =
        @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\character";

    private ConcurrentBag<(string, string)> _matches;

    public void MakeTemplate()
    {
        // Get Iggy glasses material from the game files
        const string iggyPath = $"{DataDirectory}\\nh\\nh03\\model_000\\materials\\autoexternal.earc";
        using var iggyUnpacker = new Unpacker(iggyPath);
        var iggyBytes = iggyUnpacker.UnpackFileByQuery("nh03_000_basic_01_mat.gmtl", out _);
        var glass = new MaterialReader(iggyBytes).Read();

        const string path =
            @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\character\nh\nh03\model_304\materials\autoexternal.earc";

        // Shiva ribbon
        //const string path = $"{DataDirectory}\\sm\\sm03\\model_000\\materials\\autoexternal.earc";

        using var unpacker = new Unpacker(path);
        var materialBytes = unpacker.UnpackFileByQuery("nh03_304_basic_01_mat.gmtl", out _);
        var material = new MaterialReader(materialBytes).Read();

        // Overwrite material input values with glass input values
        foreach (var input in glass.InterfaceInputs)
        {
            var match = material.InterfaceInputs.FirstOrDefault(i => i.Name == input.Name);
            if (match == null)
            {
                System.Console.WriteLine($"No match found for input {input.ShaderGenName}");
            }
            else
            {
                match.Values = input.Values;
            }
        }

        // Save new template to file
        var outJson = JsonConvert.SerializeObject(material);
        File.WriteAllText(@"C:\Modding\MaterialTesting\NAMED_HUMAN_GLASS.json", outJson);

        MaterialToPython.ConvertFromJsonFile(@"C:\Modding\MaterialTesting\NAMED_HUMAN_GLASS.json",
            @"C:\Modding\MaterialTesting\NAMED_HUMAN_GLASS.py");
    }

    public void Find()
    {
        System.Console.WriteLine("Starting search...");
        var watch = Stopwatch.StartNew();

        _matches = new ConcurrentBag<(string, string)>();
        Parallel.ForEach(Directory.EnumerateDirectories(DataDirectory), FindRecursively);
        //FindRecursively(dataDirectory);
        File.WriteAllText(@"C:\Modding\MaterialTesting\TransparencyMaterials.txt", _matches
            .Distinct()
            .Aggregate("", (current, next) => current + next.Item1 + " - " + next.Item2 + "\r\n"));
        watch.Stop();
        System.Console.WriteLine($"Search finished after {watch.ElapsedMilliseconds} milliseconds.");
    }

    private void FindRecursively(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.earc"))
        {
            using var unpacker = new Unpacker(file);
            var materials = unpacker.UnpackFilesByQuery(".gmtl");

            foreach (var (materialUri, materialData) in materials)
            {
                try
                {
                    var reader = new MaterialReader(materialData);
                    var material = reader.Read();
                    if (material.Interfaces.Any(i => i.Name == "CHR_Transparency_Material")
                        && material.Textures.Any(t => t.ShaderGenName.Contains("Emiss")))
                    {
                        _matches.Add((materialUri, file));
                    }
                }
                catch
                {
                    System.Console.WriteLine($"Failed to read material {materialUri}");
                }
            }
        }

        foreach (var subdirectory in Directory.EnumerateDirectories(directory))
        {
            FindRecursively(subdirectory);
        }
    }
}