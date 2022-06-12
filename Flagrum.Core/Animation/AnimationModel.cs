using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Animation;

public enum LmEAnimatedScaleState : ushort
{
    eAnimatedScaleState_Inactive = 0,
    eAnimatedScaleState_Scale_XSI_GS_Data_Playing = 1,
    eAnimatedScaleState_Scale_XSI_GS_Data_Playing_Force = 2
}

public enum LmERawType
{
    eRawType_Quaternion = 0,
    eRawType_UncompressedVector3 = 1,
    eRawType_Vector3_Range0 = 2,
    eRawType_Vector3_Range1 = 3,
    eRawType_Vector3_Range2 = 4,
    eRawType_Vector3_NoScale = 5,
    eRawType_Num_Raw_Types = 6
}

public enum LmEAnimProcessCutdownMode : ushort
{
    None = 0,
    Entity = 1,
    Camera = 2,
    Menu = 3,
    Swf = 4,
    Actor = 5
}

public class LmSDataChannelInfo
{
    public uint EntriesCount { get; set; }
    public uint DataOffset { get; set; }
    public ulong ModelTypeSpecificInfoOffset { get; set; }
    public uint PartMaskStartBitIndex { get; set; }
}

public class LmPoseSpec
{
    public uint PoseSize { get; set; }
    public uint RawTypesCount { get; set; }
    public uint RawDataInfoOffset { get; set; }
    public uint PartsSizeBlocks { get; set; }
    public uint PostProcessesCount { get; set; }
    public int Reserved { get; set; }
    public LmSDataChannelInfo[] Channels { get; set; }
    public ulong SkeletalBoneNamesOffset { get; set; }
    public uint SkeletalPartNamesCount { get; set; }
    public uint PartsLayersCount { get; set; }
    public uint TotalParts { get; set; }
}

public class PartsData
{
    public uint[] PartsLayerIds { get; set; }
    public List<ulong[]> PartsIds { get; set; } = new();
}

public class Quaternion
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }
}

public class LmSRawDataInfo
{
    public LmERawType Type { get; set; }
    public uint EntriesCount { get; set; }
    public Quaternion[] Values { get; set; }
}

public class LmPoseData
{
    public LmSRawDataInfo[] RawDataInfos { get; set; }
    public ulong Offset { get; set; }
}

public class LmSCustomUserDataIndexNode
{
    public ulong Offset { get; set; }
    public LmEAnimCustomDataType Type { get; set; }
}

public class CustomUserData
{
    public LmSCustomUserDataIndexNode[] CustomUserDataIndexNodes { get; set; }
    public object[] CustomUserDatas { get; set; }
}

public class LmSkeletalAnimInstanceData
{
    public LmEAnimatedScaleState ScaleState { get; set; }
    public LmEAnimProcessCutdownMode ProcessCutdownMode { get; set; }
    public float RotationX { get; set; }
    public int RotationY { get; set; }
    public float RotationZ { get; set; }
}

public class LmSTransitionParams
{
    public float BlendTime { get; set; }
    public float PlaybackRate { get; set; }
    public float StartTime { get; set; }
    public uint Flags { get; set; }
    public uint UseCount { get; set; }
}

public class LmMatrix
{
    public float[] Values { get; set; } = new float[16];
}

public class LmSkeletalAnimInfo
{
    public uint BoneCount { get; set; }
    public uint BoneCountFullSkeleton { get; set; }
    public ushort BoneCountBindOnly { get; set; }
    public ushort AfterBonamikStartIndex { get; set; }
    public ushort KdBonesStartIndex { get; set; }
    public ushort Dummy { get; set; }
    public short[] ChildInfoOffset { get; set; }
    public ushort[] UnknownIndices { get; set; }
    public short[] ParentIndices { get; set; }
    public short[] MaybeParentIndexOffsets { get; set; }
    public ushort[] UnknownIndices2 { get; set; }
    public uint[] UnknownIndices3 { get; set; }
    public Quaternion[] UnknownRotations { get; set; }
    public Quaternion[] UnknownRotations2 { get; set; }
    public LmMatrix[] Transformations { get; set; }
    public LmMatrix[] UnknownTransformations { get; set; }
}

public class DependencyLink
{
    public ulong Hash { get; set; }
    public string Uri { get; set; }
}

public class AnimationModel
{
    public uint FileSize { get; set; }
    public float Version { get; set; }
    public uint BindPoseOffset { get; set; }
    public uint CustomUserDataCount { get; set; }
    public ulong CustomUserDataOffset { get; set; }
    public ulong DefaultInstanceDataFileBinOffset { get; set; }
    public ulong TransitionsOffset { get; set; }
    public ulong DefaultTransitionOffset { get; set; }
    public uint TransitionCount { get; set; }
    public uint InstanceDataTypesCount { get; set; }
    public uint TotalInstanceDataSize { get; set; }
    public int[] InstanceDataOffsets { get; set; }
    public int ActiveInstances { get; set; }
    public LmPoseSpec PoseSpec { get; set; }
    public ushort[] Indices1 { get; set; }
    public ushort[] Indices2 { get; set; }
    public ushort[] Indices3 { get; set; }
    public PartsData PartsData { get; set; }
    public LmPoseData PoseData { get; set; }
    public string[] BoneNames { get; set; }
    public CustomUserData CustomUserData { get; set; }
    public LmSkeletalAnimInstanceData DefaultInstanceData { get; set; }
    public LmSTransitionParams[] Transitions { get; set; }

    public static AnimationModel FromData(byte[] data, bool isPs4)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var model = new AnimationModel
        {
            FileSize = reader.ReadUInt32(),
            Version = reader.ReadSingle(),
            BindPoseOffset = reader.ReadUInt32(),
            CustomUserDataCount = reader.ReadUInt32(),
            CustomUserDataOffset = reader.ReadUInt64(),
            DefaultInstanceDataFileBinOffset = reader.ReadUInt64(),
            TransitionsOffset = reader.ReadUInt64(),
            DefaultTransitionOffset = reader.ReadUInt64(),
            TransitionCount = reader.ReadUInt32(),
            InstanceDataTypesCount = reader.ReadUInt32(),
            TotalInstanceDataSize = reader.ReadUInt32()
        };

        model.InstanceDataOffsets = new int[model.InstanceDataTypesCount];
        for (var i = 0; i < model.InstanceDataTypesCount; i++)
        {
            model.InstanceDataOffsets[i] = reader.ReadInt32();
        }

        model.ActiveInstances = reader.ReadInt32();
        stream.Align(56);

        model.PoseSpec = new LmPoseSpec
        {
            PoseSize = reader.ReadUInt32(),
            RawTypesCount = reader.ReadUInt32(),
            RawDataInfoOffset = reader.ReadUInt32(),
            PartsSizeBlocks = reader.ReadUInt32()
        };

        stream.Seek(24, SeekOrigin.Current); // Padding
        model.PoseSpec.PostProcessesCount = reader.ReadUInt32();
        model.PoseSpec.Reserved = reader.ReadInt32();
        stream.Align(120);

        if (isPs4)
        {
            // PS4 amdl seems to have an extra 40 bytes of padding here
            stream.Seek(40, SeekOrigin.Current);
        }

        model.PoseSpec.Channels = new LmSDataChannelInfo[9];
        for (var i = 0; i < 9; i++)
        {
            model.PoseSpec.Channels[i] = new LmSDataChannelInfo
            {
                EntriesCount = reader.ReadUInt32(),
                DataOffset = reader.ReadUInt32(),
                ModelTypeSpecificInfoOffset = reader.ReadUInt64(),
                PartMaskStartBitIndex = reader.ReadUInt32()
            };

            stream.Align(8);
        }

        model.PoseSpec.SkeletalBoneNamesOffset = reader.ReadUInt64();
        model.PoseSpec.SkeletalPartNamesCount = reader.ReadUInt32();
        model.PoseSpec.PartsLayersCount = reader.ReadUInt32();
        model.PoseSpec.TotalParts = reader.ReadUInt32();

        var indicesOffset = stream.Position;

        stream.Seek(model.PoseSpec.RawDataInfoOffset + 112, SeekOrigin.Begin);

        model.PoseData = new LmPoseData {RawDataInfos = new LmSRawDataInfo[model.PoseSpec.RawTypesCount]};
        for (var i = 0; i < model.PoseSpec.RawTypesCount; i++)
        {
            model.PoseData.RawDataInfos[i] = new LmSRawDataInfo
            {
                Type = (LmERawType)reader.ReadInt32(),
                EntriesCount = reader.ReadUInt32()
            };
        }

        stream.Align(16);
        model.PoseData.Offset = reader.ReadUInt64();
        stream.Align(16);

        for (var i = 0; i < model.PoseSpec.RawTypesCount; i++)
        {
            var item = model.PoseData.RawDataInfos[i];
            item.Values = new Quaternion[item.EntriesCount];
            for (var j = 0; j < item.EntriesCount; j++)
            {
                item.Values[j] = new Quaternion
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle(),
                    W = reader.ReadSingle()
                };
            }
        }

        model.BoneNames = new string[model.PoseSpec.SkeletalPartNamesCount];
        for (var i = 0; i < model.PoseSpec.SkeletalPartNamesCount; i++)
        {
            model.BoneNames[i] = reader.ReadNullTerminatedString();
            stream.Seek(48 - model.BoneNames[i].Length - 1, SeekOrigin.Current);
        }

        var start = stream.Position;
        model.CustomUserData = new CustomUserData
        {
            CustomUserDataIndexNodes = new LmSCustomUserDataIndexNode[model.CustomUserDataCount],
            CustomUserDatas = new object[model.CustomUserDataCount]
        };

        for (var i = 0; i < model.CustomUserDataCount; i++)
        {
            model.CustomUserData.CustomUserDataIndexNodes[i] = new LmSCustomUserDataIndexNode
            {
                Offset = reader.ReadUInt64(),
                Type = (LmEAnimCustomDataType)reader.ReadInt32()
            };

            stream.Align(8);
        }

        for (var i = 0; i < model.CustomUserDataCount; i++)
        {
            var node = model.CustomUserData.CustomUserDataIndexNodes[i];
            stream.Seek(start + (long)node.Offset, SeekOrigin.Begin);

            switch (node.Type)
            {
                case LmEAnimCustomDataType.eCustomUserDataType_SkeletalAnimInfo:
                    var info = new LmSkeletalAnimInfo
                    {
                        BoneCount = reader.ReadUInt32(),
                        BoneCountFullSkeleton = reader.ReadUInt32(),
                        BoneCountBindOnly = reader.ReadUInt16(),
                        AfterBonamikStartIndex = reader.ReadUInt16(),
                        KdBonesStartIndex = reader.ReadUInt16(),
                        Dummy = reader.ReadUInt16()
                    };

                    info.ChildInfoOffset = new short[info.BoneCount];
                    for (var j = 0; j < info.BoneCount; j++)
                    {
                        info.ChildInfoOffset[j] = reader.ReadInt16();
                    }

                    info.UnknownIndices = new ushort[info.BoneCount];
                    for (var j = 0; j < info.BoneCount; j++)
                    {
                        info.UnknownIndices[j] = reader.ReadUInt16();
                    }

                    info.ParentIndices = new short[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.ParentIndices[j] = reader.ReadInt16();
                    }

                    info.MaybeParentIndexOffsets = new short[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.MaybeParentIndexOffsets[j] = reader.ReadInt16();
                    }

                    info.UnknownIndices2 = new ushort[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.UnknownIndices2[j] = reader.ReadUInt16();
                    }

                    info.UnknownIndices3 = new uint[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.UnknownIndices3[j] = reader.ReadUInt32();
                    }

                    stream.Align(16);

                    info.UnknownRotations = new Quaternion[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.UnknownRotations[j] = new Quaternion
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle(),
                            W = reader.ReadSingle()
                        };
                    }

                    info.UnknownRotations2 = new Quaternion[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.UnknownRotations2[j] = new Quaternion
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle(),
                            W = reader.ReadSingle()
                        };
                    }

                    info.Transformations = new LmMatrix[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.Transformations[j] = new LmMatrix();
                        for (var k = 0; k < 16; k++)
                        {
                            info.Transformations[j].Values[k] = reader.ReadSingle();
                        }
                    }

                    var finalMatricesCount = info.BoneCountBindOnly - info.BoneCountFullSkeleton;

                    info.UnknownTransformations = new LmMatrix[finalMatricesCount];
                    for (var j = 0; j < finalMatricesCount; j++)
                    {
                        info.UnknownTransformations[j] = new LmMatrix();
                        for (var k = 0; k < 16; k++)
                        {
                            info.UnknownTransformations[j].Values[k] = reader.ReadSingle();
                        }
                    }

                    model.CustomUserData.CustomUserDatas[i] = info;
                    break;
                case LmEAnimCustomDataType.eCustomUserDataType_LuminousIkRig:
                    var dependency = new DependencyLink
                    {
                        Hash = reader.ReadUInt64(),
                        Uri = reader.ReadNullTerminatedString()
                    };

                    stream.Align(8);
                    _ = stream.ReadByte(); // Guard byte
                    stream.Align(16);

                    model.CustomUserData.CustomUserDatas[i] = dependency;
                    break;
                case LmEAnimCustomDataType.eCustomUserDataType_LuminousTuningSetPack:
                    var tspack = new DependencyLink
                    {
                        Hash = reader.ReadUInt64(),
                        Uri = reader.ReadNullTerminatedString()
                    };

                    stream.Align(8);
                    _ = stream.ReadByte(); // Guard byte

                    model.CustomUserData.CustomUserDatas[i] = tspack;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unhandled LmEAnimCustomDataType {node.Type}");
            }
        }

        model.DefaultInstanceData = new LmSkeletalAnimInstanceData
        {
            ScaleState = (LmEAnimatedScaleState)reader.ReadUInt16(),
            ProcessCutdownMode = (LmEAnimProcessCutdownMode)reader.ReadUInt16(),
            RotationX = reader.ReadSingle(),
            RotationY = reader.ReadInt32(),
            RotationZ = reader.ReadSingle()
        };

        model.Transitions = new LmSTransitionParams[model.TransitionCount];
        for (var i = 0; i < model.TransitionCount; i++)
        {
            model.Transitions[i] = new LmSTransitionParams
            {
                BlendTime = reader.ReadSingle(),
                PlaybackRate = reader.ReadSingle(),
                StartTime = reader.ReadSingle(),
                Flags = reader.ReadUInt32(),
                UseCount = reader.ReadUInt32()
            };
        }

        stream.Seek(indicesOffset, SeekOrigin.Begin);

        var index = 0;
        for (var i = 0; i < model.CustomUserData.CustomUserDataIndexNodes.Length; i++)
        {
            if (model.CustomUserData.CustomUserDataIndexNodes[i].Type ==
                LmEAnimCustomDataType.eCustomUserDataType_SkeletalAnimInfo)
            {
                index = i;
                break;
            }
        }

        var skeletalAnimInfo = (LmSkeletalAnimInfo)model.CustomUserData.CustomUserDatas[index];

        model.Indices1 = new ushort[skeletalAnimInfo.BoneCount + 1];
        for (var i = 0; i <= skeletalAnimInfo.BoneCount; i++)
        {
            model.Indices1[i] = reader.ReadUInt16();
        }

        model.Indices2 = new ushort[skeletalAnimInfo.BoneCount + 1];
        for (var i = 0; i <= skeletalAnimInfo.BoneCount; i++)
        {
            model.Indices2[i] = reader.ReadUInt16();
        }

        model.Indices3 = new ushort[skeletalAnimInfo.BoneCount];
        for (var i = 0; i < skeletalAnimInfo.BoneCount; i++)
        {
            model.Indices3[i] = reader.ReadUInt16();
        }

        model.PartsData = new PartsData {PartsLayerIds = new uint[model.PoseSpec.PartsLayersCount]};
        for (var i = 0; i < model.PoseSpec.PartsLayersCount; i++)
        {
            model.PartsData.PartsLayerIds[i] = reader.ReadUInt32();
        }

        stream.Align(8);

        for (var i = 0; i < model.PoseSpec.PartsLayersCount; i++)
        {
            model.PartsData.PartsIds.Add(new ulong[model.PoseSpec.PartsSizeBlocks]);
            for (var j = 0; j < model.PoseSpec.PartsSizeBlocks; j++)
            {
                model.PartsData.PartsIds[i][j] = reader.ReadUInt64();
            }
        }

        return model;
    }

    public static byte[] ToData(AnimationModel model)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Skip header
        stream.Seek(476, SeekOrigin.Begin);

        foreach (var index in model.Indices1)
        {
            writer.Write(index);
        }

        foreach (var index in model.Indices2)
        {
            writer.Write(index);
        }

        foreach (var index in model.Indices3)
        {
            writer.Write(index);
        }

        foreach (var index in model.PartsData.PartsLayerIds)
        {
            writer.Write(index);
        }

        writer.Align(8, 0xFF);

        foreach (var partsId in model.PartsData.PartsIds.SelectMany(partsIds => partsIds))
        {
            writer.Write(partsId);
        }

        model.PoseSpec.RawDataInfoOffset = (uint)stream.Position - 112;

        foreach (var info in model.PoseData.RawDataInfos)
        {
            writer.Write((int)info.Type);
            writer.Write(info.EntriesCount);
        }

        writer.Align(16, 0xFF);
        model.BindPoseOffset = (uint)stream.Position;
        writer.Write(model.PoseData.Offset);
        writer.Align(16, 0xFF);

        foreach (var info in model.PoseData.RawDataInfos)
        {
            foreach (var quaternion in info.Values)
            {
                writer.Write(quaternion.X);
                writer.Write(quaternion.Y);
                writer.Write(quaternion.Z);
                writer.Write(quaternion.W);
            }
        }

        model.PoseSpec.SkeletalBoneNamesOffset = (ulong)stream.Position - 112;
        foreach (var name in model.BoneNames)
        {
            writer.WriteNullTerminatedString(name);
            writer.Write(new byte[48 - name.Length - 1]);
        }

        model.CustomUserDataOffset = (ulong)stream.Position;

        foreach (var node in model.CustomUserData.CustomUserDataIndexNodes)
        {
            writer.Write(node.Offset);
            writer.Write((int)node.Type);
            writer.Align(8, 0xFF);
        }

        for (var i = 0; i < model.CustomUserData.CustomUserDataIndexNodes.Length; i++)
        {
            var node = model.CustomUserData.CustomUserDataIndexNodes[i];

            switch (node.Type)
            {
                case LmEAnimCustomDataType.eCustomUserDataType_SkeletalAnimInfo:
                    var skeletalAnimInfo = (LmSkeletalAnimInfo)model.CustomUserData.CustomUserDatas[i];
                    writer.Write(skeletalAnimInfo.BoneCount);
                    writer.Write(skeletalAnimInfo.BoneCountFullSkeleton);
                    writer.Write(skeletalAnimInfo.BoneCountBindOnly);
                    writer.Write(skeletalAnimInfo.AfterBonamikStartIndex);
                    writer.Write(skeletalAnimInfo.KdBonesStartIndex);
                    writer.Write(skeletalAnimInfo.Dummy);

                    foreach (var child in skeletalAnimInfo.ChildInfoOffset)
                    {
                        writer.Write(child);
                    }

                    foreach (var unknown in skeletalAnimInfo.UnknownIndices)
                    {
                        writer.Write(unknown);
                    }

                    foreach (var parent in skeletalAnimInfo.ParentIndices)
                    {
                        writer.Write(parent);
                    }

                    foreach (var parent in skeletalAnimInfo.MaybeParentIndexOffsets)
                    {
                        writer.Write(parent);
                    }

                    foreach (var unknown in skeletalAnimInfo.UnknownIndices2)
                    {
                        writer.Write(unknown);
                    }

                    foreach (var unknown in skeletalAnimInfo.UnknownIndices3)
                    {
                        writer.Write(unknown);
                    }

                    writer.Align(16, 0xFF);

                    foreach (var rotation in skeletalAnimInfo.UnknownRotations)
                    {
                        writer.Write(rotation.X);
                        writer.Write(rotation.Y);
                        writer.Write(rotation.Z);
                        writer.Write(rotation.W);
                    }

                    foreach (var rotation in skeletalAnimInfo.UnknownRotations2)
                    {
                        writer.Write(rotation.X);
                        writer.Write(rotation.Y);
                        writer.Write(rotation.Z);
                        writer.Write(rotation.W);
                    }

                    foreach (var matrix in skeletalAnimInfo.Transformations)
                    {
                        for (var j = 0; j < 16; j++)
                        {
                            writer.Write(matrix.Values[j]);
                        }
                    }

                    foreach (var matrix in skeletalAnimInfo.UnknownTransformations)
                    {
                        for (var j = 0; j < 16; j++)
                        {
                            writer.Write(matrix.Values[j]);
                        }
                    }

                    break;
                case LmEAnimCustomDataType.eCustomUserDataType_LuminousIkRig:
                    var dependency = (DependencyLink)model.CustomUserData.CustomUserDatas[i];
                    writer.Write(dependency.Hash);
                    writer.WriteNullTerminatedString(dependency.Uri);
                    writer.Align(8, 0x00);
                    writer.Write((byte)0x00); // Guard byte
                    writer.Align(16, 0xFF);
                    break;
                case LmEAnimCustomDataType.eCustomUserDataType_LuminousTuningSetPack:
                    var tspack = (DependencyLink)model.CustomUserData.CustomUserDatas[i];
                    writer.Write(tspack.Hash);
                    writer.WriteNullTerminatedString(tspack.Uri);
                    writer.Align(8, 0x00);
                    writer.Write((byte)0x00); // Guard byte
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unhandled LmEAnimCustomDataType {node.Type}");
            }
        }

        model.DefaultInstanceDataFileBinOffset = (ulong)stream.Position;
        writer.Write((ushort)model.DefaultInstanceData.ScaleState);
        writer.Write((ushort)model.DefaultInstanceData.ProcessCutdownMode);
        writer.Write(model.DefaultInstanceData.RotationX);
        writer.Write(model.DefaultInstanceData.RotationY);
        writer.Write(model.DefaultInstanceData.RotationZ);

        model.TransitionsOffset = (ulong)stream.Position;
        foreach (var transition in model.Transitions)
        {
            writer.Write(transition.BlendTime);
            writer.Write(transition.PlaybackRate);
            writer.Write(transition.StartTime);
            writer.Write(transition.Flags);
            writer.Write(transition.UseCount);
        }

        model.FileSize = (uint)stream.Position;

        stream.Seek(0, SeekOrigin.Begin);

        writer.Write(model.FileSize);
        writer.Write(model.Version);
        writer.Write(model.BindPoseOffset);
        writer.Write(model.CustomUserDataCount);
        writer.Write(model.CustomUserDataOffset);
        writer.Write(model.DefaultInstanceDataFileBinOffset);
        writer.Write(model.TransitionsOffset);
        writer.Write(model.DefaultTransitionOffset);
        writer.Write(model.TransitionCount);
        writer.Write(model.InstanceDataTypesCount);
        writer.Write(model.TotalInstanceDataSize);

        for (var i = 0; i < model.InstanceDataTypesCount; i++)
        {
            var offset = model.InstanceDataOffsets[i];
            if (offset > 0)
            {
                offset -= 32;
            }

            writer.Write(offset);
        }

        writer.Write(model.ActiveInstances);
        writer.Align(56, 0xFF);

        writer.Write(model.PoseSpec.PoseSize);
        writer.Write(model.PoseSpec.RawTypesCount);
        writer.Write(model.PoseSpec.RawDataInfoOffset);
        writer.Write(model.PoseSpec.PartsSizeBlocks);

        writer.Write(new byte[24]);

        writer.Write(model.PoseSpec.PostProcessesCount);
        writer.Write(model.PoseSpec.Reserved);

        writer.Align(120, 0xFF);

        foreach (var channel in model.PoseSpec.Channels)
        {
            writer.Write(channel.EntriesCount);
            writer.Write(channel.DataOffset);
            writer.Write(channel.ModelTypeSpecificInfoOffset);
            writer.Write(channel.PartMaskStartBitIndex);
            writer.Align(8, 0xFF);
        }

        writer.Write(model.PoseSpec.SkeletalBoneNamesOffset);
        writer.Write(model.PoseSpec.SkeletalPartNamesCount);
        writer.Write(model.PoseSpec.PartsLayersCount);
        writer.Write(model.PoseSpec.TotalParts);

        return stream.ToArray();
    }

    public static byte[] ToPC(byte[] ps4Amdl)
    {
        var amdl = FromData(ps4Amdl, true);

        var info = (LmSkeletalAnimInfo)amdl.CustomUserData.CustomUserDatas[0];
        for (var i = 0; i < info.ChildInfoOffset.Length; i++)
        {
            if (info.ChildInfoOffset[i] >= 40)
            {
                info.ChildInfoOffset[i] -= 40;
            }
        }

        for (var i = 0; i < info.MaybeParentIndexOffsets.Length; i++)
        {
            if (info.MaybeParentIndexOffsets[i] >= 40)
            {
                info.MaybeParentIndexOffsets[i] -= 40;
            }
        }

        return ToData(amdl);
    }
}