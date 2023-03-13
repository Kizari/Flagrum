using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinMesh : IMessagePackItem
{
    public string Name { get; set; }
    public uint Unknown { get; set; }
    public IList<uint> BoneIds { get; set; }
    public VertexLayoutType VertexLayoutType { get; set; }
    public bool Unknown2 { get; set; }
    public GfxbinAABB AABB { get; set; }
    public bool IsOrientedBB { get; set; }
    public OrientedBB OrientedBB { get; set; }
    public PrimitiveType PrimitiveType { get; set; }
    public uint FaceIndexCount { get; set; }
    public IndexType FaceIndexType { get; set; }
    public uint GpubinIndex { get; set; }
    public uint FaceIndexOffset { get; set; }
    public uint FaceIndexSize { get; set; }
    public uint VertexCount { get; set; }
    public IList<GfxbinVertexStream> VertexStreams { get; set; }
    public uint VertexBufferOffset { get; set; }
    public uint VertexBufferSize { get; set; }
    public uint InstanceNumber { get; set; }
    public IList<GfxbinSubgeometry> Subgeometries { get; set; }
    public uint Unknown6 { get; set; }
    public uint UnknownOffset { get; set; }
    public uint UnknownSize { get; set; }
    public ulong MaterialHash { get; set; }
    public int DrawPriorityOffset { get; set; }
    public bool Unknown7 { get; set; }
    public bool Unknown8 { get; set; }
    public float LodNear { get; set; }
    public float LodFar { get; set; }
    public float LodFade { get; set; }
    public bool Unknown11 { get; set; }
    public bool Unknown12 { get; set; }
    public uint PartsId { get; set; }
    public IList<GfxbinMeshPart> Parts { get; set; }
    public bool Unknown9 { get; set; }
    public uint Flags { get; set; }
    public bool Unknown10 { get; set; }
    public uint BreakableBoneIndex { get; set; }
    public uint LowLodShadowCascadeNo { get; set; }

    public uint[,] FaceIndices { get; set; }

    public ConcurrentDictionary<VertexElementSemantic, IList> Semantics { get; set; } = new();
    // public List<Vector3> VertexPositions { get; set; } = new();
    // public List<Normal> Normals { get; set; } = new();
    // public List<Normal> Tangents { get; set; } = new();
    // public List<ColorMap> ColorMaps { get; set; } = new();
    // public List<UVMap> UVMaps { get; set; } = new();
    // public List<List<ushort[]>> WeightIndices { get; set; } = new();
    // public List<List<byte[]>> WeightValues { get; set; } = new();

    public void Read(MessagePackReader reader)
    {
        Name = reader.Read<string>();

        if (reader.DataVersion < 20220707)
        {
            Unknown = reader.Read<uint>();
            BoneIds = reader.Read<List<uint>>();
        }

        VertexLayoutType = (VertexLayoutType)reader.Read<uint>();
        Unknown2 = reader.Read<bool>();
        AABB = reader.ReadAABB();
        IsOrientedBB = reader.Read<bool>();
        OrientedBB = new OrientedBB(
            reader.ReadVector3(),
            reader.ReadVector3(),
            reader.ReadVector3(),
            reader.ReadVector3()
        );

        PrimitiveType = (PrimitiveType)reader.Read<int>();
        FaceIndexCount = reader.Read<uint>();
        FaceIndexType = (IndexType)reader.Read<uint>();

        if (reader.DataVersion >= 20220707)
        {
            GpubinIndex = reader.Read<uint>();
        }

        FaceIndexOffset = reader.Read<uint>();
        FaceIndexSize = reader.Read<uint>();
        VertexCount = reader.Read<uint>();
        VertexStreams = reader.Read<List<GfxbinVertexStream>>();
        VertexBufferOffset = reader.Read<uint>();
        VertexBufferSize = reader.Read<uint>();

        if (reader.DataVersion < 20220707)
        {
            InstanceNumber = reader.Read<uint>();
        }

        if (reader.DataVersion >= 20220707)
        {
            var subgeometryCount = reader.Read<uint>();
            for (var i = 0; i < subgeometryCount; i++)
            {
                var subgeometry = new GfxbinSubgeometry();
                subgeometry.Read(reader);
                Subgeometries.Add(subgeometry);
            }
        }
        else
        {
            Subgeometries = reader.Read<List<GfxbinSubgeometry>>();
        }

        if (reader.DataVersion >= 20220707)
        {
            Unknown6 = reader.Read<uint>();
            UnknownOffset = reader.Read<uint>();
            UnknownSize = reader.Read<uint>();
        }

        MaterialHash = reader.Read<ulong>();
        DrawPriorityOffset = reader.Read<int>();
        Unknown7 = reader.Read<bool>();
        Unknown8 = reader.Read<bool>();
        LodNear = reader.Read<float>();
        LodFar = reader.Read<float>();
        LodFade = reader.Read<float>();

        if (reader.DataVersion < 20220707)
        {
            Unknown11 = reader.Read<bool>();
            Unknown12 = reader.Read<bool>();
        }

        PartsId = reader.Read<uint>();
        Parts = reader.Read<List<GfxbinMeshPart>>();
        Unknown9 = reader.Read<bool>();
        Flags = reader.Read<uint>();
        Unknown10 = reader.Read<bool>();
        BreakableBoneIndex = reader.Read<uint>();
        LowLodShadowCascadeNo = reader.Read<uint>();
    }
}