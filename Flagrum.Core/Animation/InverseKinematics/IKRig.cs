using System.Collections.Generic;
using ProtoBuf;

namespace Flagrum.Core.Animation.InverseKinematics;

public enum IKJointType
{
    JointTypeBallAndSocket = 0,
    JointTypeXAxisRotation = 1,
    JointTypeYAxisRotation = 2,
    JointTypeZAxisRotation = 3,
    JointTypeMinusZAxisRotation = 4
}

public enum IKPartType
{
    RightLegID = 0x0,
    LeftLegID = 0x1,
    RightArmID = 0x2,
    LeftArmID = 0x3,
    SpineID = 0x4,
    NeckID = 0x5,
    HeadID = 0x6,
    RootID = 0x7,
    TailID = 0x8,
    RightWingID = 0x9,
    LeftWingID = 0xA,
    RightForeLegID = 0xB,
    LeftForeLegID = 0xC,
    RightHindLegID = 0xD,
    LeftHindLegID = 0xE,
    EyeID = 0xF,
    OtherLimbID = 0x65
}

[ProtoContract]
public class IKRig
{
    [ProtoMember(1)] public IList<IKJoint> Joints { get; set; }
    [ProtoMember(2)] public IList<IKPart> Parts { get; set; }
    [ProtoMember(3)] public IList<IKDepthLayer> JointLayers { get; set; }
    [ProtoMember(4)] public uint RootJoint { get; set; }
    [ProtoMember(5)] public IList<IKLookingElement> LookingElements { get; set; }
}

[ProtoContract]
public class IKJoint
{
    [ProtoMember(1)] public IKJointType Type { get; set; }
    [ProtoMember(2)] public int ModelBoneId { get; set; }
    [ProtoMember(3)] public string ModelBoneName { get; set; }
    [ProtoMember(5)] public int Parent { get; set; }
    [ProtoMember(4)] public IList<int> NonIKChildrenIds { get; set; }
    [ProtoMember(6)] public IList<uint> Children { get; set; }
    [ProtoMember(7)] public IList<string> NonIKChildren { get; set; }
    [ProtoMember(8)] public IKFrame BindPose { get; set; }
}

[ProtoContract]
public class IKFrame
{
    [ProtoMember(1)] public IKVector3 Position { get; set; }
    [ProtoMember(2)] public IKQuaternion Rotation { get; set; }
}

[ProtoContract]
public class IKVector3
{
    [ProtoMember(1)] public float X { get; set; }
    [ProtoMember(2)] public float Y { get; set; }
    [ProtoMember(3)] public float Z { get; set; }
}

[ProtoContract]
public class IKQuaternion
{
    [ProtoMember(1)] public float X { get; set; }
    [ProtoMember(2)] public float Y { get; set; }
    [ProtoMember(3)] public float Z { get; set; }
    [ProtoMember(4)] public float W { get; set; }
}

[ProtoContract]
public class IKPart
{
    [ProtoMember(1)] public IKPartType Type { get; set; }
}

[ProtoContract]
public class IKDepthLayer
{
    [ProtoMember(1)] public IList<uint> Joints { get; set; }
}

[ProtoContract]
public class IKLookingElement
{
    [ProtoMember(1)] public IKJointFrame Head { get; set; }
    [ProtoMember(2)] public IList<IKJointFrame> Eyes { get; set; }
}

[ProtoContract]
public class IKJointFrame
{
    [ProtoMember(1)] public IKFrame Frame { get; set; }
    [ProtoMember(2)] public uint Joint { get; set; }
}