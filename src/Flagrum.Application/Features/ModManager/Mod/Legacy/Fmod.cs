using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Utilities.Exceptions;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Application.Features.ModManager.Mod.Legacy;

public class Fmod
{
    public const uint CurrentFmodVersion = 4;
    private ulong _offset;
    private string _path;

    public char[] Magic { get; private set; } = "FMOD".ToCharArray();
    public uint Version { get; private set; } = CurrentFmodVersion;
    public Guid? Guid { get; set; }
    public ModFlags Flags { get; set; }
    public ModCategory Category { get; set; }
    public uint EarcCount { get; set; }
    public uint LooseFileCount { get; set; }
    public uint HookFeaturesCount { get; set; }
    public ulong RelativePathBufferOffset { get; set; }
    public ulong UriBufferOffset { get; set; }
    public ulong FileNameBufferOffset { get; set; }
    public ulong EarcsOffset { get; set; }
    public ulong FilesOffset { get; set; }
    public ulong HookFeaturesOffset { get; set; }
    public ulong DataOffset { get; set; }
    public ulong ThumbnailOffset { get; set; }
    public uint ThumbnailSize { get; set; }
    public ulong StringBufferOffset { get; set; }

    public uint NameOffset { get; set; }
    public uint AuthorOffset { get; set; }
    public uint DescriptionOffset { get; set; }
    public uint ReadmeOffset { get; set; }

    public List<FmodEarc> Earcs { get; set; } = new();
    public List<FmodFile> Files { get; set; } = new();
    public List<FmodHookFeature> HookFeatures { get; set; } = new();

    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Readme { get; set; }
    public string ThumbnailDataSource { get; set; }

    public ulong GetThumbnailOffset() => _offset + ThumbnailOffset;

    public ulong GetFileOffset(FmodEarcFile file) => _offset + DataOffset + file.DataOffset;

    public ulong GetFileOffset(FmodFile file) => _offset + DataOffset + file.DataOffset;

    public void UnpackFile(FmodEarcFile file, string outputPath)
    {
        using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read);
        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        stream.Seek((long)(_offset + DataOffset + file.DataOffset), SeekOrigin.Begin);
        stream.CopyTo(fileStream, file.Size);
    }

    public void Unpack(string earcModDirectory, string thumbnailPath)
    {
        using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read);
        stream.Seek((long)_offset, SeekOrigin.Begin);

        var index = 0;
        foreach (var file in Earcs.SelectMany(e => e.Files
                     .Where(f => f.Type is LegacyModBuildInstruction.AddPackedFile
                         or LegacyModBuildInstruction.ReplacePackedFile
                         or LegacyModBuildInstruction.AddToPackedTextureArray)))
        {
            var path = $@"{earcModDirectory}\{++index}.ffg";
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.Seek((long)(_offset + DataOffset + file.DataOffset), SeekOrigin.Begin);
            stream.CopyTo(fileStream, file.Size);
        }

        foreach (var file in Files)
        {
            var path = $@"{earcModDirectory}\{file.FileName}";
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.Seek((long)(_offset + DataOffset + file.DataOffset), SeekOrigin.Begin);
            stream.CopyTo(fileStream, file.Size);
        }

        using var thumbnailStream = new FileStream(thumbnailPath, FileMode.Create, FileAccess.Write);
        stream.Seek((long)(_offset + ThumbnailOffset), SeekOrigin.Begin);
        stream.CopyTo(thumbnailStream, ThumbnailSize);
    }

    public void Read(ulong offset, string inputPath)
    {
        _path = inputPath;
        _offset = offset;

        Stream stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        var reader = new BinaryReader(stream, Encoding.UTF8, true);
        stream.Seek((long)offset, SeekOrigin.Begin);

        Magic = reader.ReadChars(4);
        if (Magic[0] != 'F' || Magic[1] != 'M' || Magic[2] != 'O' || Magic[3] != 'D')
        {
            throw new FileFormatException("Input file was not a valid FMOD file");
        }

        Version = reader.ReadUInt32();

        switch (Version)
        {
            case > CurrentFmodVersion:
                throw new FormatVersionException("FMOD version newer than current supported version");
            case >= 2:
            {
                var guid = reader.ReadBytes(16);
                if (!guid.SequenceEqual(new byte[16]))
                {
                    Guid = new Guid(guid);
                }

                Flags = (ModFlags)reader.ReadUInt32();
                break;
            }
        }

        Category = (ModCategory)reader.ReadInt32();
        EarcCount = reader.ReadUInt32();
        LooseFileCount = reader.ReadUInt32();

        if (Version >= 4)
        {
            HookFeaturesCount = reader.ReadUInt32();
        }

        RelativePathBufferOffset = reader.ReadUInt64();
        UriBufferOffset = reader.ReadUInt64();
        FileNameBufferOffset = reader.ReadUInt64();
        EarcsOffset = reader.ReadUInt64();
        FilesOffset = reader.ReadUInt64();

        if (Version >= 4)
        {
            HookFeaturesOffset = reader.ReadUInt64();
        }

        DataOffset = reader.ReadUInt64();
        ThumbnailOffset = reader.ReadUInt64();
        ThumbnailSize = reader.ReadUInt32();
        StringBufferOffset = reader.ReadUInt64();
        NameOffset = reader.ReadUInt32();
        AuthorOffset = reader.ReadUInt32();
        DescriptionOffset = reader.ReadUInt32();
        ReadmeOffset = reader.ReadUInt32();

        // If this is a protected official mod, setup the stream for decryption
        // if (Flags.HasFlag(ModFlags.Protected))
        // {
        //     if (Guid == null || !EarlyAccessMods.GuidMap.ContainsKey(Guid.Value))
        //     {
        //         throw new FileTamperException("The FMOD file has been tampered with and cannot be installed");
        //     }
        //
        //     if (!isEarlyAccessEnabled)
        //     {
        //         throw new EarlyAccessException("Early Access is required to install this mod");
        //     }
        //
        //     var officialMod = EarlyAccessMods.GuidMap[Guid.Value];
        //     var checksum = EarlyAccessMods.CalculateChecksum(inputPath);
        //     if (!checksum.SequenceEqual(officialMod.Checksum))
        //     {
        //         throw new FileTamperException("The FMOD file has been tampered with and cannot be installed");
        //     }
        //
        //     var aes = Aes.Create();
        //     aes.Key = officialMod.Key;
        //     aes.IV = officialMod.IV;
        //     aes.Padding = PaddingMode.Zeros;
        //
        //     reader.Dispose();
        //
        //     // Decrypt the rest of the stream into memory
        //     var memoryStream = new MemoryStream();
        //     memoryStream.Seek(128, SeekOrigin.Begin);
        //     stream.Align(128);
        //
        //     using (var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        //     {
        //         cryptoStream.CopyTo(memoryStream);
        //     }
        //
        //     stream.Dispose();
        //     stream = memoryStream;
        //     reader = new BinaryReader(memoryStream, Encoding.UTF8, true);
        // }

        stream.Seek((long)(offset + EarcsOffset), SeekOrigin.Begin);

        for (var i = 0; i < EarcCount; i++)
        {
            var earc = new FmodEarc();
            earc.Read(reader);
            Earcs.Add(earc);
        }

        stream.Seek((long)(offset + FilesOffset), SeekOrigin.Begin);

        for (var i = 0; i < LooseFileCount; i++)
        {
            var file = new FmodFile();
            file.Read(reader);
            Files.Add(file);
        }

        stream.Seek((long)(offset + HookFeaturesOffset), SeekOrigin.Begin);

        for (var i = 0; i < HookFeaturesCount; i++)
        {
            var feature = new FmodHookFeature();
            feature.Read(reader);
            HookFeatures.Add(feature);
        }

        stream.Seek((long)(offset + StringBufferOffset + NameOffset), SeekOrigin.Begin);
        Name = reader.ReadNullTerminatedString();
        stream.Seek((long)(offset + StringBufferOffset + AuthorOffset), SeekOrigin.Begin);
        Author = reader.ReadNullTerminatedString();
        stream.Seek((long)(offset + StringBufferOffset + DescriptionOffset), SeekOrigin.Begin);
        Description = reader.ReadNullTerminatedString();
        stream.Seek((long)(offset + StringBufferOffset + ReadmeOffset), SeekOrigin.Begin);
        Readme = reader.ReadNullTerminatedString();

        foreach (var earc in Earcs)
        {
            stream.Seek((long)(offset + RelativePathBufferOffset + earc.RelativePathOffset), SeekOrigin.Begin);
            earc.RelativePath = reader.ReadNullTerminatedString();
        }

        foreach (var file in Files)
        {
            stream.Seek((long)(offset + RelativePathBufferOffset + file.RelativePathOffset), SeekOrigin.Begin);
            file.RelativePath = reader.ReadNullTerminatedString();
        }

        foreach (var earc in Earcs)
        {
            foreach (var file in earc.Files)
            {
                stream.Seek((long)(offset + UriBufferOffset + file.UriOffset), SeekOrigin.Begin);
                file.Uri = reader.ReadNullTerminatedString();
            }
        }

        foreach (var file in Files)
        {
            stream.Seek((long)(offset + FileNameBufferOffset + file.FileNameOffset), SeekOrigin.Begin);
            file.FileName = reader.ReadNullTerminatedString();
        }

        reader.Dispose();
        stream.Dispose();
    }

    public void Write(Stream stream)
    {
        EarcCount = (uint)Earcs.Count;
        LooseFileCount = (uint)Files.Count;
        HookFeaturesCount = (uint)HookFeatures.Count;

        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        var start = stream.Position;
        stream.Seek(128, SeekOrigin.Current);

        RelativePathBufferOffset = (ulong)(stream.Position - start);

        foreach (var earc in Earcs)
        {
            earc.FileCount = (uint)earc.Files.Count;
            earc.RelativePathOffset = (uint)((ulong)stream.Position - RelativePathBufferOffset - (ulong)start);
            writer.WriteNullTerminatedString(earc.RelativePath);
        }

        foreach (var file in Files)
        {
            file.RelativePathOffset = (uint)((ulong)stream.Position - RelativePathBufferOffset - (ulong)start);
            writer.WriteNullTerminatedString(file.RelativePath);
        }

        writer.Align(128, 0x00);

        UriBufferOffset = (ulong)(stream.Position - start);

        foreach (var earc in Earcs)
        {
            foreach (var file in earc.Files)
            {
                file.UriOffset = (uint)((ulong)stream.Position - UriBufferOffset - (ulong)start);
                writer.WriteNullTerminatedString(file.Uri);
            }
        }

        writer.Align(128, 0x00);

        FileNameBufferOffset = (ulong)(stream.Position - start);

        foreach (var file in Files)
        {
            file.FileNameOffset = (uint)((ulong)stream.Position - FileNameBufferOffset - (ulong)start);
            writer.WriteNullTerminatedString(file.FileName);
        }

        writer.Align(128, 0x00);

        EarcsOffset = (ulong)(stream.Position - start);

        stream.Seek(16 * EarcCount + 24 * Earcs.Sum(e => e.FileCount), SeekOrigin.Current);
        stream.Align(128);

        FilesOffset = (ulong)(stream.Position - start);

        stream.Seek(20 * LooseFileCount, SeekOrigin.Current);
        stream.Align(128);

        HookFeaturesOffset = (ulong)(stream.Position - start);

        foreach (var feature in HookFeatures)
        {
            feature.Write(writer);
        }

        stream.Align(128);

        DataOffset = (ulong)(stream.Position - start);

        foreach (var earc in Earcs)
        {
            foreach (var file in earc.Files.Where(f =>
                         f.Type is LegacyModBuildInstruction.AddPackedFile
                             or LegacyModBuildInstruction.ReplacePackedFile
                             or LegacyModBuildInstruction.AddToPackedTextureArray))
            {
                file.DataOffset = (uint)((ulong)stream.Position - DataOffset - (ulong)start);

                using var fileStream = File.OpenRead(file.DataSource);
                file.Size = (uint)fileStream.Length;
                fileStream.CopyTo(stream);
                writer.Align(128, 0x00);
            }
        }

        foreach (var file in Files)
        {
            file.DataOffset = (uint)((ulong)stream.Position - DataOffset - (ulong)start);
            using var fileStream = File.OpenRead(file.DataSource);
            file.Size = (uint)fileStream.Length;
            fileStream.CopyTo(stream);
            writer.Align(128, 0x00);
        }

        ThumbnailOffset = (ulong)(stream.Position - start);

        using var thumbnailStream = File.OpenRead(ThumbnailDataSource);
        ThumbnailSize = (uint)thumbnailStream.Length;
        thumbnailStream.CopyTo(stream);
        writer.Align(128, 0x00);

        StringBufferOffset = (ulong)(stream.Position - start);

        writer.WriteNullTerminatedString(Name);
        AuthorOffset = (uint)((ulong)stream.Position - StringBufferOffset - (ulong)start);
        writer.WriteNullTerminatedString(Author);
        DescriptionOffset = (uint)((ulong)stream.Position - StringBufferOffset - (ulong)start);
        writer.WriteNullTerminatedString(Description);
        ReadmeOffset = (uint)((ulong)stream.Position - StringBufferOffset - (ulong)start);
        writer.WriteNullTerminatedString(Readme);

        writer.Align(128, 0x00);
        var end = stream.Position;

        stream.Seek((long)(EarcsOffset + (ulong)start), SeekOrigin.Begin);

        foreach (var earc in Earcs)
        {
            earc.Write(writer);
        }

        writer.Align(128, 0x00);

        stream.Seek((long)(FilesOffset + (ulong)start), SeekOrigin.Begin);

        foreach (var file in Files)
        {
            file.Write(writer);
        }

        stream.Seek(start, SeekOrigin.Begin);

        writer.Write(Magic);
        writer.Write(Version);

        if (Version >= 2)
        {
            writer.Write(Guid?.ToByteArray() ?? new byte[16]);
            writer.Write((int)Flags);
        }

        writer.Write((int)Category);
        writer.Write(EarcCount);
        writer.Write(LooseFileCount);
        writer.Write(HookFeaturesCount);
        writer.Write(RelativePathBufferOffset);
        writer.Write(UriBufferOffset);
        writer.Write(FileNameBufferOffset);
        writer.Write(EarcsOffset);
        writer.Write(FilesOffset);
        writer.Write(HookFeaturesOffset);
        writer.Write(DataOffset);
        writer.Write(ThumbnailOffset);
        writer.Write(ThumbnailSize);
        writer.Write(StringBufferOffset);
        writer.Write(NameOffset);
        writer.Write(AuthorOffset);
        writer.Write(DescriptionOffset);
        writer.Write(ReadmeOffset);

        stream.Seek(end, SeekOrigin.Begin);

        // If this is a protected official mod, write a protected copy
        // if (Flags.HasFlag(ModFlags.Protected) && Guid != null && EarlyAccessMods.GuidMap.ContainsKey(Guid.Value))
        // {
        //     var officialMod = EarlyAccessMods.GuidMap[Guid.Value];
        //     var aes = Aes.Create();
        //     aes.Key = officialMod.Key;
        //     aes.IV = officialMod.IV;
        //     aes.Padding = PaddingMode.Zeros;
        //
        //     stream.Seek(0, SeekOrigin.Begin);
        //
        //     // Don't protect the header
        //     using var protectedStream = new FileStream(outputPath.Insert(outputPath.LastIndexOf('.'), "_protected"),
        //         FileMode.Create, FileAccess.Write);
        //     stream.CopyTo(protectedStream, 128L);
        //
        //     // Protect everything else
        //     using var cryptoStream =
        //         new CryptoStream(protectedStream, aes.CreateEncryptor(), CryptoStreamMode.Write, true);
        //     stream.CopyTo(cryptoStream);
        // }
    }
}