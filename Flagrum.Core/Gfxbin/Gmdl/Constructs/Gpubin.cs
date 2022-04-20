using System.Collections.Generic;

namespace Flagrum.Core.Gfxbin.Gmdl.Constructs;

public class Gpubin
{
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
    public BlenderMaterialData BlenderMaterial { get; set; }
}

public class BlenderMaterialData
{
    public string Hash { get; set; }
    public string Name { get; set; }
    public float[] UVScale { get; set; }
    public IEnumerable<BlenderTextureData> Textures { get; set; }
}

public class BlenderTextureData
{
    public string Hash { get; set; }
    public string Name { get; set; }
    public string Slot { get; set; }
    public string Path { get; set; }
    public string Uri { get; set; }
}

public class MaterialData
{
    public string Id { get; set; }
    public int WeightLimit { get; set; }
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