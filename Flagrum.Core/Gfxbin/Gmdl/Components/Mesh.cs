using System.Collections.Generic;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Core.Gfxbin.Gmdl.Components;

public enum VertexLayoutType
{
    NULL = 0x0,
    Skinning_4Bones = 0x1,
    Skinning_8Bones = 0x2,
    NTB2 = 0x4,
    Skinning_1Bones = 0x8,
    Skinning_6Bones = 0x10,
    BoneIndices16 = 0x20,
    // This is a test entry for Comrades, the name is an assumption
    BoneIndices32 = 0x21,
    // This is a test entry for Comrades, the name is an assumption
    BoneIndices64 = 0x22,
    Skinning_Any = 0x1B
}

public enum PrimitiveType
{
    PrimitiveTypePointList = 0x0,
    PrimitiveTypeLineList = 0x1,
    PrimitiveTypeLineStrip = 0x2,
    PrimitiveTypeTriangleList = 0x3,
    PrimitiveTypeTriangleStrip = 0x4,
    PrimitiveTypeLineListADJ = 0x5,
    PrimitiveTypeLineStripADJ = 0x6,
    PrimitiveTypeTriangleListADJ = 0x7,
    PrimitiveTypeTriangleStripADJ = 0x8,
    PrimitiveType1_ControlPointPatchList = 0x9,
    PrimitiveType2_ControlPointPatchList = 0xA,
    PrimitiveType3_ControlPointPatchList = 0xB,
    PrimitiveType4_ControlPointPatchList = 0xC,
    PrimitiveType5_ControlPointPatchList = 0xD,
    PrimitiveType6_ControlPointPatchList = 0xE,
    PrimitiveType7_ControlPointPatchList = 0xF,
    PrimitiveType8_ControlPointPatchList = 0x10,
    PrimitiveType9_ControlPointPatchList = 0x11,
    PrimitiveType10_ControlPointPatchList = 0x12,
    PrimitiveType11_ControlPointPatchList = 0x13,
    PrimitiveType12_ControlPointPatchList = 0x14,
    PrimitiveType13_ControlPointPatchList = 0x15,
    PrimitiveType14_ControlPointPatchList = 0x16,
    PrimitiveType15_ControlPointPatchList = 0x17,
    PrimitiveType16_ControlPointPatchList = 0x18,
    PrimitiveType17_ControlPointPatchList = 0x19,
    PrimitiveType18_ControlPointPatchList = 0x1A,
    PrimitiveType19_ControlPointPatchList = 0x1B,
    PrimitiveType20_ControlPointPatchList = 0x1C,
    PrimitiveType21_ControlPointPatchList = 0x1D,
    PrimitiveType22_ControlPointPatchList = 0x1E,
    PrimitiveType23_ControlPointPatchList = 0x1F,
    PrimitiveType24_ControlPointPatchList = 0x20,
    PrimitiveType25_ControlPointPatchList = 0x21,
    PrimitiveType26_ControlPointPatchList = 0x22,
    PrimitiveType27_ControlPointPatchList = 0x23,
    PrimitiveType28_ControlPointPatchList = 0x24,
    PrimitiveType29_ControlPointPatchList = 0x25,
    PrimitiveType30_ControlPointPatchList = 0x26,
    PrimitiveType31_ControlPointPatchList = 0x27,
    PrimitiveType32_ControlPointPatchList = 0x28,
    PrimitiveTypeNum = 0x29
}

public enum IndexType
{
    IndexType16 = 0x0,
    IndexType32 = 0x1,
    IndexTypeNum = 0x2
}

public enum MaterialType
{
    FourWeights,
    SixWeights,
    EightWeights,
    OneWeight,
    Avatara
}

public class Mesh
{
    public string Name { get; set; }
    public IEnumerable<uint> BoneIds { get; set; }
    public VertexLayoutType VertexLayoutType { get; set; }

    public Aabb Aabb { get; set; }
    public bool IsOrientedBB { get; set; }
    public OrientedBB OrientedBB { get; set; }
    public PrimitiveType PrimitiveType { get; set; }

    public uint FaceIndexBufferOffset { get; set; }
    public int[,] FaceIndices { get; set; }

    public uint VertexCount { get; set; }
    public IList<VertexStreamDescription> VertexStreamDescriptions { get; set; }
    public uint VertexBufferOffset { get; set; }

    public uint InstanceNumber { get; set; }

    public IEnumerable<SubGeometry> SubGeometries { get; set; }

    public ulong DefaultMaterialHash { get; set; }

    public uint Flag { get; set; }
    public uint Flags { get; set; }
    public int DrawPriorityOffset { get; set; }
    public float LodNear { get; set; }
    public float LodFar { get; set; }
    public float LodFade { get; set; }
    public uint PartsId { get; set; }
    public uint BreakableBoneIndex { get; set; }
    public sbyte LowLodShadowCascadeNo { get; set; }
    public IEnumerable<MeshPart> MeshParts { get; set; }

    public List<Vector3> VertexPositions { get; set; } = new();
    public List<Normal> Normals { get; set; } = new();
    public List<Normal> Tangents { get; set; } = new();
    public List<ColorMap> ColorMaps { get; set; } = new();
    public List<UVMap> UVMaps { get; set; } = new();
    public List<List<ushort[]>> WeightIndices { get; set; } = new();
    public List<List<byte[]>> WeightValues { get; set; } = new();
    
    public MaterialType MaterialType { get; set; }

    // NOTE: Class contains unknowns
    public byte Unknown1 { get; set; }
    public bool Unknown2 { get; set; }
    public bool Unknown3 { get; set; }
    public uint Unknown4 { get; set; }
    public uint Unknown5 { get; set; }
    public bool Unknown6 { get; set; }
}