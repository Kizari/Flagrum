namespace Flagrum.Core.Animation.AnimationClip;

public class AnimationTrigger
{
    public ushort TriggerFrame { get; set; }
    public ushort TypeAndMirror { get; set; }
    public LmETriggerTrackType TrackType { get; set; }
    public short CustomDataIndex { get; set; }
}