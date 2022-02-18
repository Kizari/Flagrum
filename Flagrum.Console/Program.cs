using System.IO;
using System.Linq;
using Flagrum.Core.Ebex;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
        using var fileStream = new FileStream(@"C:\Modding\nh02_initialize.ebex",
            FileMode.Open, FileAccess.Read);

        var reader = new EbexReader(fileStream);
        var result = reader.Read().ToList();

        var writer = new EbexWriter(result);
        var outputStream = writer.Write() as MemoryStream;
        File.WriteAllBytes(@"C:\Modding\nh02_initialize_reserialized.xml", outputStream.ToArray());

        var x = true;

        // var gfx = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh05\model_000\nh05_000.gmdl.gfxbin");
        // var gpu = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh05\model_000\nh05_000.gpubin");
        // var model = new ModelReader(gfx, gpu).Read();
        //
        // foreach (var mesh in model.MeshObjects[0].Meshes)
        // {
        //     System.Console.WriteLine(mesh.Name);
        //     for (var i = 0; i < mesh.ColorMaps.Count; i++)
        //     {
        //         var colorMap = mesh.ColorMaps[i];
        //         if (colorMap.Colors.Any(c => c.A is > 0 and < 255))
        //         {
        //             System.Console.WriteLine($"- Color Map {i}");
        //         }
        //     }
        //
        //     System.Console.WriteLine("\n");
        // }
        //
        // var x = true;
        //Visit(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas");
        // var json = File.ReadAllText(@"C:\Modding\map3.json");
        // var map = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<string>>>(json);
        //
        // var start = DateTime.Now;
        //
        // var root = new NamedTreeNode("data://", null);
        // foreach (var (archive, uris) in map)
        // {
        //     foreach (var uri in uris)
        //     {
        //         var tokens = uri.Split('/');
        //         var currentDirectory = root;
        //         foreach (var token in tokens)
        //         {
        //             var subdirectory = currentDirectory.Children
        //                 .FirstOrDefault(c => c.Name == token) ?? currentDirectory.AddChild(token);
        //
        //             currentDirectory = subdirectory;
        //         }
        //     }
        // }
        //
        // var serializer = new DataContractSerializer(typeof(NamedTreeNode));
        // using var fileStream = new FileStream(@"C:\Modding\Tree.xml", FileMode.Create, FileAccess.Write);
        // serializer.WriteObject(fileStream, root);
        //
        // //File.WriteAllText(@"C:\Modding\UriDirectoryMap.json", JsonConvert.SerializeObject(root));
        // System.Console.WriteLine((DateTime.Now - start).TotalMilliseconds);
    }

    private static void Visit(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            if (!file.EndsWith(".earc") && !file.EndsWith(".heb") && !file.EndsWith(".hephysx") &&
                !file.EndsWith(".bk2") && !file.EndsWith(".sab"))
            {
                System.Console.WriteLine(file);
            }
        }

        foreach (var subdirectory in Directory.EnumerateDirectories(directory))
        {
            Visit(subdirectory);
        }
    }
}