using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.AssetExplorer.Indexing;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;

namespace Flagrum.Application.Features.ModManager.Services;

public class ForspokenModManager : ModManagerServiceBase
{
    public ForspokenModManager(
        IProfileService profile,
        IFileIndex fileIndex,
        AssetConverter assetConverter,
        IServiceProvider provider)
        : base(profile, fileIndex, assetConverter, provider)
    {
        var modDirectory = $@"{_profile.GameDataDirectory}\mods";
        if (!Directory.Exists(modDirectory))
        {
            Directory.CreateDirectory(modDirectory);
        }
    }

    protected override Task ApplyMod(IFlagrumProject mod, EbonyArchiveManager archiveManager)
    {
        var c000 = $@"{_profile.GameDataDirectory}\c000.earc";
        var backup = $@"{_profile.GameDataDirectory}\c000.backup";

        if (!File.Exists(backup))
        {
            File.Copy(c000, backup);
        }

        using var archive = new EbonyArchive(c000);
        var erepEntry = archive["data://c000.erep"];
        var erep = new EbonyReplace(erepEntry.GetReadableData());

        using (var modArchive = new EbonyArchive(false))
        {
            foreach (var file in mod.Archives.SelectMany(e => e.Instructions))
            {
                var newHash = 0UL;

                switch (file)
                {
                    case AddPackedFileBuildInstruction:
                    case ReplacePackedFileBuildInstruction:
                        var packedAssetInstruction = (PackedAssetBuildInstruction)file;
                        var hash = Cryptography.HashFileUri64(file.Uri);
                        var cachePath = $@"{_profile.CacheDirectory}\{mod.Identifier}{hash}.ffg";
                        var fragment = new FmodFragment();
                        fragment.Read(packedAssetInstruction.FilePath.EndsWith(".ffg")
                            ? packedAssetInstruction.FilePath
                            : cachePath);
                        var newUri = $"data://mods/{Guid.NewGuid()}.{file.Uri.Split('.').Last()}";
                        modArchive.AddProcessedFile(newUri, fragment.Flags, fragment.Data, fragment.OriginalSize,
                            fragment.Key, fragment.RelativePath);
                        newHash = modArchive[newUri].Id;
                        break;
                    case AddReferenceBuildInstruction:
                        throw new Exception(
                            "Forspoken doesn't need references since the whole earc tree is loaded on startup");
                    case RemovePackedFileBuildInstruction:
                        // Removals are handled purely via EREP, so there's nothing to do here
                        break;
                    default:
                        throw new Exception($"Unsupported build instruction {file.GetType().Name}");
                }

                if (file is ReplacePackedFileBuildInstruction or RemovePackedFileBuildInstruction)
                {
                    var originalHash = Cryptography.HashFileUri64(file.Uri);

                    // Add a pointer from the original file to the new file
                    erep.Replacements[originalHash] = newHash;

                    // Point all existing pointers to the original file to the new file as well
                    foreach (var key in erep.Replacements
                                 .Where(kvp => kvp.Value == originalHash)
                                 .Select(kvp => kvp.Key))
                    {
                        erep.Replacements[key] = newHash;
                    }
                }
            }

            modArchive.WriteToFile($@"{_profile.GameDataDirectory}\mods\{mod.Identifier}.earc", LuminousGame.Forspoken);
        }

        erepEntry.SetRawData(erep.ToArray());
        archive.AddFile($"data://mods/{mod.Identifier}.ebex@",
            EbonyArchiveFileFlags.Autoload | EbonyArchiveFileFlags.Reference,
            []);
        archive.WriteToSource(LuminousGame.Forspoken);

        return Task.CompletedTask;
    }

    protected override Task RevertMod(IFlagrumProject mod)
    {
        using var archive = new EbonyArchive($@"{_profile.GameDataDirectory}\c000.earc");
        var erepEntry = archive["data://c000.erep"];
        var erep = new EbonyReplace(erepEntry.GetReadableData());

        foreach (var file in mod.Archives
                     .SelectMany(e => e.Instructions
                         .Where(f => f is ReplacePackedFileBuildInstruction
                             or RemovePackedFileBuildInstruction)))
        {
            var originalHash = Cryptography.HashFileUri64(file.Uri);
            if (erep.Replacements.ContainsKey(originalHash))
            {
                // TODO: Backup the original hashes so they can be put back here
                erep.Replacements.Remove(originalHash);
            }
        }

        erepEntry.SetRawData(erep.ToArray());
        archive.RemoveFile($"data://mods/{mod.Identifier}.ebex@");
        archive.WriteToSource(LuminousGame.Forspoken);

        var modPath = $@"{_profile.GameDataDirectory}\mods\{mod.Identifier}.earc";
        if (File.Exists(modPath))
        {
            File.Delete(modPath);
        }

        return Task.CompletedTask;
    }

    public override Guid[] Reset()
    {
        var enabledMods = Projects.Values
            .Where(p => ModsState.GetActive(p.Identifier))
            .Select(p => p.Identifier)
            .ToArray();

        // Delete all mod files
        var modDirectory = Path.Combine(_profile.GameDataDirectory, "mods");
        if (Directory.Exists(modDirectory))
        {
            Directory.Delete(modDirectory, true);
        }

        // Revert c000.earc
        var c000Backup = Path.Combine(_profile.GameDataDirectory, "c000.backup");
        var c000 = Path.Combine(_profile.GameDataDirectory, "c000.earc");
        if (File.Exists(c000Backup))
        {
            File.Delete(c000);
            File.Copy(c000Backup, c000);
        }

        // Mark all mods as disabled
        ModsState.SetAllInactive();

        return enabledMods;
    }
}