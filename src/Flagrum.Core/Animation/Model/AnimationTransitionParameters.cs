namespace Flagrum.Core.Animation.Model;

public class AnimationTransitionParameters
{
    public float BlendTime { get; set; }
    public float PlaybackRate { get; set; }
    public float StartTime { get; set; }
    public uint Flags { get; set; }
    public uint UseCount { get; set; }
}