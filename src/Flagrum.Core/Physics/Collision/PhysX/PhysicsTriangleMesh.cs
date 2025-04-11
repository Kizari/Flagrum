using System.IO;
using Flagrum.Core.Mathematics;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsTriangleMesh : PhysicsBase
{
    public ulong UnknownPointer { get; set; }
    public uint Has16BitIndices { get; set; }
    public uint VertexCount { get; set; }
    public uint TriangleCount { get; set; }
    public ulong VerticesPointer { get; set; }
    public ulong TrianglesPointer { get; set; }
    public PhysicsCollisionModel CollisionModel { get; set; }
    public AxisAlignedBoundingBox AxisAlignedBoundingBox { get; set; }
    public ulong ExtraTrigDataPointer { get; set; }
    public byte Flags { get; set; }
    public ulong MaterialIndicesPointer { get; set; }
    public ulong FaceRemapPointer { get; set; }
    public ulong AdjacenciesPointer { get; set; }
    public uint AdjacenciesCount { get; set; }
    public float ConvexEdgeThreshold { get; set; }
    public PhysicsMeshInterface MeshInterface { get; set; }
    public uint OwnsMemory { get; set; }
    public ulong MeshFactoryPointer { get; set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        
        UnknownPointer = reader.ReadUInt64();
        Has16BitIndices = reader.ReadUInt32();
        
        reader.Align(16);
        VertexCount = reader.ReadUInt32();
        TriangleCount = reader.ReadUInt32();
        VerticesPointer = reader.ReadUInt64();
        TrianglesPointer = reader.ReadUInt64();
        
        reader.Align(16);
        CollisionModel = new PhysicsCollisionModel();
        CollisionModel.Read(reader);

        AxisAlignedBoundingBox = new AxisAlignedBoundingBox();
        AxisAlignedBoundingBox.Read(reader);

        ExtraTrigDataPointer = reader.ReadUInt64();
        Flags = reader.ReadByte();
        
        reader.Align(16);
        MaterialIndicesPointer = reader.ReadUInt64();
        FaceRemapPointer = reader.ReadUInt64();
        AdjacenciesPointer = reader.ReadUInt64();
        AdjacenciesCount = reader.ReadUInt32();
        ConvexEdgeThreshold = reader.ReadSingle();

        MeshInterface = new PhysicsMeshInterface();
        MeshInterface.Read(reader);
        OwnsMemory = reader.ReadUInt32();
        
        reader.Align(16);
        MeshFactoryPointer = reader.ReadUInt64();
        reader.Align(16);
        reader.Align(128);
    }

    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        
        writer.Write(UnknownPointer);
        writer.Write(Has16BitIndices);
        
        writer.Align(16, 0xCD);
        writer.Write(VertexCount);
        writer.Write(TriangleCount);
        writer.Write(VerticesPointer);
        writer.Write(TrianglesPointer);
        
        writer.Align(16, 0xCD);
        CollisionModel.Write(writer);
        AxisAlignedBoundingBox.Write(writer);
        writer.Write(ExtraTrigDataPointer);
        writer.Write(Flags);
        
        writer.Align(16, 0xCD);
        writer.Write(MaterialIndicesPointer);
        writer.Write(FaceRemapPointer);
        writer.Write(AdjacenciesPointer);
        writer.Write(AdjacenciesCount);
        writer.Write(ConvexEdgeThreshold);
        
        MeshInterface.Write(writer);
        writer.Write(OwnsMemory);
        
        writer.Align(16, 0xCD);
        writer.Write(MeshFactoryPointer);
        writer.Align(16, 0xCD);
    }
}