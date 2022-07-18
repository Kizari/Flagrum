namespace Flagrum.Core.Animation.AnimationClip;

public class AnimationRawTypeInfo
{
    public LmEPackedKeyType PackedKeyType { get; set; }
    public ushort NumberInCache { get; set; }
    public ushort PoseStartBit { get; set; }
    public uint PoseDataStartOffset { get; set; }
    public float DecompressRangeScalar { get; set; }
}