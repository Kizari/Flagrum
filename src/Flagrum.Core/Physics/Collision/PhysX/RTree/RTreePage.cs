using System.IO;
using System.Numerics;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTreePage
{
    public Vector4 MinX;
    public Vector4 MinY;
    public Vector4 MinZ;
    public Vector4 MaxX;
    public Vector4 MaxY;
    public Vector4 MaxZ;
    public uint[] Pointers { get; set; } = new uint[4];

    public void Read(BinaryReader reader)
    {
        MinX = reader.ReadVector4();
        MinY = reader.ReadVector4();
        MinZ = reader.ReadVector4();
        MaxX = reader.ReadVector4();
        MaxY = reader.ReadVector4();
        MaxZ = reader.ReadVector4();
        
        for (var i = 0; i < 4; i++)
        {
            Pointers[i] = reader.ReadUInt32();
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.WriteVector4(MinX);
        writer.WriteVector4(MinY);
        writer.WriteVector4(MinZ);
        writer.WriteVector4(MaxX);
        writer.WriteVector4(MaxY);
        writer.WriteVector4(MaxZ);

        foreach (var pointer in Pointers)
        {
            writer.Write(pointer);
        }
    }
}