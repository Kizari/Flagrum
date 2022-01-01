using System.Collections.Generic;
using Flagrum.Core.Gfxbin.Gmdl.Components;

namespace Flagrum.Core.Gfxbin.Gmdl.Constructs;

public class Gpubin
{
    public string Target { get; set; }
    public string Uuid { get; set; }
    public string Title { get; set; }
    public Dictionary<int, string> BoneTable { get; set; }
    public IEnumerable<GpubinMesh> Meshes { get; set; }
}

public class GpubinMesh
{
    public string Name { get; set; }
    public uint[,] FaceIndices { get; set; }
    public List<Vector3> VertexPositions { get; set; } = new();
    public List<Normal> Normals { get; set; } = new();
    public List<Normal> Tangents { get; set; } = new();
    public List<UVMap32> UVMaps { get; set; } = new();
    public List<ColorMap> ColorMaps { get; set; } = new();
    public List<List<ushort[]>> WeightIndices { get; set; } = new();
    public List<List<int[]>> WeightValues { get; set; } = new();
    public MaterialData Material { get; set; }
    public MaterialType MaterialType { get; set; }
}

public class MaterialData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, float[]> Inputs { get; set; }
    public Dictionary<string, string> Textures { get; set; }

    public List<TextureData> TextureData { get; set; } = new();
}

public class TextureData
{
    public string Id { get; set; }
    public string Uri { get; set; }
    public byte[] Data { get; set; }
}