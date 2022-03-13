using System;
using System.IO;
using Flagrum.Core.Gfxbin.Gmdl;

namespace Flagrum.Console;

public class Program
{
    public static void Main(string[] args)
    {
        var gfx = @"C:\Users\Kieran\Desktop\Models2\bo02\model_000\bo02_000.gmdl.gfxbin";
        var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
        var model = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu)).Read();

        foreach (var meshObject in model.MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                foreach (var uvMap in mesh.UVMaps)
                {
                    foreach (var coord in uvMap.UVs)
                    {
                        if (coord.U == Half.NaN || coord.U == Half.NegativeInfinity || coord.U == Half.PositiveInfinity
                            || coord.V == Half.NaN || coord.V == Half.NegativeInfinity ||
                            coord.V == Half.PositiveInfinity)
                        {
                            System.Console.WriteLine($"{meshObject.Name}, {mesh.Name} - U: {coord.U}, V: {coord.V}");
                        }
                    }
                }
            }
        }

        var x = true;

        // var finder = new FileFinder();
        // finder.FindByQuery(
        //     file => file.Uri.EndsWith(".amdl"),
        //     file => System.Console.WriteLine($"{file.Uri.Split('/').Last()}\t\t{file.Uri}")
        // );

        // var input = @"C:\Modding\teal.png";
        // var output = @"C:\Modding\teal.btex";
        // var converter = new TextureConverter();
        // var btex = converter.ToBtex("teal", "png", TextureType.Mrs, File.ReadAllBytes(input));
        // File.WriteAllBytes(output, btex);
        // var gfx = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\le_ar_gqshop1\le_ar_gqshop1.gmdl.gfxbin");
        // var gpu = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\le_ar_gqshop1\le_ar_gqshop1.gpubin");
        // var model = new ModelReader(gfx, gpu).Read();
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