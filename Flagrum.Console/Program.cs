using System.IO;
using Flagrum.Core.Gfxbin.Gmdl;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
        var gfx = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh00\mod\seams_test.gmdl.gfxbin");
        var gpu = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh00\mod\seams_test.gpubin");
        var model = new ModelReader(gfx, gpu).Read();
        var x = true;
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