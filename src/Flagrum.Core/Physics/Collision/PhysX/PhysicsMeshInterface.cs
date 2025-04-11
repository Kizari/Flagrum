using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsMeshInterface
{
    public uint TriangleCount { get; set; }
    public uint VertexCount { get; set; }
    public ulong TrianglesPointer { get; set; }
    public ulong VerticesPointer { get; set; }
    public uint Has16BitIndices { get; set; }

    public void Read(BinaryReader reader)
    {
        TriangleCount = reader.ReadUInt32();
        VertexCount = reader.ReadUInt32();
        TrianglesPointer = reader.ReadUInt64();
        VerticesPointer = reader.ReadUInt64();
        Has16BitIndices = reader.ReadUInt32();
        reader.Align(16);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(TriangleCount);
        writer.Write(VertexCount);
        writer.Write(TrianglesPointer);
        writer.Write(VerticesPointer);
        writer.Write(Has16BitIndices);
        writer.Align(16, 0xCD);
    }
}