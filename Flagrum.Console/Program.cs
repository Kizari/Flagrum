using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Constructs;
using Newtonsoft.Json;

namespace Flagrum.Console;

public class Program
{
    // public static void Import()
    // {
    //     var gfxbinPath = "C:\\Testing\\extract\\mod\\prompto_custom_2\\promptocustom2.gmdl.gfxbin";
    //     var gpubinPath = gfxbinPath.Replace(".gmdl.gfxbin", ".gpubin");
    //     var gfxbin = File.ReadAllBytes(gfxbinPath);
    //     var gpubin = File.ReadAllBytes(gpubinPath);
    //     var reader = new ModelReader(gfxbin, gpubin);
    //     var model = reader.Read();
    //
    //     Dictionary<int, string> boneTable;
    //     if (model.BoneHeaders.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
    //     {
    //         // Probably a broken MO gfxbin with all IDs set to this value
    //         var arbitraryIndex = 0;
    //         boneTable = model.BoneHeaders.ToDictionary(b => arbitraryIndex++, b => b.Name);
    //     }
    //     else
    //     {
    //         boneTable = model.BoneHeaders.ToDictionary(b => (int)b.UniqueIndex, b => b.Name);
    //     }
    //
    //     var meshData = new Gpubin
    //     {
    //         BoneTable = boneTable,
    //         Meshes = model.MeshObjects.SelectMany(o => o.Meshes
    //             .Where(m => m.LodNear == 0)
    //             .Select(m => new GpubinMesh
    //             {
    //                 Name = m.Name,
    //                 FaceIndices = m.FaceIndices,
    //                 VertexPositions = m.VertexPositions,
    //                 Normals = m.Normals,
    //                 UVMaps = m.UVMaps.Select(m => new UVMap32
    //                 {
    //                     UVs = m.UVs.Select(uv => new UV32
    //                     {
    //                         U = (float)uv.U,
    //                         V = (float)uv.V
    //                     }).ToList()
    //                 }).ToList(),
    //                 WeightIndices = m.WeightIndices,
    //                 WeightValues = m.WeightValues
    //                     .Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
    //                     .ToList()
    //             }))
    //     };
    //
    //     var json = JsonConvert.SerializeObject(meshData);
    //     var temp = Path.GetTempFileName();
    //     File.WriteAllText(temp, json);
    //
    //     System.Console.WriteLine("Success");
    // }

    public static void Main(string[] args)
    {
        GfxbinTests.CheckMaterialDefaults();
        // var gfx = "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh02\\model_000\\nh02_000.gmdl.gfxbin";
        // var gpu = "C:\\Users\\Kieran\\Desktop\\character\\nh\\nh02\\model_000\\nh02_000.gpubin";
        //
        // var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
        // var model = reader.Read();
        //
        // var output = "";
        // foreach (var bone in model.BoneHeaders)
        // {
        //     output += "new BoneHeader {Name = \"" + bone.Name + "\", LodIndex = " + bone.LodIndex + "},\n";
        // }
        //
        // System.Console.WriteLine(output);

        //Import();
        //GfxbinTests.GetBoneTable();
       // GfxbinTests.CheckMaterialDefaults();
        //Test();
    }

    public static void Test()
    {
        // 970-984 model name
        // 1071-1088 mod dir
        // 1154-1171 mod dir
        // 1198-1215 mod dir
        // 1262-1279 mod dir
        // 1280-1294 model name.fbx
        // 1299-1313 model name

        var stream = new MemoryStream(File.ReadAllBytes("C:\\Testing\\prompto.exml"));

        var part1 = new byte[970];
        stream.Read(part1);

        var buffer = new byte[14];
        stream.Read(buffer);
        var log = Encoding.ASCII.GetString(buffer);
        System.Console.WriteLine(log);

        var part2 = new byte[87];
        stream.Read(part2);

        buffer = new byte[17];
        stream.Read(buffer);
        log = Encoding.ASCII.GetString(buffer);
        System.Console.WriteLine(log);

        var part3 = new byte[66];
        stream.Read(part3);

        buffer = new byte[17];
        stream.Read(buffer);
        log = Encoding.ASCII.GetString(buffer);
        System.Console.WriteLine(log);

        var part4 = new byte[27];
        stream.Read(part4);

        buffer = new byte[17];
        stream.Read(buffer);
        log = Encoding.ASCII.GetString(buffer);
        System.Console.WriteLine(log);

        var part5 = new byte[47];
        stream.Read(part5);

        buffer = new byte[17];
        stream.Read(buffer);
        log = Encoding.ASCII.GetString(buffer);
        System.Console.WriteLine(log);

        var part6 = new byte[1];
        stream.Read(part6);

        buffer = new byte[14];
        stream.Read(buffer);
        log = Encoding.ASCII.GetString(buffer);
        System.Console.WriteLine(log);

        var part7 = new byte[5];
        stream.Read(part7);

        buffer = new byte[14];
        stream.Read(buffer);
        log = Encoding.ASCII.GetString(buffer);
        System.Console.WriteLine(log);

        var part8 = new byte[62];
        stream.Read(part8);

        var s = "";
        s += PartToString(part1);
        s += PartToString(part2);
        s += PartToString(part3);
        s += PartToString(part4);
        s += PartToString(part5);
        s += PartToString(part6);
        s += PartToString(part7);
        s += PartToString(part8);

        File.WriteAllText("C:\\Testing\\Prompto.cs", s);
    }

    private static string PartToString(byte[] part)
    {
        var s = $"parts.Add(new byte[{part.Length}]\n";
        s += "{\n";

        foreach (var b in part)
        {
            s += b + ",";
        }

        s = s.Remove(s.Length - 1);
        s += "\n});\n\n";
        return s;
    }
}