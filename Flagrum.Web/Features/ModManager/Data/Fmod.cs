using System;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Exceptions;
using Flagrum.Web.Persistence.Entities.ModManager;

namespace Flagrum.Web.Features.ModManager.Data;

public class Fmod
{
    public const uint CurrentFmodVersion = 1;

    public char[] Magic { get; private set; } = "FMOD".ToCharArray();
    public uint Version { get; private set; } = CurrentFmodVersion;
    public ModCategory Category { get; set; }
    public uint EarcCount { get; set; }
    public uint LooseFileCount { get; set; }
    public ulong RelativePathBufferOffset { get; set; }
    public ulong UriBufferOffset { get; set; }
    public ulong FileNameBufferOffset { get; set; }
    public ulong EarcsOffset { get; set; }
    public ulong FilesOffset { get; set; }
    public ulong DataOffset { get; set; }
    public ulong ThumbnailOffset { get; set; }
    public uint ThumbnailSize { get; set; }
    public ulong StringBufferOffset { get; set; }

    public uint NameOffset { get; set; }
    public uint AuthorOffset { get; set; }
    public uint DescriptionOffset { get; set; }
    public uint ReadmeOffset { get; set; }

    public FmodEarc[] Earcs { get; set; }
    public FmodFile[] Files { get; set; }

    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Readme { get; set; }
    public string ThumbnailDataSource { get; set; }

    public void Read(string inputPath, string earcModDirectory, string thumbnailPath)
    {
        using var stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        Magic = reader.ReadChars(4);
        if (Magic[0] != 'F' || Magic[1] != 'M' || Magic[2] != 'O' || Magic[3] != 'D')
        {
            throw new FileFormatException("Input file was not a valid FMOD file");
        }

        Version = reader.ReadUInt32();
        if (Version > CurrentFmodVersion)
        {
            throw new FormatVersionException("FMOD version newer than current supported version");
        }

        Category = (ModCategory)reader.ReadInt32();
        EarcCount = reader.ReadUInt32();
        LooseFileCount = reader.ReadUInt32();
        RelativePathBufferOffset = reader.ReadUInt64();
        UriBufferOffset = reader.ReadUInt64();
        FileNameBufferOffset = reader.ReadUInt64();
        EarcsOffset = reader.ReadUInt64();
        FilesOffset = reader.ReadUInt64();
        DataOffset = reader.ReadUInt64();
        ThumbnailOffset = reader.ReadUInt64();
        ThumbnailSize = reader.ReadUInt32();
        StringBufferOffset = reader.ReadUInt64();
        NameOffset = reader.ReadUInt32();
        AuthorOffset = reader.ReadUInt32();
        DescriptionOffset = reader.ReadUInt32();
        ReadmeOffset = reader.ReadUInt32();

        stream.Seek((long)EarcsOffset, SeekOrigin.Begin);

        Earcs = new FmodEarc[EarcCount];
        for (var i = 0; i < EarcCount; i++)
        {
            Earcs[i] = new FmodEarc();
            Earcs[i].Read(reader);
        }

        stream.Seek((long)FilesOffset, SeekOrigin.Begin);

        Files = new FmodFile[LooseFileCount];
        for (var i = 0; i < LooseFileCount; i++)
        {
            Files[i] = new FmodFile();
            Files[i].Read(reader);
        }

        stream.Seek((long)(StringBufferOffset + NameOffset), SeekOrigin.Begin);
        Name = reader.ReadNullTerminatedString();
        stream.Seek((long)(StringBufferOffset + AuthorOffset), SeekOrigin.Begin);
        Author = reader.ReadNullTerminatedString();
        stream.Seek((long)(StringBufferOffset + DescriptionOffset), SeekOrigin.Begin);
        Description = reader.ReadNullTerminatedString();
        stream.Seek((long)(StringBufferOffset + ReadmeOffset), SeekOrigin.Begin);
        Readme = reader.ReadNullTerminatedString();

        foreach (var earc in Earcs)
        {
            stream.Seek((long)RelativePathBufferOffset + earc.RelativePathOffset, SeekOrigin.Begin);
            earc.RelativePath = reader.ReadNullTerminatedString();
        }

        foreach (var file in Files)
        {
            stream.Seek((long)RelativePathBufferOffset + file.RelativePathOffset, SeekOrigin.Begin);
            file.RelativePath = reader.ReadNullTerminatedString();
        }

        foreach (var earc in Earcs)
        {
            foreach (var file in earc.Files)
            {
                stream.Seek((long)UriBufferOffset + file.UriOffset, SeekOrigin.Begin);
                file.Uri = reader.ReadNullTerminatedString();
            }
        }

        foreach (var file in Files)
        {
            stream.Seek((long)FileNameBufferOffset + file.FileNameOffset, SeekOrigin.Begin);
            file.FileName = reader.ReadNullTerminatedString();
        }

        var index = 0;
        foreach (var file in Earcs.SelectMany(e => e.Files
                     .Where(f => f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                         or EarcFileChangeType.AddToTextureArray)))
        {
            var path = $@"{earcModDirectory}\{++index}.ffg";
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.Seek((long)(DataOffset + file.DataOffset), SeekOrigin.Begin);
            stream.CopyTo(fileStream, file.Size);
        }

        foreach (var file in Files)
        {
            var path = $@"{earcModDirectory}\{file.FileName}";
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.Seek((long)(DataOffset + file.DataOffset), SeekOrigin.Begin);
            stream.CopyTo(fileStream, file.Size);
        }

        using var thumbnailStream = new FileStream(thumbnailPath, FileMode.Create, FileAccess.Write);
        stream.Seek((long)ThumbnailOffset, SeekOrigin.Begin);
        stream.CopyTo(thumbnailStream, ThumbnailSize);
    }

    public void Write(string outputPath)
    {
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        stream.Seek(128, SeekOrigin.Begin);

        RelativePathBufferOffset = (ulong)stream.Position;

        foreach (var earc in Earcs)
        {
            earc.RelativePathOffset = (uint)((ulong)stream.Position - RelativePathBufferOffset);
            writer.WriteNullTerminatedString(earc.RelativePath);
        }

        foreach (var file in Files)
        {
            file.RelativePathOffset = (uint)((ulong)stream.Position - RelativePathBufferOffset);
            writer.WriteNullTerminatedString(file.RelativePath);
        }

        writer.Align(128, 0x00);

        UriBufferOffset = (ulong)stream.Position;

        foreach (var earc in Earcs)
        {
            foreach (var file in earc.Files)
            {
                file.UriOffset = (uint)((ulong)stream.Position - UriBufferOffset);
                writer.WriteNullTerminatedString(file.Uri);
            }
        }

        writer.Align(128, 0x00);

        FileNameBufferOffset = (ulong)stream.Position;

        foreach (var file in Files)
        {
            file.FileNameOffset = (uint)((ulong)stream.Position - FileNameBufferOffset);
            writer.WriteNullTerminatedString(file.FileName);
        }

        writer.Align(128, 0x00);

        EarcsOffset = (ulong)stream.Position;

        stream.Seek(16 * EarcCount + 24 * Earcs.Sum(e => e.FileCount), SeekOrigin.Current);
        stream.Align(128);

        FilesOffset = (ulong)stream.Position;

        stream.Seek(20 * LooseFileCount, SeekOrigin.Current);
        stream.Align(128);

        DataOffset = (ulong)stream.Position;

        foreach (var earc in Earcs)
        {
            foreach (var file in earc.Files.Where(f =>
                         f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                             or EarcFileChangeType.AddToTextureArray))
            {
                file.DataOffset = (uint)((ulong)stream.Position - DataOffset);

                using var fileStream = File.OpenRead(file.DataSource);
                file.Size = (uint)fileStream.Length;
                fileStream.CopyTo(stream);
                writer.Align(128, 0x00);
            }
        }

        foreach (var file in Files)
        {
            file.DataOffset = (uint)((ulong)stream.Position - DataOffset);
            using var fileStream = File.OpenRead(file.DataSource);
            file.Size = (uint)fileStream.Length;
            fileStream.CopyTo(stream);
            writer.Align(128, 0x00);
        }

        ThumbnailOffset = (ulong)stream.Position;

        using var thumbnailStream = File.OpenRead(ThumbnailDataSource);
        ThumbnailSize = (uint)thumbnailStream.Length;
        thumbnailStream.CopyTo(stream);
        writer.Align(128, 0x00);

        StringBufferOffset = (ulong)stream.Position;

        writer.WriteNullTerminatedString(Name);
        AuthorOffset = (uint)((ulong)stream.Position - StringBufferOffset);
        writer.WriteNullTerminatedString(Author);
        DescriptionOffset = (uint)((ulong)stream.Position - StringBufferOffset);
        writer.WriteNullTerminatedString(Description);
        ReadmeOffset = (uint)((ulong)stream.Position - StringBufferOffset);
        writer.WriteNullTerminatedString(Readme);

        writer.Align(128, 0x00);

        stream.Seek((long)EarcsOffset, SeekOrigin.Begin);

        foreach (var earc in Earcs)
        {
            earc.Write(writer);
        }

        writer.Align(128, 0x00);

        stream.Seek((long)FilesOffset, SeekOrigin.Begin);

        foreach (var file in Files)
        {
            file.Write(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        writer.Write(Magic);
        writer.Write(Version);
        writer.Write((int)Category);
        writer.Write(EarcCount);
        writer.Write(LooseFileCount);
        writer.Write(RelativePathBufferOffset);
        writer.Write(UriBufferOffset);
        writer.Write(FileNameBufferOffset);
        writer.Write(EarcsOffset);
        writer.Write(FilesOffset);
        writer.Write(DataOffset);
        writer.Write(ThumbnailOffset);
        writer.Write(ThumbnailSize);
        writer.Write(StringBufferOffset);
        writer.Write(NameOffset);
        writer.Write(AuthorOffset);
        writer.Write(DescriptionOffset);
        writer.Write(ReadmeOffset);
    }

    public static Fmod FromEarcMod(EarcMod mod, string thumbnailPath, string cacheDirectory)
    {
        var fmod = new Fmod
        {
            Category = mod.Category,
            EarcCount = (uint)mod.Earcs.Count,
            LooseFileCount = (uint)mod.LooseFiles.Count,
            Earcs = mod.Earcs.Select(e => new FmodEarc
            {
                Type = e.Type,
                Flags = e.Flags,
                FileCount = (uint)e.Files.Count,
                Files = e.Files.Select(f => new FmodEarcFile
                {
                    Type = f.Type,
                    Flags = f.Flags,
                    Uri = f.Uri,
                    DataSource = f.Type is EarcFileChangeType.Remove or EarcFileChangeType.AddReference
                        ? null
                        : f.ReplacementFilePath.EndsWith(".ffg")
                            ? f.ReplacementFilePath
                            : $@"{cacheDirectory}\{mod.Id}{f.Id}{Cryptography.HashFileUri64(f.Uri)}.ffg"
                }).ToArray(),
                RelativePath = e.EarcRelativePath
            }).ToArray(),
            Files = mod.LooseFiles.Select(f => new FmodFile
            {
                Type = f.Type,
                RelativePath = f.RelativePath,
                FileName = f.FilePath.Split('\\').Last(),
                DataSource = f.FilePath
            }).ToArray(),
            Name = mod.Name,
            Author = mod.Author,
            Description = mod.Description,
            ThumbnailDataSource = thumbnailPath,
            Readme = mod.Readme
        };

        var problems = fmod.Files.Where(f =>
                fmod.Files.Count(g => g.FileName.Equals(f.FileName, StringComparison.OrdinalIgnoreCase)) > 1)
            .ToList();

        foreach (var name in problems.Select(p => p.FileName).Distinct())
        {
            var counter = 0;
            foreach (var problem in problems.Where(p => p.FileName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                counter++;
                if (counter == 1)
                {
                    continue;
                }

                problem.FileName = name + "_" + counter;
            }
        }

        return fmod;
    }

    public static EarcMod ToEarcMod(Fmod mod, string projectDirectory)
    {
        var index = 0;
        var earcMod = new EarcMod
        {
            Category = mod.Category,
            Earcs = mod.Earcs.Select(e => new EarcModEarc
            {
                Type = e.Type,
                Flags = e.Flags,
                Files = e.Files.Select(f => new EarcModFile
                {
                    Type = f.Type,
                    Flags = f.Flags,
                    ReplacementFilePath = f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                        or EarcFileChangeType.AddToTextureArray
                        ? $@"{projectDirectory}\{++index}.ffg"
                        : null,
                    Uri = f.Uri
                }).ToList(),
                EarcRelativePath = e.RelativePath
            }).ToList(),
            LooseFiles = mod.Files.Select(f => new EarcModLooseFile
            {
                Type = f.Type,
                RelativePath = f.RelativePath,
                FilePath = $@"{projectDirectory}\{f.FileName}"
            }).ToList(),
            Name = mod.Name,
            Author = mod.Author,
            Description = mod.Description,
            Readme = mod.Readme
        };

        return earcMod;
    }
}