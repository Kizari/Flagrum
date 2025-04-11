namespace Flagrum.Core.Animation.Clip;

public enum AnimationCustomDataType
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