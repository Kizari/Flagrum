using System.IO;
using System.Linq;
using Flagrum.Core.Gfxbin.Gmdl;

namespace Flagrum.Console.Utilities;

public class AOFixer
{
    public static void Run()
    {
        var gfx = @"C:\Users\Kieran\Desktop\character\nh\nh02\model_000\nh02_000.gmdl.gfxbin";
        var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
        var gfxbinData = File.ReadAllBytes(gfx);
        var gpubinData = File.ReadAllBytes(gpu);
        var reader = new ModelReader(gfxbinData, gpubinData);
        var model = reader.Read();

        var meshes = new[] {"EyelashShape", "BodyShape"};
        var matches = model.MeshObjects[0].Meshes.Where(m => meshes.Contains(m.Name));

        foreach (var match in matches)
        {
            foreach (var color in match.ColorMaps[2].Colors)
            {
                color.R = byte.MaxValue;
                color.G = byte.MaxValue;
                color.B = byte.MaxValue;
                color.A = byte.MaxValue;
            }
        }

        var (gfxbin, gpubin) = new ModelWriter(model).Write();
        File.WriteAllBytes(@"C:\Modding\nh02_000.gmdl.gfxbin", gfxbin);
        File.WriteAllBytes(@"C:\Modding\nh02_000.gpubin", gpubin);
    }
}