using System.IO;

namespace Flagrum.Core.Animation;

public enum LmETriggerTrackType : ushort
{
    eTriggerTrackType_LocomotionFeetEventTrack = 0x0,
    eTriggerTrackType_ExampleType_RangeBegin = 0x0B,
    eTriggerTrackType_ExampleType_RangeEnd = 0x14,
    eTriggerTrackType_LuminousMessageTriggerTrack_RangeBegin = 0x21,
    eTriggerTrackType_LuminousMessageTriggerTrack_RangeEnd = 0x100,
    eTriggerTrackType_LuminousAssetTriggerTrack_RangeBegin = 0x1001,
    eTriggerTrackType_LuminousAssetTriggerTrack_Anonymous_RangeBegin = 0x1001,
    eTriggerTrackType_LuminousAssetTriggerTrack_Anonymous_RangeEnd = 0x1100,
    eTriggerTrackType_LuminousAssetTriggerTrack_InModel_RangeBegin = 0x1101,
    eTriggerTrackType_LuminousAssetTriggerTrack_InModel_RangeEnd = 0x1200,
    eTriggerTrackType_LuminousAssetTriggerTrack_ModelDrawable_RangeBegin = 0x1201,
    eTriggerTrackType_LuminousAssetTriggerTrack_ModelDrawable_RangeEnd = 0x1300,
    eTriggerTrackType_LuminousAssetTriggerTrack_Collision_RangeBegin = 0x1301,
    eTriggerTrackType_LuminousAssetTriggerTrack_Collision_RangeEnd = 0x1400,
    eTriggerTrackType_LuminousAssetTriggerTrack_VFX_RangeBegin = 0x1401,
    eTriggerTrackType_LuminousAssetTriggerTrack_VFX_RangeEnd = 0x1500,
    eTriggerTrackType_LuminousAssetTriggerTrack_Sound_RangeBegin = 0x1501,
    eTriggerTrackType_LuminousAssetTriggerTrack_Sound_RangeEnd = 0x1600,
    eTriggerTrackType_LuminousAssetTriggerTrack_Light_RangeBegin = 0x1601,
    eTriggerTrackType_LuminousAssetTriggerTrack_Light_RangeEnd = 0x1700,
    eTriggerTrackType_LuminousAssetTriggerTrack_Camera_RangeBegin = 0x1701,
    eTriggerTrackType_LuminousAssetTriggerTrack_Camera_RangeEnd = 0x1800,
    eTriggerTrackType_LuminousAssetTriggerTrack_Scaleform_RangeBegin = 0x1801,
    eTriggerTrackType_LuminousAssetTriggerTrack_Scaleform_RangeEnd = 0x1900,
    eTriggerTrackType_RequireTrackTypeMatchForRangeEventMatch_RangeEnd = 0x1FFF,
    eTriggerTrackType_BlackAssetTriggerTrack_General_RangeBegin = 0x2001,
    eTriggerTrackType_BlackAssetTriggerTrack_General_RangeEnd = 0x2100,
    eTriggerTrackType_BlackAssetTriggerTrack_VFX_RangeBegin = 0x2101,
    eTriggerTrackType_BlackAssetTriggerTrack_VFX_RangeEnd = 0x2200,
    eTriggerTrackType_BlackAssetTriggerTrack_Sound_RangeBegin = 0x2201,
    eTriggerTrackType_BlackAssetTriggerTrack_Sound_RangeEnd = 0x2300,
    eTriggerTrackType_BlackAssetTriggerTrack_Combat_RangeBegin = 0x2301,
    eTriggerTrackType_BlackAssetTriggerTrack_Combat_RangeEnd = 0x2400,
    eTriggerTrackType_BlackAssetTriggerTrack_Facial_RangeBegin = 0x2401,
    eTriggerTrackType_BlackAssetTriggerTrack_Facial_RangeEnd = 0x2500,
    eTriggerTrackType_AnyTypeEnumIdx = 0x0FFFE,
    eTriggerTrackType_Unknown = 0x0FFFF
}

public enum LmEAnimCustomDataType
{
    eCustomUserDataType_SkeletalAnimInfo = 0x0,
    eCustomUserDataType_PhysicsBoneInfo = 0x1,
    eCustomUserDataType_VertexCache_Deprecated = 0x2,
    eCustomUserDataType_BeginSubAssetItemTypes = 0x3,
    eCustomUserDataType_LuminousTriggeredAssetItem_Generic = 0x3,
    eCustomUserDataType_BeginSubAssetContainerTypes = 0x100,
    eCustomUserDataType_LuminousAssetTriggerDataCollection = 0x100,
    eCustomUserDataType_EndSubAssetContainerTypes = 0x101,
    eCustomUserDataType_Obsolete_LuminousIK_ModelData = 0x102,
    eCustomUserDataType_BlackTriggerData = 0x103,
    eCustomUserDataType_OBSOLETE_Sasquatch_ModelData = 0x104,
    eCustomUserDataType_OBSOLETE_Sasquatch_AnimData = 0x105,
    eCustomUserDataType_PackedTypeOverrideList = 0x106,
    eCustomUserDataType_NoMirrorParts = 0x107,
    eCustomUserDataType_AssetRefContainer_SystemInternalUseOnly = 0x10000,
    eCustomUserDataType_Obsolete_LuminousWalker_ModelData = 0x10001,
    eCustomUserDataType_Obsolete_LuminousWalker_AnimData = 0x10002,
    eCustomUserDataType_LuminousMessageTriggerData = 0x10003,
    eCustomUserDataType_LuminousAssetTriggerData = 0x10004,

    //eCustomUserDataType_BeginBuildCoordinatorDependencyTypes = 0x20000,
    eCustomUserDataType_LuminousIkRig = 0x20000,
    eCustomUserDataType_LuminousTuningSetPack = 0x20001,
    eCustomUserDataType_LuminousLinkAnimScenePath = 0x20002,
    eCustomUserDataType_VertexCache = 0x20003,
    eCustomUserDataType_EndBuildCoordinatorDependencyTypes = 0x20004,
    eCustomUserDataType_NumCustomDataTypes = 0x20005
}

public class AnimationClip
{
    public float DurationSeconds { get; set; }
    public int Id { get; set; }
    public uint Properties { get; set; }
    public uint KeyframeFps { get; set; }
    public float Version { get; set; }
    public uint CacheTypesCount { get; set; }
    public uint PartsSizeBlocksCount { get; set; }
    public short UsersCount { get; set; }
    public short PlayCount { get; set; }

    public ulong ConstantDataOffset { get; set; }
    public ulong FrameDataChunkStartPointerArrayOffset { get; set; }

    public byte[] FirstBlock { get; set; }

    public ulong Unknown { get; set; }

    public byte[] SecondBlock { get; set; }

    public static AnimationClip FromData(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var clip = new AnimationClip
        {
            DurationSeconds = reader.ReadSingle(),
            Id = reader.ReadInt32(),
            Properties = reader.ReadUInt32(),
            KeyframeFps = reader.ReadUInt32(),
            Version = reader.ReadSingle(),
            CacheTypesCount = reader.ReadUInt32(),
            PartsSizeBlocksCount = reader.ReadUInt32(),
            UsersCount = reader.ReadInt16(),
            PlayCount = reader.ReadInt16(),
            ConstantDataOffset = reader.ReadUInt64(),
            FrameDataChunkStartPointerArrayOffset = reader.ReadUInt64()
        };

        var firstBlockSize = clip.FrameDataChunkStartPointerArrayOffset - (ulong)stream.Position - 8;
        clip.FirstBlock = new byte[firstBlockSize];
        reader.Read(clip.FirstBlock);

        stream.Seek(6, SeekOrigin.Current);

        var number = reader.ReadUInt16();
        clip.Unknown = number;

        var secondBlockSize = stream.Length - stream.Position;
        clip.SecondBlock = new byte[secondBlockSize];
        reader.Read(clip.SecondBlock);

        return clip;
    }

    public static byte[] ToData(AnimationClip clip)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(clip.DurationSeconds);
        writer.Write(clip.Id);
        writer.Write(clip.Properties);
        writer.Write(clip.KeyframeFps);
        writer.Write(clip.Version);
        writer.Write(clip.CacheTypesCount);
        writer.Write(clip.PartsSizeBlocksCount);
        writer.Write(clip.UsersCount);
        writer.Write(clip.PlayCount);
        writer.Write(clip.ConstantDataOffset);
        writer.Write(clip.FrameDataChunkStartPointerArrayOffset);
        writer.Write(clip.FirstBlock);
        writer.Write(clip.Unknown);
        writer.Write(clip.SecondBlock);

        return stream.ToArray();
    }
}