using System.IO;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Utilities;

namespace Flagrum.Console.Utilities;

public class PlatinumDemoGmdl
{
    public static void Convert(string gmdlPath)
    {
        var gpubinPath = gmdlPath.Replace(".gmdl.gfxbin", ".gpubin");
        var model = new ModelReader(File.ReadAllBytes(gmdlPath), File.ReadAllBytes(gpubinPath)).Read();
        
        model.Header.Version = 20160705;
        // foreach (var meshObject in model.MeshObjects)
        // {
        //     foreach (var mesh in meshObject.Meshes)
        //     {
        //         mesh.VertexLayoutType = VertexLayoutType.Skinning_1Bones;
        //     }
        // }
        
        var (gmdlOut, gpubinOut) = new ModelWriter(model).Write();
        var gmdlOutPath = gmdlPath.Insert(gmdlPath.LastIndexOf('\\'), "\\restored");
        var gpubinOutPath = gpubinPath.Insert(gpubinPath.LastIndexOf('\\'), "\\restored");
        IOHelper.EnsureDirectoriesExistForFilePath(gmdlOutPath);
        File.WriteAllBytes(gmdlOutPath, gmdlOut);
        File.WriteAllBytes(gpubinOutPath, gpubinOut);
    }
}