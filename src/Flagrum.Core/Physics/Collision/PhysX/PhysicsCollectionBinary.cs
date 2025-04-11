using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Flagrum.Core.Physics.Collision.PhysX.RTree;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsCollectionBinary
{
    public char[] Magic { get; set; } = "SEBD".ToCharArray();
    public ushort VersionMajor { get; set; } = 3 << 8;
    public byte VersionMinor { get; set; } = 3;
    public byte VersionPatch { get; set; } = 3;
    public uint BinaryVersion { get; set; }
    public uint BuildNumber { get; set; }
    public char[] PlatformTag { get; set; } = "W_64".ToCharArray();
    public uint MarkedPadding { get; set; } = 1;

    public uint ObjectCount { get; set; } = 2;
    public uint ManifestEntryCount { get; set; } = 2;
    public List<PhysicsCollectionManifestEntry> ManifestEntries { get; set; } = new();
    public uint DataEndOffset { get; set; }
    
    public uint ImportReferenceCount { get; set; }
    // TODO: Read references (not sure if XV ever uses this)

    public uint ExportReferenceCount { get; set; } = 2;
    public List<PhysicsCollectionExportReference> ExportReferences { get; set; } = new();

    public uint InternalPointerReferenceCount { get; set; } = 2;
    public List<PhysicsCollectionReference<ulong>> InternalPointerReferences { get; set; } = new();
    
    public uint InternalIndexReferenceCount { get; set; }
    public List<PhysicsCollectionReference<uint>> InternalIndexReferences { get; set; } = new();

    public ulong UnknownPointer { get; set; } = 305419896;
    public PhysicsMaterial Material { get; set; } = new();

    public ulong UnknownPointer2 { get; set; } = 305419896;
    public PhysicsTriangleMesh Mesh { get; set; } = new();

    public List<RTreePage> TreePages { get; set; } = new();
    public List<Vector3> Vertices { get; set; } = new();
    public ushort[] TriangleIndices { get; set; }
    public byte[] ExtraData2 { get; set; }
    public ushort[] MaterialIndices { get; set; }
    public uint[] FaceRemap { get; set; }

    public void Read(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        Magic = reader.ReadChars(4);
        VersionMajor = reader.ReadUInt16();
        VersionMinor = reader.ReadByte();
        VersionPatch = reader.ReadByte();
        BinaryVersion = reader.ReadUInt32();
        BuildNumber = reader.ReadUInt32();
        PlatformTag = reader.ReadChars(4);
        MarkedPadding = reader.ReadUInt32();
        
        reader.Align(16);
        ObjectCount = reader.ReadUInt32();
        
        reader.Align(16);
        ManifestEntryCount = reader.ReadUInt32();
        for (var i = 0; i < ManifestEntryCount; i++)
        {
            var entry = new PhysicsCollectionManifestEntry();
            entry.Read(reader);
            ManifestEntries.Add(entry);
        }

        DataEndOffset = reader.ReadUInt32();
        
        reader.Align(16);
        ImportReferenceCount = reader.ReadUInt32();
        
        reader.Align(16);
        ExportReferenceCount = reader.ReadUInt32();
        for (var i = 0; i < ExportReferenceCount; i++)
        {
            var reference = new PhysicsCollectionExportReference();
            reference.Read(reader);
            ExportReferences.Add(reference);
        }
        
        reader.Align(16);
        InternalPointerReferenceCount = reader.ReadUInt32();
        for (var i = 0; i < InternalPointerReferenceCount; i++)
        {
            var reference = new PhysicsCollectionReference<ulong>();
            reference.Read(reader);
            InternalPointerReferences.Add(reference);
        }

        InternalIndexReferenceCount = reader.ReadUInt32();
        for (var i = 0; i < InternalIndexReferenceCount; i++)
        {
            var reference = new PhysicsCollectionReference<uint>();
            reference.Read(reader);
            InternalIndexReferences.Add(reference);
        }
        
        reader.Align(16);
        UnknownPointer = reader.ReadUInt64();
        Material.Read(reader);
        
        reader.Align(16);
        UnknownPointer2 = reader.ReadUInt64();
        Mesh.Read(reader);

        for (var i = 0; i < Mesh.CollisionModel.TotalPages; i++)
        {
            var page = new RTreePage();
            page.Read(reader);
            TreePages.Add(page);
        }

        for (var i = 0; i < Mesh.VertexCount; i++)
        {
            Vertices.Add(reader.ReadVector3());
        }

        reader.Align(16);
        TriangleIndices = new ushort[Mesh.TriangleCount * 3];
        for (var i = 0; i < TriangleIndices.Length; i++)
        {
            TriangleIndices[i] = reader.ReadUInt16();
        }

        reader.Align(16);
        ExtraData2 = new byte[Mesh.TriangleCount];
        for (var i = 0; i < Mesh.TriangleCount; i++)
        {
            ExtraData2[i] = reader.ReadByte();
        }

        reader.Align(16);
        MaterialIndices = new ushort[Mesh.TriangleCount];
        for (var i = 0; i < Mesh.TriangleCount; i++)
        {
            MaterialIndices[i] = reader.ReadUInt16();
        }

        reader.Align(16);
        FaceRemap = new uint[Mesh.TriangleCount];
        for (var i = 0; i < Mesh.TriangleCount; i++)
        {
            FaceRemap[i] = reader.ReadUInt32();
        }
    }

    public byte[] Write()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        
        writer.Write(Magic);
        writer.Write(VersionMajor);
        writer.Write(VersionMinor);
        writer.Write(VersionPatch);
        writer.Write(BinaryVersion);
        writer.Write(BuildNumber);
        writer.Write(PlatformTag);
        writer.Write(MarkedPadding);
        
        writer.Align(16, 0x42);
        writer.Write(ObjectCount);
        
        writer.Align(16, 0x42);
        writer.Write(ManifestEntryCount);

        foreach (var manifestEntry in ManifestEntries)
        {
            manifestEntry.Write(writer);
        }

        var dataEndOffsetAddress = stream.Position;
        writer.Write(DataEndOffset);
        
        writer.Align(16, 0x42);
        writer.Write(ImportReferenceCount);
        
        writer.Align(16, 0x42);
        writer.Write(ExportReferenceCount);
        foreach (var reference in ExportReferences)
        {
            reference.Write(writer);
        }
        
        writer.Align(16, 0x42);
        writer.Write(InternalPointerReferenceCount);
        foreach (var reference in InternalPointerReferences)
        {
            reference.Write(writer);
        }
        
        writer.Write(InternalIndexReferenceCount);
        foreach (var reference in InternalIndexReferences)
        {
            reference.Write(writer);
        }
        
        writer.Align(16, 0x42);

        var dataStart = stream.Position;
        writer.Write(UnknownPointer);
        
        Material.Write(writer);
        
        writer.Align(16, 0xCD);
        writer.Write(UnknownPointer2);
        
        Mesh.Write(writer);

        var returnAddress = stream.Position;
        DataEndOffset = (uint)(stream.Position - dataStart);
        stream.Seek(dataEndOffsetAddress, SeekOrigin.Begin);
        writer.Write(DataEndOffset);
        stream.Seek(returnAddress, SeekOrigin.Begin);
        
        writer.Align(128, 0x42);

        foreach (var page in TreePages)
        {
            page.Write(writer);
        }
        
        foreach (var value in Vertices)
        {
            writer.WriteVector3(value);
        }
        
        writer.Align(16, 0x42);
        foreach (var value in TriangleIndices)
        {
            writer.Write(value);
        }
        
        writer.Align(16, 0x42);
        foreach (var value in ExtraData2)
        {
            writer.Write(value);
        }
        
        writer.Align(16, 0x42);
        foreach (var value in MaterialIndices)
        {
            writer.Write(value);
        }
        
        writer.Align(16, 0x42);
        foreach (var value in FaceRemap)
        {
            writer.Write(value);
        }

        return stream.ToArray();
    }
}