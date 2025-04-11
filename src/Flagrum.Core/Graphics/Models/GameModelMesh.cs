using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Flagrum.Core.Mathematics;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class GameModelMesh : IMessagePackItem
{
    public string Name { get; set; }
    public uint Unknown { get; set; }
    public IList<uint> BoneIds { get; set; }
    public VertexLayoutType VertexLayoutType { get; set; }
    public bool Unknown2 { get; set; }
    public AxisAlignedBoundingBox AABB { get; set; }
    public bool IsOrientedBB { get; set; }
    public OrientedBoundingBox OrientedBoundingBox { get; set; }
    public PrimitiveType PrimitiveType { get; set; }
    public uint FaceIndexCount { get; set; }
    public FaceIndexType FaceIndexType { get; set; }
    public uint GpubinIndex { get; set; }
    public uint FaceIndexOffset { get; set; }
    public uint FaceIndexSize { get; set; }
    public uint VertexCount { get; set; }
    public IList<VertexStream> VertexStreams { get; set; }
    public uint VertexBufferOffset { get; set; }
    public uint VertexBufferSize { get; set; }
    public uint InstanceNumber { get; set; }
    public IList<GameModelSubgeometry> Subgeometries { get; set; }
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
    public IList<GameModelMeshPart> Parts { get; set; }
    public bool Unknown9 { get; set; }
    public uint Flags { get; set; }
    public bool Unknown10 { get; set; }
    public uint BreakableBoneIndex { get; set; }
    public uint LowLodShadowCascadeNo { get; set; }

    public uint[,] FaceIndices { get; set; }
    public ConcurrentDictionary<VertexElementSemantic, IList> Semantics { get; } = new();
    public int LodLevel { get; set; } = -1;

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
        AABB = new AxisAlignedBoundingBox();
        AABB.Read(reader);

        if (reader.DataVersion >= 20160705)
        {
            IsOrientedBB = reader.Read<bool>();
            OrientedBoundingBox = new OrientedBoundingBox();
            OrientedBoundingBox.Read(reader);
        }

        PrimitiveType = (PrimitiveType)reader.Read<int>();
        FaceIndexCount = reader.Read<uint>();
        FaceIndexType = (FaceIndexType)reader.Read<uint>();

        if (reader.DataVersion >= 20220707)
        {
            GpubinIndex = reader.Read<uint>();
        }

        FaceIndexOffset = reader.Read<uint>();
        FaceIndexSize = reader.Read<uint>();
        VertexCount = reader.Read<uint>();
        VertexStreams = reader.Read<List<VertexStream>>();
        VertexBufferOffset = reader.Read<uint>();
        VertexBufferSize = reader.Read<uint>();

        if (reader.DataVersion is >= 20150413 and < 20220707)
        {
            InstanceNumber = reader.Read<uint>();
        }

        if (reader.DataVersion >= 20220707)
        {
            var subgeometryCount = reader.Read<uint>();
            for (var i = 0; i < subgeometryCount; i++)
            {
                var subgeometry = new GameModelSubgeometry();
                subgeometry.Read(reader);
                Subgeometries.Add(subgeometry);
            }
        }
        else
        {
            Subgeometries = reader.Read<List<GameModelSubgeometry>>();
        }

        if (reader.DataVersion >= 20220707)
        {
            Unknown6 = reader.Read<uint>();
            UnknownOffset = reader.Read<uint>();
            UnknownSize = reader.Read<uint>();
        }

        MaterialHash = reader.Read<ulong>();

        if (reader.DataVersion >= 20140623)
        {
            DrawPriorityOffset = reader.Read<int>();
            Unknown7 = reader.Read<bool>();
            Unknown8 = reader.Read<bool>();
            LodNear = reader.Read<float>();
            LodFar = reader.Read<float>();
            LodFade = reader.Read<float>();
        }

        if (reader.DataVersion is >= 20140814 and < 20220707)
        {
            Unknown11 = reader.Read<bool>();
        }

        if (reader.DataVersion is >= 20141112 and < 20220707)
        {
            Unknown12 = reader.Read<bool>();
        }

        if (reader.DataVersion >= 20140815)
        {
            PartsId = reader.Read<uint>();
        }

        if (reader.DataVersion >= 20141115)
        {
            Parts = reader.Read<List<GameModelMeshPart>>();
        }

        if (reader.DataVersion >= 20150413)
        {
            Unknown9 = reader.Read<bool>();
        }

        if (reader.DataVersion >= 20150430)
        {
            Flags = reader.Read<uint>();
        }

        if (reader.DataVersion >= 20150512)
        {
            Unknown10 = reader.Read<bool>();
        }

        if (reader.DataVersion < 20160420)
        {
            BreakableBoneIndex = uint.MaxValue;
            LowLodShadowCascadeNo = 2;
        }
        else
        {
            BreakableBoneIndex = reader.Read<uint>();
            LowLodShadowCascadeNo = reader.Read<uint>();
        }
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(Name);

        if (writer.DataVersion < 20220707)
        {
            writer.Write(Unknown);
            writer.Write(BoneIds);
        }

        writer.Write((uint)VertexLayoutType);
        writer.Write(Unknown2);
        AABB.Write(writer);
        writer.Write(IsOrientedBB);
        OrientedBoundingBox.Write(writer);

        writer.Write((int)PrimitiveType);
        writer.Write(FaceIndexCount);
        writer.Write((uint)FaceIndexType);

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(GpubinIndex);
        }

        writer.Write(FaceIndexOffset);
        writer.Write(FaceIndexSize);
        writer.Write(VertexCount);
        writer.Write(VertexStreams);
        writer.Write(VertexBufferOffset);
        writer.Write(VertexBufferSize);

        if (writer.DataVersion < 20220707)
        {
            writer.Write(InstanceNumber);
        }

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(Subgeometries.Count);
            foreach (var subgeometry in Subgeometries)
            {
                subgeometry.Write(writer);
            }
        }
        else
        {
            writer.Write(Subgeometries);
        }

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(Unknown6);
            writer.Write(UnknownOffset);
            writer.Write(UnknownSize);
        }

        writer.Write(MaterialHash);
        writer.Write(DrawPriorityOffset);
        writer.Write(Unknown7);
        writer.Write(Unknown8);
        writer.Write(LodNear);
        writer.Write(LodFar);
        writer.Write(LodFade);

        if (writer.DataVersion < 20220707)
        {
            writer.Write(Unknown11);
            writer.Write(Unknown12);
        }

        writer.Write(PartsId);
        writer.Write(Parts);
        writer.Write(Unknown9);
        writer.Write(Flags);
        writer.Write(Unknown10);
        writer.Write(BreakableBoneIndex);
        writer.Write(LowLodShadowCascadeNo);
    }
}