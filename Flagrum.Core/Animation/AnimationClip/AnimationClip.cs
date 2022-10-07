using System;
using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Animation.AnimationClip;

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

    public AnimationFrame[] KeyFrames { get; set; }

    public static AnimationClip FromData(byte[] ani)
    {
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
                TrackType = (LmETriggerTrackType)reader.ReadUInt16(),
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

        // Skip reading the actual custom user data as it is not yet understood
        stream.Seek((long)clip.CustomUserDataIndexOffset, SeekOrigin.Begin);

        if (clip.CustomUserDataCount > 0)
        {
            clip.CustomUserData = new AnimationCustomUserData[clip.CustomUserDataCount];
            for (var i = 0; i < clip.CustomUserDataCount; i++)
            {
                clip.CustomUserData[i] = new AnimationCustomUserData
                {
                    Offset = reader.ReadUInt64(),
                    Type = (LmEAnimCustomDataType)reader.ReadInt32()
                };

                stream.Align(8);
            }
        }

        stream.Seek((long)clip.FrameDataOffset, SeekOrigin.Begin);

        var frameCount = (int)(Math.Ceiling(clip.DurationSeconds * clip.KeyframeFps) + 1);
        clip.KeyFrames = new AnimationFrame[frameCount];
        for (var i = 0; i < frameCount; i++)
        {
            var numKeys1 = reader.ReadByte();
            var numKeys2 = reader.ReadByte();

            if (numKeys1 == 0 && numKeys2 == 0)
            {
                // This is an empty frame, continue to the next
                continue;
            }

            var frame = new AnimationFrame
            {
                NumKeys = new[] {numKeys1, numKeys2, reader.ReadByte()}
            };

            var unknownsCount = ((frame.NumKeys[1] << 8) + frame.NumKeys[2]) & 0xFFF;
            frame.Unknowns = new byte[unknownsCount];
            for (var j = 0; j < unknownsCount; j++)
            {
                frame.Unknowns[j] = reader.ReadByte();
            }

            var currentKeyTimeBytesCount = (((frame.NumKeys[1] >> 4) + 16 * frame.NumKeys[0]) & 1) +
                                           (((frame.NumKeys[1] >> 4) + 16 * frame.NumKeys[0]) >> 1);
            frame.CurrentKeyTimeBytes = new byte[currentKeyTimeBytesCount];
            for (var j = 0; j < currentKeyTimeBytesCount; j++)
            {
                frame.CurrentKeyTimeBytes[j] = reader.ReadByte();
            }

            stream.Align(2);

            var packedKeysCount = (frame.NumKeys[1] >> 4) + 16 * frame.NumKeys[0];
            frame.PackedKeys = new AnimationPackedKey[packedKeysCount];
            for (var j = 0; j < packedKeysCount; j++)
            {
                frame.PackedKeys[j] = new AnimationPackedKey
                {
                    Values = new[] {reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()}
                };
            }

            clip.KeyFrames[i] = frame;
        }

        clip.SeedFrame = new AnimationFrame
        {
            NumKeys = reader.ReadBytes(3)
        };

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
}