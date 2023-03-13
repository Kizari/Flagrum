using System;
using System.IO;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Web.Features.ModManager.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;

namespace Flagrum.Web.Features.ModManager.Services;

public class ForspokenModManager : ModManagerServiceBase
{
    public ForspokenModManager(
        ProfileService profile,
        FlagrumDbContext context,
        AppStateService appState,
        AssetConverter assetConverter)
        : base(profile, context, appState, assetConverter)
    {
        var modDirectory = $@"{_profile.GameDataDirectory}\mods";
        if (!Directory.Exists(modDirectory))
        {
            Directory.CreateDirectory(modDirectory);
        }
    }

    public override void BuildAndApplyMod(EarcMod mod, EbonyArchiveManager archiveManager)
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
            foreach (var file in mod.Earcs.SelectMany(e => e.Files))
            {
                var newHash = 0UL;

                switch (file.Type)
                {
                    case EarcFileChangeType.Add:
                    case EarcFileChangeType.Replace:
                        var hash = Cryptography.HashFileUri64(file.Uri);
                        var cachePath = $@"{_profile.CacheDirectory}\{mod.Id}{file.Id}{hash}.ffg";
                        var fragment = new FmodFragment();
                        fragment.Read(file.ReplacementFilePath.EndsWith(".ffg")
                            ? file.ReplacementFilePath
                            : cachePath);
                        var newUri = $"data://mods/{file.Id}.{file.Uri.Split('.').Last()}";
                        modArchive.AddProcessedFile(newUri, fragment.Flags, fragment.Data, fragment.OriginalSize,
                            fragment.Key, fragment.RelativePath);
                        newHash = modArchive[newUri].UriAndTypeHash;
                        break;
                    case EarcFileChangeType.AddReference:
                        throw new ArgumentOutOfRangeException(nameof(file.Type), file.Type,
                            @"Forspoken doesn't need references since the whole earc tree is loaded on startup");
                    case EarcFileChangeType.Remove:
                        // Removals are handled purely via EREP, so there's nothing to do here
                        break;
                    case EarcFileChangeType.AddToTextureArray:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(file.Type), file.Type,
                            $@"Unsupported {nameof(EarcFileChangeType)}");
                }

                if (file.Type is EarcFileChangeType.Replace or EarcFileChangeType.Remove)
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

            modArchive.WriteToFile($@"{_profile.GameDataDirectory}\mods\{mod.Id}.earc", LuminousGame.Forspoken);
        }

        erepEntry.SetRawData(erep.ToArray());
        archive.AddFile($"data://mods/{mod.Id}.ebex@", ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference,
            Array.Empty<byte>());
        archive.WriteToSource(LuminousGame.Forspoken);
    }

    public override void RevertMod(EarcMod mod)
    {
        using var archive = new EbonyArchive($@"{_profile.GameDataDirectory}\c000.earc");
        var erepEntry = archive["data://c000.erep"];
        var erep = new EbonyReplace(erepEntry.GetReadableData());

        foreach (var file in mod.Earcs
                     .SelectMany(e => e.Files
                         .Where(f => f.Type is EarcFileChangeType.Replace or EarcFileChangeType.Remove)))
        {
            var originalHash = Cryptography.HashFileUri64(file.Uri);
            if (erep.Replacements.ContainsKey(originalHash))
            {
                // TODO: Backup the original hashes so they can be put back here
                erep.Replacements.Remove(originalHash);
            }
        }

        erepEntry.SetRawData(erep.ToArray());
        archive.RemoveFile($"data://mods/{mod.Id}.ebex@");
        archive.WriteToSource(LuminousGame.Forspoken);

        var modPath = $@"{_profile.GameDataDirectory}\mods\{mod.Id}.earc";
        if (File.Exists(modPath))
        {
            File.Delete(modPath);
        }
    }
}