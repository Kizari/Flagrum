using System.IO;
using System.Numerics;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision;

public class PhysicsShape
{
    public PhysicsShapeType Type { get; set; }
    public int NameOffset { get; set; }
    public int BoneNameOffset { get; set; }
    public int UserStringOffset { get; set; }
    public Vector3 Translation { get; set; }
    public Vector3 Rotation { get; set; }
    public float Friction { get; set; }
    public float Restitution { get; set; }
    public float Coefficient { get; set; }
    public Vector3 CenterPivot { get; set; }

    public void Read(BinaryReader reader)
    {
        Type = (PhysicsShapeType)reader.ReadUInt32();
        NameOffset = reader.ReadInt32();
        BoneNameOffset = reader.ReadInt32();
        UserStringOffset = reader.ReadInt32();
        Translation = reader.ReadVector3();
        Rotation = reader.ReadVector3();
        Friction = reader.ReadSingle();
        Restitution = reader.ReadSingle();
        Coefficient = reader.ReadSingle();
        CenterPivot = reader.ReadVector3();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint)Type);
        writer.Write(NameOffset);
        writer.Write(BoneNameOffset);
        writer.Write(UserStringOffset);
        writer.WriteVector3(Translation);
        writer.WriteVector3(Rotation);
        writer.Write(Friction);
        writer.Write(Restitution);
        writer.Write(Coefficient);
        writer.WriteVector3(CenterPivot);
    }
}