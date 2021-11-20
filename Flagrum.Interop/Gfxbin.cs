using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Constructs;
using Newtonsoft.Json;

namespace Flagrum.Interop;

public static class Gfxbin
{
    [UnmanagedCallersOnly]
    public static IntPtr Import(IntPtr pathPointer)
    {
        var gfxbinPath = Marshal.PtrToStringUni(pathPointer);
        var gpubinPath = gfxbinPath.Replace(".gmdl.gfxbin", ".gpubin");
        var gfxbin = File.ReadAllBytes(gfxbinPath);
        var gpubin = File.ReadAllBytes(gpubinPath);
        var reader = new ModelReader(gfxbin, gpubin);
        var model = reader.Read();

        var key = 0;
        var meshData = new Gpubin
        {
            BoneTable = model.BoneHeaders.ToDictionary(b => key++, b => b.Name),
            Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                .Select(m => new GpubinMesh
                {
                    Name = m.Name,
                    FaceIndices = m.FaceIndices,
                    VertexPositions = m.VertexPositions,
                    Normals = m.Normals,
                    UVMaps = m.UVMaps.Select(m => new UVMap32
                    {
                        UVs = m.UVs.Select(uv => new UV32
                        {
                            U = (float)uv.U,
                            V = (float)uv.V
                        }).ToList()
                    }).ToList(),
                    WeightIndices = m.WeightIndices,
                    WeightValues = m.WeightValues.Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
                        .ToList()
                }))
        };

        var json = JsonConvert.SerializeObject(meshData);
        var temp = Path.GetTempFileName();
        File.WriteAllText(temp, json);

        var pointer = Marshal.StringToHGlobalAnsi(temp);
        return pointer;
    }

    [UnmanagedCallersOnly]
    public static IntPtr Export(IntPtr pathPointer)
    {
        var jsonPath = Marshal.PtrToStringUni(pathPointer);
        var json = File.ReadAllText(jsonPath);
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

        var exportPath = jsonPath.Replace(".json", "");
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

        var pointer = Marshal.StringToHGlobalAnsi("Success!");
        return pointer;
    }
}