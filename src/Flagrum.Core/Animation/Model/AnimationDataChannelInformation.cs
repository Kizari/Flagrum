namespace Flagrum.Core.Animation.Model;

public class AnimationDataChannelInformation
{
    public uint EntriesCount { get; set; }
    public uint DataOffset { get; set; }
    public ulong ModelTypeSpecificInfoOffset { get; set; }
    public uint PartMaskStartBitIndex { get; set; }
}