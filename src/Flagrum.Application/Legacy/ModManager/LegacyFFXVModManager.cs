using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities.ModManager;
using Flagrum.Application.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Microsoft.Extensions.Logging;

namespace Flagrum.Application.Features.ModManager.Services;

public class LegacyFFXVModManager(
    IProfileService profile,
    FlagrumDbContext context,
    ILogger<LegacyFFXVModManager> logger)
    : LegacyModManagerServiceBase(profile, context)
{
    public override async Task RevertMod(EarcMod mod)
    {
        var earcs = new ConcurrentDictionary<string, string>();

        // Repack all earcs with their original files
        Parallel.ForEach(mod.Earcs, earc =>
        {
            var earcPath = $@"{_context.Profile.GameDataDirectory}\{earc.EarcRelativePath}";
            var stagingPath = $@"{_context.Profile.ModStagingDirectory}\{earc.Id}.earc";

            if (earc.Type == ModChangeType.Create)
            {
                File.Delete(earcPath);
                //IOHelper.DeleteEmptyDirectoriesInPath(_context.Profile.GameDataDirectory, earcPath);
                return;
            }

            // Skip if the 4K pack is not present to prevent crash
            if (!File.Exists(earcPath) && (earcPath.Contains(@"\highimages\") || earcPath.EndsWith("_$h2.earc")))
            {
                return;
            }

            using var archive = new EbonyArchive(earcPath);

            foreach (var replacement in earc.Files)
            {
                if (replacement.Type is LegacyModBuildInstruction.AddReference
                    or LegacyModBuildInstruction.AddPackedFile)
                {
                    if (archive.HasFile(replacement.Uri))
                    {
                        archive.RemoveFile(replacement.Uri);
                    }

                    continue;
                }

                var hash = Cryptography.HashFileUri64(replacement.Uri);
                var fragment = new FmodFragment();
                fragment.Read($@"{_context.Profile.EarcModBackupsDirectory}\{hash}.ffg");

                switch (replacement.Type)
                {
                    case LegacyModBuildInstruction.ReplacePackedFile
                        or LegacyModBuildInstruction.AddToPackedTextureArray:
                        archive.UpdateFileWithProcessedData(replacement.Uri, fragment.OriginalSize, fragment.Data);
                        break;
                    case LegacyModBuildInstruction.RemovePackedFile:
                        archive.AddFileFromBackup(replacement.Uri, fragment.RelativePath, fragment.OriginalSize,
                            fragment.Flags, fragment.Key, fragment.Data);
                        break;
                }
            }

            archive.WriteToFile(stagingPath, LuminousGame.FFXV);
            earcs.TryAdd(earcPath, stagingPath);
        });

        // Move the repacked earcs into place now that they have all repacked successfully
        foreach (var (earcPath, stagingPath) in earcs)
        {
            await logger.ExecuteWithRetryAsync("Move staged earc to final position (RevertMod)", 10,
                TimeSpan.FromMilliseconds(200), () =>
                {
                    // This is an attempt to fix an inconsistent System.UnauthorizedAccessException
                    // See https://github.com/Kizari/Flagrum/issues/60
                    IOHelper.SetFileAttributesNormal(stagingPath);
                    IOHelper.SetFileAttributesNormal(earcPath);

                    File.Move(stagingPath, earcPath, true);
                });
        }

        // Revert loose files
        foreach (var file in mod.LooseFiles)
        {
            var path = $@"{_context.Profile.GameDataDirectory}\{file.RelativePath}";
            if (File.Exists(path))
            {
                File.Delete(path);
                IOHelper.DeleteEmptyDirectoriesInPath(_context.Profile.GameDataDirectory, path);
            }

            if (file.Type == ModChangeType.Change)
            {
                // Restore the backup
                var fileName = file.RelativePath.ToBase64();
                var backupPath = $@"{_context.Profile.EarcModBackupsDirectory}\{fileName}";
                File.Copy(backupPath, path);
                File.Delete(backupPath);
            }
        }

        // Now that the mod has been successfully reverted, remove the backup files
        foreach (var earc in mod.Earcs)
        {
            foreach (var replacement in earc.Files.Where(r =>
                         r.Type is LegacyModBuildInstruction.RemovePackedFile
                             or LegacyModBuildInstruction.ReplacePackedFile))
            {
                var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                var backupFilePath = $@"{_context.Profile.EarcModBackupsDirectory}\{hash}.ffg";
                File.Delete(backupFilePath);
            }
        }

        _context.ChangeTracker.Clear();
    }
}