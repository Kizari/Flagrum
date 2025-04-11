namespace Flagrum.Core.Animation.Model;

public class PoseSpecification
{
    public uint PoseSize { get; set; }
    public uint RawTypesCount { get; set; }
    public uint RawDataInfoOffset { get; set; }
    public uint PartsSizeBlocks { get; set; }
    public uint PostProcessesCount { get; set; }
    public int Reserved { get; set; }
    public AnimationDataChannelInformation[] Channels { get; set; }
    public ulong SkeletalBoneNamesOffset { get; set; }
    public uint SkeletalPartNamesCount { get; set; }
    public uint PartsLayersCount { get; set; }
    public uint TotalParts { get; set; }
}