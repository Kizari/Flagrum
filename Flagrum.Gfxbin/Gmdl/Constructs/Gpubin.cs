using System.Collections.Generic;

namespace Flagrum.Gfxbin.Gmdl.Constructs;

public class Gpubin
{
    public Dictionary<int, string> BoneTable { get; set; }
    public IEnumerable<GpubinMesh> Meshes { get; set; }
}

public class GpubinMesh
{
    public string Name { get; set; }
    public int[,] FaceIndices { get; set; }
    public List<Vector3> VertexPositions { get; set; } = new();
    public List<Normal> Normals { get; set; } = new();
    public List<Normal> Tangents { get; set; } = new();
    public List<UVMap32> UVMaps { get; set; } = new();
    public List<List<ushort[]>> WeightIndices { get; set; } = new();
    public List<List<int[]>> WeightValues { get; set; } = new();
}