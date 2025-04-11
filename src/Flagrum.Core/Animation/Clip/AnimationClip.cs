using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Animation.Model;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Animation.Clip;

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
    public ulong AnimationFrameChunkInformationPointerArrayOffset { get; set; }
    public ulong FrameDataOffset { get; set; }
    public ulong SeedFrameOffset { get; set; }
    public ulong UnpackConstantsOffset { get; set; }
    public ulong CustomUserDataIndexOffset { get; set; }

    public uint CustomUserDataCount { get; set; }
    public uint TriggerCount { get; set; }

    public AnimationTrigger[] Triggers { get; set; }
    public ulong[] PartsSizeBlocks1 { get; set; }
    public ulong[] PartsSizeBlocks2 { get; set; }
    public ulong[] PartsSizeBlocks3 { get; set; }

    public ulong[] FrameDataChunkOffsets { get; set; }
    public ushort[] LastFrameStartOffsets { get; set; }
    public float[] UnpackConstants { get; set; }
    public float DecompressRangeScalar { get; set; }

    public AnimationCustomUserData[] CustomUserData { get; set; }
    public List<AnimationRawTypeInfo> RawTypeInfos { get; set; } = new();
    public AnimationFrame SeedFrame { get; set; }

    public static AnimationClip FromData(byte[] amdl, byte[] ani)
    {
        var model = AnimationModel.FromData(amdl, false);

        using var stream = new MemoryStream(ani);
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
            FrameDataChunkStartPointerArrayOffset = reader.ReadUInt64(),
            AnimationFrameChunkInformationPointerArrayOffset = reader.ReadUInt64(),
            FrameDataOffset = reader.ReadUInt64(),
            SeedFrameOffset = reader.ReadUInt64(),
            UnpackConstantsOffset = reader.ReadUInt64(),
            CustomUserDataIndexOffset = reader.ReadUInt64(),
            CustomUserDataCount = reader.ReadUInt32(),
            TriggerCount = reader.ReadUInt32()
        };

        clip.Triggers = new AnimationTrigger[clip.TriggerCount];
        for (var i = 0; i < clip.TriggerCount; i++)
        {
            clip.Triggers[i] = new AnimationTrigger
            {
                TriggerFrame = reader.ReadUInt16(),
                TypeAndMirror = reader.ReadUInt16(),
                TrackType = (TriggerTrackType)reader.ReadUInt16(),
                CustomDataIndex = reader.ReadInt16()
            };
        }

        clip.PartsSizeBlocks1 = new ulong[clip.PartsSizeBlocksCount];
        for (var i = 0; i < clip.PartsSizeBlocksCount; i++)
        {
            clip.PartsSizeBlocks1[i] = reader.ReadUInt64();
        }

        clip.PartsSizeBlocks2 = new ulong[clip.PartsSizeBlocksCount];
        for (var i = 0; i < clip.PartsSizeBlocksCount; i++)
        {
            clip.PartsSizeBlocks2[i] = reader.ReadUInt64();
        }

        clip.PartsSizeBlocks3 = new ulong[clip.PartsSizeBlocksCount];
        for (var i = 0; i < clip.PartsSizeBlocksCount; i++)
        {
            clip.PartsSizeBlocks3[i] = reader.ReadUInt64();
        }

        var count = (int)(clip.DurationSeconds * clip.KeyframeFps + 0.001);
        var chunkCount = count / 16 + 1;

        clip.FrameDataChunkOffsets = new ulong[chunkCount];
        for (var i = 0; i < chunkCount; i++)
        {
            clip.FrameDataChunkOffsets[i] = reader.ReadUInt64();
        }

        clip.UnpackConstants = new float[clip.CacheTypesCount];
        for (var i = 0; i < clip.CacheTypesCount; i++)
        {
            clip.UnpackConstants[i] = reader.ReadSingle();
        }

        clip.DecompressRangeScalar = reader.ReadSingle();

        stream.Seek((long)clip.CustomUserDataIndexOffset, SeekOrigin.Begin);

        if (clip.CustomUserDataCount > 0)
        {
            clip.CustomUserData = new AnimationCustomUserData[clip.CustomUserDataCount];
            for (var i = 0; i < clip.CustomUserDataCount; i++)
            {
                clip.CustomUserData[i] = new AnimationCustomUserData
                {
                    Offset = reader.ReadUInt64(),
                    Type = (AnimationCustomDataType)reader.ReadInt32()
                };

                stream.Align(8);
            }
        }

        stream.Seek((long)clip.SeedFrameOffset, SeekOrigin.Begin);

        clip.SeedFrame = new AnimationFrame
        {
            NumKeys = reader.ReadBytes(3),
            OnOffBits = reader.ReadByte()
        };

        var seedFrameKeyCount = GetPopulationCount(clip.PartsSizeBlocks1) - GetPopulationCount(clip.PartsSizeBlocks2);
        var first = GetPopulationCount(clip.PartsSizeBlocks1);
        var second = GetPopulationCount(clip.PartsSizeBlocks2);
        var third = GetPopulationCount(clip.PartsSizeBlocks3);
        clip.CalculateRawTypeInfos(model, clip.PartsSizeBlocksCount << 6, reader);

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

        return stream.ToArray();
    }

    private static ulong GetPopulationCount(IEnumerable<ulong> bitset)
    {
        return bitset.Aggregate(0UL, (current, value) =>
            current + ((0x101010101010101
                        * ((((value - ((value >> 1) & 0x5555555555555555)) & 0x3333333333333333)
                            + (((value - ((value >> 1) & 0x5555555555555555)) >> 2) & 0x3333333333333333)
                            + ((((value - ((value >> 1) & 0x5555555555555555)) & 0x3333333333333333)
                                + (((value - ((value >> 1) & 0x5555555555555555)) >> 2) & 0x3333333333333333)) >> 4)) &
                           0xF0F0F0F0F0F0F0F)) >> 32 >> 24));
    }

    private void CalculateRawTypeInfos(AnimationModel model, ulong bitsetPopulationCount, BinaryReader reader)
    {
        var returnAddress = reader.BaseStream.Position;

        var playbackRatea = PartsSizeBlocksCount << 6;
        var counter2 = 0;
        var counter3 = 0;
        var conditionalCounter = 0;
        var rawDataEntriesCounter = 0;
        var numPoseSpecRawTypes = model.PoseSpecification.RawTypesCount;
        var poseDataStartOffset = 16u;
        var poseDataStartOffset2 = 16u;
        var defaultRawDataType = RawType.eRawType_Num_Raw_Types;
        var rawDataEntriesCount = model.PoseData.RawDataInfos[0].EntriesCount;
        var rawDataEntriesCount2 = 0u;
        var rawDataEntriesCount3 = 0u;
        var innerCounter = 0;
        var numConstantRawTypes = 0;
        RawTypeInfos.Add(new AnimationRawTypeInfo());

        do
        {
            if (counter2 >= playbackRatea)
            {
                break;
            }

            if (counter2 >= rawDataEntriesCount)
            {
                rawDataEntriesCount2 = model.PoseData.RawDataInfos[rawDataEntriesCounter].EntriesCount;
                rawDataEntriesCounter++;
                conditionalCounter++;

                counter3 = 0;
                rawDataEntriesCount3 = rawDataEntriesCount;
                rawDataEntriesCount += model.PoseData.RawDataInfos[rawDataEntriesCounter].EntriesCount;

                poseDataStartOffset2 = (16 * rawDataEntriesCount2 + poseDataStartOffset2 + 15) & 0xFFFFFFF0;
                numPoseSpecRawTypes--;

                poseDataStartOffset = poseDataStartOffset2;

                if (numPoseSpecRawTypes <= 0)
                {
                    break;
                }
            }

            var partsSize = PartsSizeBlocks1[counter2 >> 6];

            if (BitExtensions.BitTest64(partsSize, counter2 & 0x3F))
            {
                var rawDataType = model.PoseData.RawDataInfos[rawDataEntriesCounter].Type;

                if (defaultRawDataType != rawDataType)
                {
                    defaultRawDataType = rawDataType;

                    if (numConstantRawTypes > 0)
                    {
                        innerCounter++;

                        if (RawTypeInfos.Count - 1 < innerCounter)
                        {
                            RawTypeInfos.Add(new AnimationRawTypeInfo());
                        }

                        RawTypeInfos[innerCounter].NumberInCache = 0;
                    }

                    RawTypeInfos[innerCounter].PoseDataStartOffset = poseDataStartOffset;
                    RawTypeInfos[innerCounter].PoseStartBit = (ushort)rawDataEntriesCount3;

                    var offset = UnpackConstantsOffset + ((uint)innerCounter + CacheTypesCount) * 4;
                    reader.BaseStream.Seek((long)offset, SeekOrigin.Begin);
                    RawTypeInfos[innerCounter].DecompressRangeScalar = reader.ReadSingle();

                    // if (CustomUserDataCount > 0)
                    // {
                    //     offset = CustomUserData[conditionalCounter].Offset + 32;
                    //     reader.BaseStream.Seek((long)offset, SeekOrigin.Begin);
                    // }
                    // else
                    {
                        RawTypeInfos[innerCounter].PackedKeyType = GetDefaultForRawType(rawDataType);
                    }

                    numConstantRawTypes++;
                    poseDataStartOffset2 = poseDataStartOffset;
                }

                bitsetPopulationCount--;
                RawTypeInfos[innerCounter].NumberInCache++;
            }

            counter2++;
            counter3++;
        } while (bitsetPopulationCount > 0);

        reader.BaseStream.Seek(returnAddress, SeekOrigin.Begin);
    }

    private static PackedKeyType GetDefaultForRawType(RawType type)
    {
        return type switch
        {
            RawType.eRawType_Quaternion => PackedKeyType.ePackedKeyType_Quat_48_Deprecated,
            RawType.eRawType_UncompressedVector3 => PackedKeyType.ePackedKeyType_Vector3_128,
            RawType.eRawType_Vector3_Range0 => PackedKeyType.ePackedKeyType_Vector3_48,
            RawType.eRawType_Vector3_Range1 => PackedKeyType.ePackedKeyType_Vector3_48,
            RawType.eRawType_Vector3_Range2 => PackedKeyType.ePackedKeyType_Vector3_48,
            RawType.eRawType_Vector3_NoScale => PackedKeyType.ePackedKeyType_Vector3_48_NoScale,
            RawType.eRawType_Num_Raw_Types => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}