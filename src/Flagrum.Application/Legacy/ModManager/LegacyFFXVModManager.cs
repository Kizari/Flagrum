using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Archive;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities.ModManager;
using Flagrum.Application.Utilities;
using Microsoft.Extensions.Logging;

namespace Flagrum.Application.Features.ModManager.Services;

public class LegacyFFXVModManager(
    IProfileService profile,
    FlagrumDbContext context,
    ILogger<LegacyFFXVModManager> logger)
    : LegacyModManagerServiceBase(profile, context)
{
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public override async Task BuildAndApplyMod(EarcMod mod, EbonyArchiveManager archiveManager)
    {
        var earcs = new ConcurrentDictionary<string, string>();

        // Backup all files that will be replaced, altered, or removed
        Parallel.ForEach(mod.Earcs.Where(e => e.Type == ModChangeType.Change && e.Files
                .Any(f => f.Type is LegacyModBuildInstruction.RemovePackedFile
                    or LegacyModBuildInstruction.ReplacePackedFile
                    or LegacyModBuildInstruction.AddToPackedTextureArray)),
            earc =>
            {
                var path = $@"{_profile.GameDataDirectory}\{earc.EarcRelativePath}";

                // Skip if 4K pack is missing so Flagrum doesn't crash
                if (!File.Exists(path) && (path.Contains(@"\highimages\") || path.EndsWith("_$h2.earc")))
                {
                    return;
                }

                var archive = archiveManager.Open(path);
                foreach (var replacement in earc.Files.Where(r => r.Type is LegacyModBuildInstruction.RemovePackedFile
                             or LegacyModBuildInstruction.ReplacePackedFile
                             or LegacyModBuildInstruction.AddToPackedTextureArray))
                {
                    var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                    var filePath = $@"{_profile.EarcModBackupsDirectory}\{hash}.ffg";
                    if (!File.Exists(filePath))
                    {
                        var file = archive.Files.FirstOrDefault(f => f.Value.Uri == replacement.Uri)!.Value;
                        var data = archive[replacement.Uri].GetRawData();
                        var fragment = new FmodFragment
                        {
                            OriginalSize = file.Size,
                            ProcessedSize = (uint)data.Length,
                            Flags = file.Flags,
                            Key = file.Key,
                            RelativePath = file.RelativePath,
                            Data = data
                        };

                        fragment.Write(filePath);
                    }
                }
            });

        // Pack all necessary earcs
        Parallel.ForEach(mod.Earcs.Where(e => e.Files.Any()), earc =>
        {
            var path = $@"{_profile.GameDataDirectory}\{earc.EarcRelativePath}";

            EbonyArchive archive;
            if (earc.Type == ModChangeType.Create)
            {
                archive = new EbonyArchive(true);
                archive.SetFlags(earc.Flags);
            }
            else
            {
                // Skip if 4K pack is missing so Flagrum doesn't crash
                if (!File.Exists(path) && (path.Contains(@"\highimages\") || path.EndsWith("_$h2.earc")))
                {
                    return;
                }

                archive = archiveManager.Open(path);
            }

            // Process physical files
            Parallel.Invoke(() =>
                {
                    Parallel.ForEach(
                        earc.Files.Where(f => f.Type is LegacyModBuildInstruction.AddPackedFile
                            or LegacyModBuildInstruction.ReplacePackedFile
                            or LegacyModBuildInstruction.AddToPackedTextureArray),
                        file =>
                        {
                            var hash = Cryptography.HashFileUri64(file.Uri);
                            var cachePath = $@"{_profile.CacheDirectory}\{mod.Id}{file.Id}{hash}.ffg";

                            var fragment = new FmodFragment();
                            fragment.Read(file.ReplacementFilePath.EndsWith(".ffg")
                                ? file.ReplacementFilePath
                                : cachePath);

                            switch (file.Type)
                            {
                                case LegacyModBuildInstruction.AddPackedFile:
                                    archive.AddProcessedFile(file.Uri, fragment.Flags, fragment.Data,
                                        fragment.OriginalSize, fragment.Key, fragment.RelativePath);
                                    break;
                                case LegacyModBuildInstruction.AddToPackedTextureArray:
                                {
                                    var unprocessedData = EbonyArchiveFile.GetUnprocessedData(fragment.Flags,
                                        fragment.OriginalSize, fragment.Key, fragment.Data);
                                    var textureArray = archive[file.Uri].GetReadableData();
                                    var textureArrayBinary = new BlackTexture(LuminousGame.FFXV);
                                    textureArrayBinary.Read(textureArray);
                                    var textureBinary = new BlackTexture(LuminousGame.FFXV);
                                    textureBinary.Read(unprocessedData);
                                    textureArrayBinary.AppendTextureToArray(textureBinary);
                                    var result = textureArrayBinary.Write();
                                    archive.UpdateFile(file.Uri, result);
                                    break;
                                }
                                case LegacyModBuildInstruction.ReplacePackedFile:
                                case LegacyModBuildInstruction.RemovePackedFile:
                                case LegacyModBuildInstruction.AddReference:
                                default:
                                    archive.UpdateFileWithProcessedData(file.Uri, fragment.OriginalSize,
                                        fragment.Data);
                                    break;
                            }
                        });
                },
                () =>
                {
                    foreach (var file in earc.Files)
                    {
                        if (file.Type == LegacyModBuildInstruction.AddReference)
                        {
                            archive.AddFile(file.Uri, file.Flags, null);
                        }
                        else if (file.Type == LegacyModBuildInstruction.RemovePackedFile)
                        {
                            archive.RemoveFile(file.Uri);
                        }
                    }
                });

            // Pack the earc
            var outPath = $@"{_profile.ModStagingDirectory}\{earc.Id}.earc";
            IOHelper.EnsureDirectoriesExistForFilePath(outPath);
            archive.WriteToFile(outPath, LuminousGame.FFXV);
            archive.Dispose();

            earcs.TryAdd(outPath, path);
        });

        // Move built earcs to the correct locations
        foreach (var (stagingPath, destinationPath) in earcs)
        {
            IOHelper.EnsureDirectoriesExistForFilePath(destinationPath);

            await logger.ExecuteWithRetryAsync("Move staged earc to final position (BuildAndApplyMod)", 10,
                TimeSpan.FromMilliseconds(200), () =>
                {
                    // This is an attempt to fix an inconsistent System.UnauthorizedAccessException
                    // See https://github.com/Kizari/Flagrum/issues/60
                    IOHelper.SetFileAttributesNormal(stagingPath);
                    IOHelper.SetFileAttributesNormal(destinationPath);

                    File.Move(stagingPath, destinationPath, true);
                });
        }

        // Backup and apply loose files
        Parallel.ForEach(mod.LooseFiles, file =>
        {
            if (file.Type == ModChangeType.Change)
            {
                // Backup the original file
                var fileName = file.RelativePath.ToBase64();
                var backupPath = $@"{_profile.EarcModBackupsDirectory}\{fileName}";
                if (!File.Exists(backupPath))
                {
                    var originalFile = $@"{_profile.GameDataDirectory}\{file.RelativePath}";
                    if (File.Exists(originalFile))
                    {
                        File.Copy(originalFile, backupPath);
                    }
                }
            }

            var destination = $@"{_profile.GameDataDirectory}\{file.RelativePath}";
            File.Copy(file.FilePath, destination, true);
        });
    }

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