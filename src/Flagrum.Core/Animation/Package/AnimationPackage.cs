using System;
using System.Collections.Generic;
using System.IO;

namespace Flagrum.Core.Animation.Package;

public class AnimationPackage
{
    public const int HeaderSize = 32;

    public ulong UsersOffet { get; set; }
    public uint Guid { get; set; }
    public uint FileSize { get; set; }
    public AnimationResourceType Type { get; set; }
    public uint ResourceIdCount { get; set; }
    public uint TypicalOverridePackageCount { get; set; }
    public uint ResourceItemsCount { get; set; }
    public List<uint> ResourceOffsets { get; set; } = new();
    public List<uint> ResourceIds { get; set; } = new();
    public uint OffsetToEndOfHashes { get; set; }
    public List<uint> ResourceHashes { get; set; } = new();
    public List<AnimationPackageItem> Items { get; set; } = new();

    public void ReplaceAnimation(int index, string newAniFile)
    {
        var item = Items[index];
        item.Ani = File.ReadAllBytes(newAniFile);
        var hash = BitConverter.ToUInt32(item.Ani, 4);
        item.Hash = hash;
    }

    public static AnimationPackage FromData(byte[] pka)
    {
        using var stream = new MemoryStream(pka);
        using var reader = new BinaryReader(stream);

        var package = new AnimationPackage
        {
            UsersOffet = reader.ReadUInt64(),
            Guid = reader.ReadUInt32(),
            FileSize = reader.ReadUInt32(),
            Type = (AnimationResourceType)reader.ReadInt32(),
            ResourceIdCount = reader.ReadUInt32(),
            TypicalOverridePackageCount = reader.ReadUInt32(),
            ResourceItemsCount = reader.ReadUInt32()
        };

        for (var i = 0; i < package.ResourceItemsCount; i++)
        {
            package.ResourceOffsets.Add(reader.ReadUInt32());
        }

        for (var i = 0; i < package.ResourceIdCount; i++)
        {
            package.ResourceIds.Add(reader.ReadUInt32());
        }

        package.OffsetToEndOfHashes = reader.ReadUInt32();

        for (var i = 0; i < package.ResourceItemsCount; i++)
        {
            package.ResourceHashes.Add(reader.ReadUInt32());
        }

        for (var i = 0; i < package.ResourceItemsCount; i++)
        {
            uint size;
            if (i == package.ResourceItemsCount - 1)
            {
                size = package.FileSize - package.ResourceOffsets[i];
            }
            else
            {
                size = package.ResourceOffsets[i + 1] - package.ResourceOffsets[i];
            }

            var item = new AnimationPackageItem
            {
                Offset = package.ResourceOffsets[i],
                Id = package.ResourceIds[i],
                Hash = package.ResourceHashes[i],
                Ani = new byte[size]
            };

            stream.Seek(item.Offset, SeekOrigin.Begin);
            stream.ReadExactly(item.Ani);
            package.Items.Add(item);
        }

        return package;
    }

    public static byte[] ToData(AnimationPackage package)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Skip the header and offsets for now
        stream.Seek(HeaderSize + package.ResourceItemsCount * 4, SeekOrigin.Begin);

        // Write the resource IDs
        foreach (var item in package.Items)
        {
            writer.Write(item.Id);
        }

        // Write the OffsetToEndOfHashes
        writer.Write((uint)(stream.Position + package.Items.Count * 4 + 4));

        // Write the resource hashes
        foreach (var item in package.Items)
        {
            writer.Write(item.Hash);
        }

        // Write the header alignment
        var paddingSize = 16 + 16 * (stream.Position / 16) - stream.Position;
        var padding = new byte[paddingSize];
        for (var i = 0; i < paddingSize; i++)
        {
            padding[i] = 0xFF;
        }

        writer.Write(padding);

        // Write the ANI files
        foreach (var item in package.Items)
        {
            item.Offset = (uint)stream.Position;
            writer.Write(item.Ani);
        }

        // Write the header
        var fileSize = stream.Position;
        stream.Seek(0, SeekOrigin.Begin);
        writer.Write(package.UsersOffet);
        writer.Write(package.Guid);
        writer.Write((uint)fileSize);
        writer.Write((int)package.Type);
        writer.Write((uint)package.ResourceIds.Count);
        writer.Write(package.TypicalOverridePackageCount);
        writer.Write((uint)package.Items.Count);

        // Write the resource offsets
        foreach (var item in package.Items)
        {
            writer.Write(item.Offset);
        }

        return stream.ToArray();
    }
}