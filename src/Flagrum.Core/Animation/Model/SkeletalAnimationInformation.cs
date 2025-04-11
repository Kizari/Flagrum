using System.Numerics;
using Quaternion = Flagrum.Core.Mathematics.Quaternion;

namespace Flagrum.Core.Animation.Model;

public class SkeletalAnimationInformation
{
    public uint BoneCount { get; set; }
    public uint BoneCountFullSkeleton { get; set; }
    public ushort BoneCountBindOnly { get; set; }
    public ushort AfterBonamikStartIndex { get; set; }
    public ushort KdBonesStartIndex { get; set; }
    public ushort Dummy { get; set; }
    public short[] ChildInfoOffset { get; set; }
    public ushort[] UnknownIndices { get; set; }
    public short[] ParentIndices { get; set; }
    public short[] MaybeParentIndexOffsets { get; set; }
    public ushort[] UnknownIndices2 { get; set; }
    public uint[] UnknownIndices3 { get; set; }
    public Quaternion[] UnknownRotations { get; set; }
    public Quaternion[] UnknownRotations2 { get; set; }
    public Matrix4x4[] Transformations { get; set; }
    public Matrix4x4[] UnknownTransformations { get; set; }
}