using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Flagrum.Gfxbin.Gmdl;
using Newtonsoft.Json;

namespace Flagrum.Interop;

public static class Gfxbin
{
    private const string CDeclarations = @"
            struct Color4
            {
                unsigned char R;
                unsigned char G;
                unsigned char B;
                unsigned char A;
            };

            struct Vector4
            {
                signed char X;
                signed char Y;
                signed char Z;
                signed char W;
            };

            struct ColorMap
            {
                struct Color4* Color;
            };

            struct WeightMap
            {
                struct VertexWeights* VertexWeights;
            };

            struct VertexWeights
            {
                struct VertexWeight* VertexWeight;
            };

            struct VertexWeight
            {
                int BoneIndex;
                float Weight;
            };

            struct Vector2
            {
                float X;
                float Y;
            };

            struct UVMap
            {
                struct Vector2* Coords;
            };

            struct Gpubin
            {
                struct Vector3* VertexPositions;
                struct Vector4* Normals;
                struct ColorMap* ColorMaps;
                struct WeightMap* WeightMaps;
                struct UVMap* UVMaps;
            };";

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
}