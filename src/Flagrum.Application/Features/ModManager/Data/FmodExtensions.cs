using System.IO;
using System.Linq;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Features.ModManager.Project;

namespace Flagrum.Application.Features.ModManager.Data;

public static class FmodExtensions
{
    public static FlagrumMod FromFlagrumProject(IFlagrumProject mod, string thumbailPath, string cacheDirectory)
    {
        var fmod = new FlagrumMod
        {
            Metadata = new FlagrumModMetadata
            {
                Guid = mod.Identifier,
                Flags = mod.Flags,
                Archives = mod.Archives,
                Instructions = mod.Instructions,
                Name = mod.Name,
                Author = mod.Author,
                Description = mod.Description,
                Readme = mod.Readme
            },
            ThumbnailPath = thumbailPath
        };

        foreach (var instruction in fmod.Metadata.Archives.SelectMany(archive =>
                     archive.Instructions.OfType<PackedAssetBuildInstruction>()))
        {
            instruction.DataSource = instruction.FilePath.EndsWith(".ffg")
                ? instruction.FilePath
                : Path.Combine(cacheDirectory, $"{mod.Identifier}{Cryptography.HashFileUri64(instruction.Uri)}.ffg");
        }

        return fmod;
    }

    public static FlagrumProject ToFlagrumProject(FlagrumMod mod) =>
        new()
        {
            Identifier = mod.Metadata.Guid,
            Flags = mod.Metadata.Flags,
            Archives = mod.Metadata.Archives,
            Instructions = mod.Metadata.Instructions,
            Name = mod.Metadata.Name,
            Author = mod.Metadata.Author,
            Description = mod.Metadata.Description,
            Readme = mod.Metadata.Readme
        };
}