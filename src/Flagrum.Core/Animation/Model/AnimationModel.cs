using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Flagrum.Core.Animation.Clip;
using Flagrum.Core.Utilities.Extensions;
using Quaternion = Flagrum.Core.Mathematics.Quaternion;

namespace Flagrum.Core.Animation.Model;

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
    public PoseSpecification PoseSpecification { get; set; }
    public ushort[] Indices1 { get; set; }
    public ushort[] Indices2 { get; set; }
    public ushort[] Indices3 { get; set; }
    public PartsData PartsData { get; set; }
    public PoseData PoseData { get; set; }
    public string[] BoneNames { get; set; }
    public AnimationModelCustomUserData CustomUserData { get; set; }
    public SkeletalAnimationInstanceData DefaultInstanceData { get; set; }
    public AnimationTransitionParameters[] Transitions { get; set; }

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

        model.PoseSpecification = new PoseSpecification
        {
            PoseSize = reader.ReadUInt32(),
            RawTypesCount = reader.ReadUInt32(),
            RawDataInfoOffset = reader.ReadUInt32(),
            PartsSizeBlocks = reader.ReadUInt32()
        };

        stream.Seek(24, SeekOrigin.Current); // Padding
        model.PoseSpecification.PostProcessesCount = reader.ReadUInt32();
        model.PoseSpecification.Reserved = reader.ReadInt32();
        stream.Align(120);

        if (isPs4)
        {
            // PS4 amdl seems to have an extra 40 bytes of padding here
            stream.Seek(40, SeekOrigin.Current);
        }

        model.PoseSpecification.Channels = new AnimationDataChannelInformation[9];
        for (var i = 0; i < 9; i++)
        {
            model.PoseSpecification.Channels[i] = new AnimationDataChannelInformation
            {
                EntriesCount = reader.ReadUInt32(),
                DataOffset = reader.ReadUInt32(),
                ModelTypeSpecificInfoOffset = reader.ReadUInt64(),
                PartMaskStartBitIndex = reader.ReadUInt32()
            };

            stream.Align(8);
        }

        model.PoseSpecification.SkeletalBoneNamesOffset = reader.ReadUInt64();
        model.PoseSpecification.SkeletalPartNamesCount = reader.ReadUInt32();
        model.PoseSpecification.PartsLayersCount = reader.ReadUInt32();
        model.PoseSpecification.TotalParts = reader.ReadUInt32();

        var indicesOffset = stream.Position;

        stream.Seek(model.PoseSpecification.RawDataInfoOffset + 112, SeekOrigin.Begin);

        model.PoseData = new PoseData {RawDataInfos = new RawDataInformation[model.PoseSpecification.RawTypesCount]};
        for (var i = 0; i < model.PoseSpecification.RawTypesCount; i++)
        {
            model.PoseData.RawDataInfos[i] = new RawDataInformation
            {
                Type = (RawType)reader.ReadInt32(),
                EntriesCount = reader.ReadUInt32()
            };
        }

        stream.Align(16);
        model.PoseData.Offset = reader.ReadUInt64();
        stream.Align(16);

        for (var i = 0; i < model.PoseSpecification.RawTypesCount; i++)
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

        model.BoneNames = new string[model.PoseSpecification.SkeletalPartNamesCount];
        for (var i = 0; i < model.PoseSpecification.SkeletalPartNamesCount; i++)
        {
            model.BoneNames[i] = reader.ReadNullTerminatedString();
            stream.Seek(48 - model.BoneNames[i].Length - 1, SeekOrigin.Current);
        }

        var start = stream.Position;
        model.CustomUserData = new AnimationModelCustomUserData
        {
            CustomUserDataIndexNodes = new CustomUserDataIndexNode[model.CustomUserDataCount],
            CustomUserDatas = new object[model.CustomUserDataCount]
        };

        for (var i = 0; i < model.CustomUserDataCount; i++)
        {
            model.CustomUserData.CustomUserDataIndexNodes[i] = new CustomUserDataIndexNode
            {
                Offset = reader.ReadUInt64(),
                Type = (AnimationCustomDataType)reader.ReadInt32()
            };

            stream.Align(8);
        }

        for (var i = 0; i < model.CustomUserDataCount; i++)
        {
            var node = model.CustomUserData.CustomUserDataIndexNodes[i];
            stream.Seek(start + (long)node.Offset, SeekOrigin.Begin);

            switch (node.Type)
            {
                case AnimationCustomDataType.eCustomUserDataType_SkeletalAnimInfo:
                    var info = new SkeletalAnimationInformation
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

                    info.Transformations = new Matrix4x4[info.BoneCountFullSkeleton];
                    for (var j = 0; j < info.BoneCountFullSkeleton; j++)
                    {
                        info.Transformations[j] = new Matrix4x4();
                        for (var row = 0; row < 4; row++)
                        {
                            for (var column = 0; column < 4; column++)
                            {
                                info.Transformations[j].SetValue(row, column, reader.ReadSingle());
                            }
                        }
                    }

                    var finalMatricesCount = info.BoneCountBindOnly - info.BoneCountFullSkeleton;

                    info.UnknownTransformations = new Matrix4x4[finalMatricesCount];
                    for (var j = 0; j < finalMatricesCount; j++)
                    {
                        info.UnknownTransformations[j] = new Matrix4x4();
                        for (var row = 0; row < 4; row++)
                        {
                            for (var column = 0; column < 4; column++)
                            {
                                info.UnknownTransformations[j].SetValue(row, column, reader.ReadSingle());
                            }
                        }
                    }

                    model.CustomUserData.CustomUserDatas[i] = info;
                    break;
                case AnimationCustomDataType.eCustomUserDataType_LuminousIkRig:
                    var dependency = new AnimationModelDependencyLink
                    {
                        Hash = reader.ReadUInt64(),
                        Uri = reader.ReadNullTerminatedString()
                    };

                    stream.Align(8);
                    _ = stream.ReadByte(); // Guard byte
                    stream.Align(16);

                    model.CustomUserData.CustomUserDatas[i] = dependency;
                    break;
                case AnimationCustomDataType.eCustomUserDataType_LuminousTuningSetPack:
                    var tspack = new AnimationModelDependencyLink
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

        model.DefaultInstanceData = new SkeletalAnimationInstanceData
        {
            ScaleState = (AnimatedScaleState)reader.ReadUInt16(),
            ProcessCutdownMode = (AnimationProcessCutdownMode)reader.ReadUInt16(),
            RotationX = reader.ReadSingle(),
            RotationY = reader.ReadInt32(),
            RotationZ = reader.ReadSingle()
        };

        model.Transitions = new AnimationTransitionParameters[model.TransitionCount];
        for (var i = 0; i < model.TransitionCount; i++)
        {
            model.Transitions[i] = new AnimationTransitionParameters
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
                AnimationCustomDataType.eCustomUserDataType_SkeletalAnimInfo)
            {
                index = i;
                break;
            }
        }

        var skeletalAnimInfo = (SkeletalAnimationInformation)model.CustomUserData.CustomUserDatas[index];

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

        model.PartsData = new PartsData {PartsLayerIds = new uint[model.PoseSpecification.PartsLayersCount]};
        for (var i = 0; i < model.PoseSpecification.PartsLayersCount; i++)
        {
            model.PartsData.PartsLayerIds[i] = reader.ReadUInt32();
        }

        stream.Align(8);

        for (var i = 0; i < model.PoseSpecification.PartsLayersCount; i++)
        {
            model.PartsData.PartsIds.Add(new ulong[model.PoseSpecification.PartsSizeBlocks]);
            for (var j = 0; j < model.PoseSpecification.PartsSizeBlocks; j++)
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

        var modelTypeSpecificInfoOffset = 0UL;

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

        model.PoseSpecification.RawDataInfoOffset = (uint)stream.Position - 112;

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

        model.PoseSpecification.SkeletalBoneNamesOffset = (ulong)stream.Position - 112;
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
                case AnimationCustomDataType.eCustomUserDataType_SkeletalAnimInfo:
                    modelTypeSpecificInfoOffset = (ulong)stream.Position - 112;
                    var skeletalAnimInfo = (SkeletalAnimationInformation)model.CustomUserData.CustomUserDatas[i];
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
                        for (var row = 0; row < 4; row++)
                        {
                            for (var column = 0; column < 4; column++)
                            {
                                writer.Write(matrix.GetValue(row, column));
                            }
                        }
                    }

                    foreach (var matrix in skeletalAnimInfo.UnknownTransformations)
                    {
                        for (var row = 0; row < 4; row++)
                        {
                            for (var column = 0; column < 4; column++)
                            {
                                writer.Write(matrix.GetValue(row, column));
                            }
                        }
                    }

                    break;
                case AnimationCustomDataType.eCustomUserDataType_LuminousIkRig:
                    var dependency = (AnimationModelDependencyLink)model.CustomUserData.CustomUserDatas[i];
                    writer.Write(dependency.Hash);
                    writer.WriteNullTerminatedString(dependency.Uri);
                    writer.Align(8, 0x00);
                    writer.Write((byte)0x00); // Guard byte
                    writer.Align(16, 0xFF);
                    break;
                case AnimationCustomDataType.eCustomUserDataType_LuminousTuningSetPack:
                    var tspack = (AnimationModelDependencyLink)model.CustomUserData.CustomUserDatas[i];
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
            writer.Write(model.InstanceDataOffsets[i]);
        }

        writer.Write(model.ActiveInstances);
        writer.Align(56, 0xFF);

        writer.Write(model.PoseSpecification.PoseSize);
        writer.Write(model.PoseSpecification.RawTypesCount);
        writer.Write(model.PoseSpecification.RawDataInfoOffset);
        writer.Write(model.PoseSpecification.PartsSizeBlocks);

        writer.Write(new byte[24]);

        writer.Write(model.PoseSpecification.PostProcessesCount);
        writer.Write(model.PoseSpecification.Reserved);

        writer.Align(120, 0xFF);

        foreach (var channel in model.PoseSpecification.Channels)
        {
            writer.Write(channel.EntriesCount);
            writer.Write(channel.DataOffset);
            writer.Write(channel.ModelTypeSpecificInfoOffset > 0 ? modelTypeSpecificInfoOffset : 0UL);
            writer.Write(channel.PartMaskStartBitIndex);
            writer.Align(8, 0xFF);
        }

        writer.Write(model.PoseSpecification.SkeletalBoneNamesOffset);
        writer.Write(model.PoseSpecification.SkeletalPartNamesCount);
        writer.Write(model.PoseSpecification.PartsLayersCount);
        writer.Write(model.PoseSpecification.TotalParts);

        return stream.ToArray();
    }

    public static byte[] ToPC(byte[] ps4Amdl)
    {
        var amdl = FromData(ps4Amdl, true);
        return ToData(amdl);
    }
}