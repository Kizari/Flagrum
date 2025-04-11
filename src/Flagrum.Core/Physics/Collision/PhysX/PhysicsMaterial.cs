using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX;

public class PhysicsMaterial : PhysicsBase
{
    public ulong UserDataPointer { get; set; }
    public ulong PhysicsMaterialPointer { get; set; }
    public uint MaterialIndex { get; set; }
    public float DynamicFriction { get; set; }
    public float StaticFriction { get; set; }
    public float Restitution { get; set; }
    public ushort Flags { get; set; }
    public byte FrictionRestitutionCombineMode { get; set; }
    public byte Padding { get; set; }
    public ulong UnknownPointer { get; set; }
    public uint MaybeReferenceCount { get; set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);

        UserDataPointer = reader.ReadUInt64();
        PhysicsMaterialPointer = reader.ReadUInt64();
        MaterialIndex = reader.ReadUInt32();
        
        reader.Align(16);
        DynamicFriction = reader.ReadSingle();
        StaticFriction = reader.ReadSingle();
        Restitution = reader.ReadSingle();
        Flags = reader.ReadUInt16();
        FrictionRestitutionCombineMode = reader.ReadByte();
        Padding = reader.ReadByte();
        UnknownPointer = reader.ReadUInt64();
        MaybeReferenceCount = reader.ReadUInt32();
    }

    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        
        writer.Write(UserDataPointer);
        writer.Write(PhysicsMaterialPointer);
        writer.Write(MaterialIndex);
        
        writer.Align(16, 0xCD);
        writer.Write(DynamicFriction);
        writer.Write(StaticFriction);
        writer.Write(Restitution);
        writer.Write(Flags);
        writer.Write(FrictionRestitutionCombineMode);
        writer.Write(Padding);
        writer.Write(UnknownPointer);
        writer.Write(MaybeReferenceCount);
    }
}