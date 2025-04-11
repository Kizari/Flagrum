using System.IO;
using System.Numerics;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsCollisionModel
{
    public ulong MeshInterfacePointer { get; set; }
    public float GeometryEpsilon { get; set; }
    public Vector4 BoundsMin { get; set; }
    public Vector4 BoundsMax { get; set; }
    public Vector4 InvDiagonal { get; set; }
    public Vector4 DiagonalScaler { get; set; }
    public uint PageSize { get; set; }
    public uint RootPageCount { get; set; }
    public uint LevelsCount { get; set; }
    public uint TotalNodes { get; set; }
    public uint TotalPages { get; set; }
    public uint Flags { get; set; } // USER_ALLOCATED=1, IS_DYNAMIC=2
    public uint Reserved { get; set; }
    public ulong PagesPointer { get; set; }

    public void Read(BinaryReader reader)
    {
        MeshInterfacePointer = reader.ReadUInt64();
        GeometryEpsilon = reader.ReadSingle();
        
        reader.Align(16);
        BoundsMin = reader.ReadVector4();
        BoundsMax = reader.ReadVector4();
        InvDiagonal = reader.ReadVector4();
        DiagonalScaler = reader.ReadVector4();
        PageSize = reader.ReadUInt32();
        RootPageCount = reader.ReadUInt32();
        LevelsCount = reader.ReadUInt32();
        TotalNodes = reader.ReadUInt32();
        TotalPages = reader.ReadUInt32();
        Flags = reader.ReadUInt32();
        Reserved = reader.ReadUInt32();
        
        reader.Align(16);
        PagesPointer = reader.ReadUInt64();
        reader.Align(16);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(MeshInterfacePointer);
        writer.Write(GeometryEpsilon);
        
        writer.Align(16, 0xCD);
        writer.WriteVector4(BoundsMin);
        writer.WriteVector4(BoundsMax);
        writer.WriteVector4(InvDiagonal);
        writer.WriteVector4(DiagonalScaler);
        writer.Write(PageSize);
        writer.Write(RootPageCount);
        writer.Write(LevelsCount);
        writer.Write(TotalNodes);
        writer.Write(TotalPages);
        writer.Write(Flags);
        writer.Write(Reserved);
        
        writer.Align(16, 0xCD);
        writer.Write(PagesPointer);
        writer.Align(16, 0xCD);
    }
}