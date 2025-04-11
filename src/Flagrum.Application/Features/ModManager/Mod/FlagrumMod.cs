using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Mod;

public class FlagrumMod
{
    private Stream _stream;

    public int MetadataSize { get; set; }
    public FlagrumModMetadata Metadata { get; set; }

    public Dictionary<IModBuildInstruction, (ulong Offset, uint Size)> FileTable { get; set; }
    public string ThumbnailPath { get; set; }
    public ulong ThumbnailOffset { get; set; }
    public uint ThumbnailSize { get; set; }

    public void Unpack(IModBuildInstruction instruction, string outputPath)
    {
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        var (offset, size) = FileTable[instruction];
        _stream.Seek((long)offset, SeekOrigin.Begin);
        _stream.CopyTo(stream, size);
    }

    public void Unpack(IModBuildInstruction instruction, string inputPath, string outputPath)
    {
        using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        var (offset, size) = FileTable[instruction];
        inputStream.Seek((long)offset, SeekOrigin.Begin);
        inputStream.CopyTo(stream, size);
    }

    public void UnpackThumbnail(string inputPath, string outputPath)
    {
        using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        inputStream.Seek((long)ThumbnailOffset, SeekOrigin.Begin);
        inputStream.CopyTo(stream, ThumbnailSize);
    }

    public void Read(BinaryReader reader, IModBuildInstructionFactory factory)
    {
        _stream = reader.BaseStream;

        // Read the metadata
        MetadataSize = reader.ReadInt32();
        var buffer = reader.ReadBytes(MetadataSize);
        Metadata = MemoryPackSerializer.Deserialize<FlagrumModMetadata>(buffer);
        reader.Align(16);

        // Inject any necessary dependencies into the build instructions
        foreach (var instruction in Metadata.Archives.SelectMany(a => a.Instructions)
                     .Union(Metadata.Instructions))
        {
            factory.Inject(instruction);
        }

        // Generate the appropriate information from the metadata
        var assets = Metadata.Archives
            .SelectMany(a => a.Instructions.Where(i => i is PackedAssetBuildInstruction))
            .Union(Metadata.Instructions.Where(i => i is LooseAssetBuildInstruction))
            .ToList();

        // Get the thumbnail location and size
        ThumbnailOffset = reader.ReadUInt64();
        ThumbnailSize = reader.ReadUInt32();

        // Read the rest of the file table
        FileTable = new Dictionary<IModBuildInstruction, (ulong Offset, uint Size)>();
        foreach (var asset in assets)
        {
            FileTable[asset] = (reader.ReadUInt64(), reader.ReadUInt32());
        }

        reader.Align(16);

        // Move to the end of this file
        if (FileTable.Count == 0)
        {
            reader.BaseStream.Seek((long)(ThumbnailOffset + ThumbnailSize), SeekOrigin.Begin);
        }
        else
        {
            var last = FileTable.Last().Value;
            reader.BaseStream.Seek((long)(last.Offset + last.Size), SeekOrigin.Begin);
        }

        reader.Align(16);
    }

    public void Write(BinaryWriter writer)
    {
        // Generate the appropriate information from the metadata
        var assets = Metadata.Archives
            .SelectMany(a => a.Instructions.Where(i => i is PackedAssetBuildInstruction))
            .Union(Metadata.Instructions.Where(i => i is LooseAssetBuildInstruction))
            .ToList();

        // Write the metadata
        var metadata = MemoryPackSerializer.Serialize(Metadata);
        MetadataSize = metadata.Length;
        writer.Write(MetadataSize);
        writer.Write(metadata);
        writer.Align(16, 0x00);

        // We'll return to write the file table later
        var fileTableAddress = writer.BaseStream.Position;
        var fileTableSize = 12 * assets.Count + 12;
        writer.Write(new byte[fileTableSize]);
        writer.Align(16, 0x00);

        // Write the thumbnail data
        var offsets = new List<(ulong, uint)>();
        var thumbnailOffset = writer.BaseStream.Position;
        using var thumbnailStream = new FileStream(ThumbnailPath, FileMode.Open, FileAccess.Read);
        thumbnailStream.CopyTo(writer.BaseStream);
        offsets.Add(((ulong)thumbnailOffset, (uint)(writer.BaseStream.Position - thumbnailOffset)));
        writer.Align(16, 0x00);

        // Write the file data
        foreach (var asset in assets)
        {
            var offset = writer.BaseStream.Position;

            var path = asset switch
            {
                PackedAssetBuildInstruction packedAsset => packedAsset.DataSource,
                LooseAssetBuildInstruction looseAsset => looseAsset.FilePath,
                _ => throw new ArgumentOutOfRangeException()
            };

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            fileStream.CopyTo(writer.BaseStream);
            offsets.Add(((ulong)offset, (uint)(writer.BaseStream.Position - offset)));
            writer.Align(16, 0x00);
        }

        // Write the file table
        var returnAddress = writer.BaseStream.Position;
        writer.BaseStream.Seek(fileTableAddress, SeekOrigin.Begin);
        foreach (var (offset, size) in offsets)
        {
            writer.Write(offset);
            writer.Write(size);
        }

        // Return to the end of the stream
        writer.BaseStream.Seek(returnAddress, SeekOrigin.Begin);
    }
}