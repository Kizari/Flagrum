namespace Flagrum.Core.Animation.Clip;

public class AnimationRawTypeInfo
{
    public PackedKeyType PackedKeyType { get; set; }
    public ushort NumberInCache { get; set; }
    public ushort PoseStartBit { get; set; }
    public uint PoseDataStartOffset { get; set; }
    public float DecompressRangeScalar { get; set; }
}