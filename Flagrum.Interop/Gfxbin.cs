using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Data;
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

        model.Gpubin.BoneTable = model.BoneHeaders.ToDictionary(b => (int)b.UniqueIndex, b => b.Name);
        var json = JsonConvert.SerializeObject(model.Gpubin);
        var temp = Path.GetTempFileName();
        File.WriteAllText(temp, json);

        var pointer = Marshal.StringToHGlobalAnsi(temp);
        return pointer;
    }

    [UnmanagedCallersOnly]
    public static void Export(IntPtr pathPointer)
    {
        var jsonPath = Marshal.PtrToStringUni(pathPointer);
        var exportPath = jsonPath.Replace(".json", "");

        var json = File.ReadAllText(jsonPath);
        File.Delete(jsonPath);

        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);
        var model = new Model
        {
            Gpubin = gpubin
        };

        var writer = new ModelWriter(model, new byte[1]);
        var data = writer.Write();
        File.WriteAllBytes(exportPath, data);
    }
}