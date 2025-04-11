using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Utilities.Exceptions;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Project;

namespace Flagrum.Application.Features.ModManager.Mod.Legacy;

public class Fmod3
{
    public char[] Magic { get; private set; } = "FMOD".ToCharArray();
    public uint Version { get; private set; } = Fmod.CurrentFmodVersion;
    public uint Count { get; set; }
    public ulong[] Offsets { get; set; }

    public List<Fmod> Mods { get; set; } = new();

    public void Write(string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        if (Version < 3)
        {
            throw new Exception("Please use Fmod class for versions below 3");
        }

        writer.Write(Magic);
        writer.Write(Version);

        Count = (uint)Mods.Count;
        writer.Write(Count);

        var offsetsOffset = stream.Position;
        stream.Seek(Mods.Count * sizeof(ulong), SeekOrigin.Current);
        writer.Align(128, 0x00);

        Offsets = new ulong[Count];
        for (var i = 0; i < Count; i++)
        {
            Offsets[i] = (ulong)stream.Position;
            Mods[i].Write(stream);
            writer.Align(128, 0x00);
        }

        stream.Seek(offsetsOffset, SeekOrigin.Begin);
        foreach (var offset in Offsets)
        {
            writer.Write(offset);
        }
    }

    public void Read(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            using var reader = new BinaryReader(stream);

            Magic = reader.ReadChars(4);
            if (Magic[0] != 'F' || Magic[1] != 'M' || Magic[2] != 'O' || Magic[3] != 'D')
            {
                throw new FileFormatException("Input file was not a valid FMOD file");
            }

            Version = reader.ReadUInt32();
            switch (Version)
            {
                case > Fmod.CurrentFmodVersion:
                    throw new FormatVersionException("FMOD version newer than current supported version");
                case < 3:
                {
                    Offsets = new[] {0UL};
                    break;
                }
                default:
                {
                    Count = reader.ReadUInt32();
                    Offsets = new ulong[Count];

                    for (var i = 0; i < Count; i++)
                    {
                        Offsets[i] = reader.ReadUInt64();
                    }

                    break;
                }
            }
        }

        foreach (var offset in Offsets)
        {
            var mod = new Fmod();
            mod.Read(offset, path);
            Mods.Add(mod);
        }
    }

    public static void ToFlagrumModPack(string path, FlagrumModPack modPack, IModBuildInstructionFactory factory)
    {
        var mod = new Fmod3();
        mod.Read(path);

        modPack.Magic = new string(mod.Magic);
        modPack.Version = mod.Version;
        modPack.Count = mod.Count;
        modPack.Mods = mod.Mods.Select(m => new FlagrumMod
        {
            Metadata = new FlagrumModMetadata
            {
                Guid = m.Guid ?? Guid.NewGuid(),
                Flags = m.Flags,
                Archives = m.Earcs.Select(IFlagrumProjectArchive (e) => new FlagrumProjectArchive
                {
                    Type = e.Type,
                    RelativePath = e.RelativePath.Replace('\\', '/'),
                    Flags = e.Flags,
                    Instructions = e.Files.Select(IPackedBuildInstruction (f) =>
                    {
                        PackedBuildInstruction instruction = f.Type switch
                        {
                            LegacyModBuildInstruction.ReplacePackedFile => factory
                                .Create<ReplacePackedFileBuildInstruction>(),
                            LegacyModBuildInstruction.AddPackedFile => factory.Create<AddPackedFileBuildInstruction>(),
                            LegacyModBuildInstruction.RemovePackedFile => factory
                                .Create<RemovePackedFileBuildInstruction>(),
                            LegacyModBuildInstruction.AddReference => factory.Create<AddReferenceBuildInstruction>(),
                            LegacyModBuildInstruction.AddToPackedTextureArray => factory
                                .Create<AddToPackedTextureArrayBuildInstruction>(),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        instruction.Uri = f.Uri;
                        instruction.Flags = f.Flags;

                        return instruction;
                    }).ToList()
                }).ToList(),
                Instructions = m.Files.Select(IModBuildInstruction (f) =>
                {
                    LooseAssetBuildInstruction instruction = f.Type switch
                    {
                        ModChangeType.Change => factory.Create<ReplaceLooseFileBuildInstruction>(),
                        ModChangeType.Create => factory.Create<AddLooseFileBuildInstruction>(),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    instruction.RelativePath = f.RelativePath.Replace('\\', '/');
                    return instruction;
                }).ToList(),
                Name = m.Name,
                Author = m.Author,
                Description = m.Description,
                Readme = m.Readme
            }
        }).ToList();

        // Generate file table
        for (var m = 0; m < mod.Mods.Count; m++)
        {
            modPack.Mods[m].FileTable = new Dictionary<IModBuildInstruction, (ulong Offset, uint Size)>();

            // Add packed instructions to table
            for (var a = 0; a < mod.Mods[m].Earcs.Count; a++)
            {
                for (var f = 0; f < mod.Mods[m].Earcs[a].Files.Count; f++)
                {
                    if (modPack.Mods[m].Metadata.Archives[a].Instructions[f] is PackedAssetBuildInstruction packed)
                    {
                        var offset = mod.Mods[m].GetFileOffset(mod.Mods[m].Earcs[a].Files[f]);
                        var size = mod.Mods[m].Earcs[a].Files[f].Size;
                        modPack.Mods[m].FileTable.Add(packed, (offset, size));
                    }
                }
            }

            // Add loose instructions to table
            for (var f = 0; f < mod.Mods[m].Files.Count; f++)
            {
                if (modPack.Mods[m].Metadata.Instructions[f] is LooseAssetBuildInstruction loose)
                {
                    var offset = mod.Mods[m].GetFileOffset(mod.Mods[m].Files[f]);
                    var size = mod.Mods[m].Files[f].Size;
                    modPack.Mods[m].FileTable.Add(loose, (offset, size));
                }
            }

            modPack.Mods[m].ThumbnailOffset = mod.Mods[m].GetThumbnailOffset();
            modPack.Mods[m].ThumbnailSize = mod.Mods[m].ThumbnailSize;
        }
    }
}