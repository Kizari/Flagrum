using System;

namespace Flagrum.Core.Animation.AnimationClip;

public enum LmETriggerTrackType : ushort
{
    eTriggerTrackType_LocomotionFeetEventTrack = 0x0,
    eTriggerTrackType_ExampleType_RangeBegin = 0x0B,
    eTriggerTrackType_ExampleType_RangeEnd = 0x14,
    eTriggerTrackType_LuminousMessageTriggerTrack_RangeBegin = 0x21,
    eTriggerTrackType_LuminousMessageTriggerTrack_RangeEnd = 0x100,

    //eTriggerTrackType_LuminousAssetTriggerTrack_RangeBegin = 0x1001,
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
    // 0-3 appear to be for Animation Model
    eCustomUserDataType_SkeletalAnimInfo = 0x0,
    eCustomUserDataType_PhysicsBoneInfo = 0x1,
    eCustomUserDataType_VertexCache_Deprecated = 0x2,

    //eCustomUserDataType_BeginSubAssetItemTypes = 0x3,
    eCustomUserDataType_LuminousTriggeredAssetItem_Generic = 0x3,

    // 100+ appears to be for Animation Clip
    //eCustomUserDataType_BeginSubAssetContainerTypes = 0x100,
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

public enum LmEPackedKeyType
{
    ePackedKeyType_Quat_40 = 0,
    ePackedKeyType_Quat_48 = 1,
    ePackedKeyType_Quat_48_Deprecated = 2,
    ePackedKeyType_Quat_128 = 3,
    ePackedKeyType_Vector3_48 = 4,
    ePackedKeyType_Vector3_48_NoScale = 5,
    ePackedKeyType_Vector3_128 = 6,
    ePackedKeyType_Vector3_128_NoScale = 7,
    ePackedKeyType_Num_PK_Types = 8
}

public static class AnimationKeys
{
    public static LmEPackedKeyType GetDefaultForRawType(LmERawType type)
    {
        return type switch
        {
            LmERawType.eRawType_Quaternion => LmEPackedKeyType.ePackedKeyType_Quat_48_Deprecated,
            LmERawType.eRawType_UncompressedVector3 => LmEPackedKeyType.ePackedKeyType_Vector3_128,
            LmERawType.eRawType_Vector3_Range0 => LmEPackedKeyType.ePackedKeyType_Vector3_48,
            LmERawType.eRawType_Vector3_Range1 => LmEPackedKeyType.ePackedKeyType_Vector3_48,
            LmERawType.eRawType_Vector3_Range2 => LmEPackedKeyType.ePackedKeyType_Vector3_48,
            LmERawType.eRawType_Vector3_NoScale => LmEPackedKeyType.ePackedKeyType_Vector3_48_NoScale,
            LmERawType.eRawType_Num_Raw_Types => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}