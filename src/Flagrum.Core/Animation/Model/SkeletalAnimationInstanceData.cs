namespace Flagrum.Core.Animation.Model;

public class SkeletalAnimationInstanceData
{
    public AnimatedScaleState ScaleState { get; set; }
    public AnimationProcessCutdownMode ProcessCutdownMode { get; set; }
    public float RotationX { get; set; }
    public int RotationY { get; set; }
    public float RotationZ { get; set; }
}