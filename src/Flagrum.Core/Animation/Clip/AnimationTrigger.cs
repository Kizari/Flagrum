namespace Flagrum.Core.Animation.Clip;

public class AnimationTrigger
{
    public ushort TriggerFrame { get; set; }
    public ushort TypeAndMirror { get; set; }
    public TriggerTrackType TrackType { get; set; }
    public short CustomDataIndex { get; set; }
}