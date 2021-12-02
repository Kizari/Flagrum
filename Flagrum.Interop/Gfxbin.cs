using System;
using System.Collections.Generic;
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
        try
        {
            var gfxbinPath = Marshal.PtrToStringUni(pathPointer);
            var gpubinPath = gfxbinPath.Replace(".gmdl.gfxbin", ".gpubin");
            var gfxbin = File.ReadAllBytes(gfxbinPath);
            var gpubin = File.ReadAllBytes(gpubinPath);
            var reader = new ModelReader(gfxbin, gpubin);
            var model = reader.Read();

            Dictionary<int, string> boneTable;
            if (model.BoneHeaders.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
            {
                // Probably a broken MO gfxbin with all IDs set to this value
                var arbitraryIndex = 0;
                boneTable = model.BoneHeaders.ToDictionary(b => arbitraryIndex++, b => b.Name);
            }
            else
            {
                boneTable = model.BoneHeaders.ToDictionary(b => (int)b.UniqueIndex, b => b.Name);
            }

            var meshData = new Gpubin
            {
                BoneTable = boneTable,
                Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                    .Where(m => m.LodNear == 0)
                    .Select(m => new GpubinMesh
                    {
                        Name = m.Name,
                        FaceIndices = m.FaceIndices,
                        VertexPositions = m.VertexPositions,
                        ColorMaps = m.ColorMaps,
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
                        WeightValues = m.WeightValues
                            .Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
                            .ToList()
                    }))
            };

            var json = JsonConvert.SerializeObject(meshData);
            var temp = Path.GetTempFileName();
            File.WriteAllText(temp, json);

            var pointer = Marshal.StringToHGlobalAnsi(temp);
            return pointer;
        }
        catch (Exception e)
        {
            Console.WriteLine("[E] Failed to import GFXBIN!");
            Console.WriteLine(e.Message);
        }

        var error = Marshal.StringToHGlobalAnsi("ERROR!");
        return error;
    }
}