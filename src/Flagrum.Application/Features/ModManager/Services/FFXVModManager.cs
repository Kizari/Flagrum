using System;
using System.Diagnostics.CodeAnalysis;
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
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;

namespace Flagrum.Application.Features.ModManager.Services;

public class FFXVModManager(
    IProfileService profile,
    IFileIndex fileIndex,
    AssetConverter assetConverter,
    IServiceProvider provider)
    : ModManagerServiceBase(profile, fileIndex, assetConverter, provider)
{
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    protected override Task ApplyMod(IFlagrumProject mod, EbonyArchiveManager archiveManager)
    {
        // Load up the patch archives
        var patchIndex1ArchivePath = Path.Combine(_profile.PatchDirectory, "patch1_initial", "patchindex.earc");
        using var patchIndex1Archive = File.Exists(patchIndex1ArchivePath)
            ? new EbonyArchive(patchIndex1ArchivePath)
            : new EbonyArchive(true);

        var patchIndexArchivePath = Path.Combine(_profile.PatchDirectory, "patch1", "patchindex.earc");
        using var patchIndexArchive = File.Exists(patchIndexArchivePath)
            ? new EbonyArchive(patchIndexArchivePath)
            : new EbonyArchive(true);

        // Process each archive in the mod
        Parallel.Invoke(() =>
        {
            // Apply instructions for packed files
            Parallel.ForEach(mod.Archives.Where(a => a.Instructions.Count != 0), earc =>
            {
                // Determine path based on whether new archive or existing
                string path;
                var isInitial = false;

                if (earc.Type == ModChangeType.Change)
                {
                    var comparePath = earc.RelativePath.Replace('\\', '/').ToLower();
                    isInitial = CommonAllDependencies.Instance.Contains(comparePath);

                    path = Path.Combine(_profile.PatchDirectory, isInitial ? "patch1_initial" : "patch1",
                        earc.RelativePath);
                }
                else
                {
                    path = Path.Combine(_profile.GameDataDirectory, earc.RelativePath);
                }

                // Create or open the archive
                var doesExist = File.Exists(path);
                using var archive = doesExist ? archiveManager.Open(path) : new EbonyArchive(true);

                if (!doesExist)
                {
                    // Not actually sure if HasLooseData is required here, could test this if we care
                    archive.SetFlags(earc.Flags | EbonyArchiveFlags.HasLooseData | EbonyArchiveFlags.FlagrumModArchive);
                }

                // Run build process for each file in the archive
                Parallel.ForEach(earc.Instructions, file => file.Apply(mod, archive, earc));

                // Pack the archive if something was added to it
                if (!archive.Files.IsEmpty)
                {
                    IOHelper.EnsureDirectoriesExistForFilePath(path);
                    archive.WriteToFile(path, LuminousGame.FFXV);

                    // Add a reference to the patch archive
                    if (earc.Type == ModChangeType.Change)
                    {
                        if (isInitial)
                        {
                            var referenceUri = "data://patch/patch1_initial/" +
                                               earc.RelativePath.Replace('\\', '/').Replace(".earc", ".ebex@");
                            if (!patchIndex1Archive.HasFile(referenceUri))
                            {
                                patchIndex1Archive.AddFile(referenceUri,
                                    EbonyArchiveFileFlags.Autoload | EbonyArchiveFileFlags.Reference,
                                    []);
                            }
                        }
                        else
                        {
                            var referenceUri = "data://patch/patch1/" +
                                               earc.RelativePath.Replace('\\', '/').Replace(".earc", ".ebex@");
                            if (!patchIndexArchive.HasFile(referenceUri))
                            {
                                patchIndexArchive.AddFile(referenceUri,
                                    EbonyArchiveFileFlags.Autoload | EbonyArchiveFileFlags.Reference,
                                    Array.Empty<byte>());
                            }
                        }
                    }
                }
            });
        }, () =>
        {
            // TODO: Loose files ideally need to be not mixed in with the main files
            // Apply non-packed instructions
            Parallel.ForEach(mod.Instructions.Cast<GlobalBuildInstruction>(), i => i.Apply());
        });

        // Save the patch archive
        if (!patchIndexArchive.Files.IsEmpty)
        {
            IOHelper.EnsureDirectoriesExistForFilePath(patchIndexArchivePath);
            patchIndexArchive.WriteToFile(patchIndexArchivePath, LuminousGame.FFXV);
        }

        if (!patchIndex1Archive.Files.IsEmpty)
        {
            IOHelper.EnsureDirectoriesExistForFilePath(patchIndex1ArchivePath);
            patchIndex1Archive.WriteToFile(patchIndex1ArchivePath, LuminousGame.FFXV);
        }

        return Task.CompletedTask;
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    protected override Task RevertMod(IFlagrumProject mod)
    {
        // Load up the patch archives
        var patchInitialIndexArchivePath = Path.Combine(_profile.PatchDirectory, "patch1_initial", "patchindex.earc");
        var patchIndexArchivePath = Path.Combine(_profile.PatchDirectory, "patch1", "patchindex.earc");
        using var patchIndexArchive =
            File.Exists(patchIndexArchivePath) ? new EbonyArchive(patchIndexArchivePath) : null;
        using var patchInitialIndexArchive = File.Exists(patchInitialIndexArchivePath)
            ? new EbonyArchive(patchInitialIndexArchivePath)
            : null;

        Parallel.Invoke(() =>
        {
            // Revert packed instructions
            Parallel.ForEach(mod.Archives, earc =>
            {
                // Simply delete the archive and move on if it's a non-patch archive
                if (earc.Type == ModChangeType.Create)
                {
                    File.Delete(Path.Combine(_profile.GameDataDirectory, earc.RelativePath));
                    return;
                }

                var comparePath = earc.RelativePath.Replace('\\', '/').ToLower();
                var isInitial = CommonAllDependencies.Instance.Contains(comparePath);
                var patchFolder = isInitial ? "patch1_initial" : "patch1";
                var path = Path.Combine(_profile.PatchDirectory, patchFolder, earc.RelativePath);
                var patchArchive = isInitial ? patchInitialIndexArchive : patchIndexArchive;

                // Run revert process for each file in the archive
                using var archive = File.Exists(path) ? new EbonyArchive(path) : null;
                foreach (var file in earc.Instructions)
                {
                    file.Revert(archive, earc);
                }

                if (archive?.Files.Any() == true)
                {
                    // Other mods must still be using this archive, write it back to disk
                    archive.WriteToSource(LuminousGame.FFXV);
                }
                else
                {
                    // No mods applied to this archive anymore, delete it
                    archive?.Dispose();
                    IOHelper.DeleteFileIfExists(path);

                    // Remove the reference to this archive from the patch index
                    var referenceUri = $"data://patch/{patchFolder}/" +
                                       earc.RelativePath.Replace('\\', '/').Replace(".earc", ".ebex@");
                    if (patchArchive!.HasFile(referenceUri))
                    {
                        patchArchive.RemoveFile(referenceUri);
                    }
                }
            });
        }, () =>
        {
            // Revert non-packed instructions
            // TODO: Loose files ideally need to be not mixed in with the main files
            foreach (var instruction in mod.Instructions.Cast<GlobalBuildInstruction>())
            {
                instruction.Revert();
            }
        });

        IOHelper.DeleteEmptySubdirectoriesRecursively(_profile.PatchDirectory);
        var patchInitialHasFiles = patchInitialIndexArchive?.Files.Any() == true;
        var patchHasFiles = patchIndexArchive?.Files.Any() == true;

        if (patchHasFiles)
        {
            patchIndexArchive.WriteToSource(LuminousGame.FFXV);
        }

        if (patchInitialHasFiles)
        {
            patchInitialIndexArchive.WriteToSource(LuminousGame.FFXV);
        }

        patchIndexArchive?.Dispose();
        patchInitialIndexArchive?.Dispose();

        if (!patchHasFiles)
        {
            IOHelper.DeleteFileIfExists(patchIndexArchivePath);
        }

        if (!patchInitialHasFiles)
        {
            IOHelper.DeleteFileIfExists(patchInitialIndexArchivePath);
        }

        // Delete the patch folder if no more mods are applied
        if (!patchHasFiles && !patchInitialHasFiles && Directory.Exists(_profile.PatchDirectory))
        {
            Directory.Delete(Path.Combine(_profile.GameDataDirectory, "patch"), true);
        }

        return Task.CompletedTask;
    }

    public override Guid[] Reset()
    {
        var enabledMods = Projects.Values
            .Where(p => ModsState.GetActive(p.Identifier))
            .Select(p => p.Identifier)
            .ToArray();

        // Delete all Flagrum-added loose files
        // TODO: Loose files ideally need to not be mixed in with main files
        var looseFiles = Projects.Values
            .SelectMany(p => p.Instructions.OfType<AddLooseFileBuildInstruction>()
                .Select(i => Path.Combine(_profile.GameDataDirectory, i.RelativePath)));
        foreach (var file in looseFiles)
        {
            IOHelper.DeleteFileIfExists(file);
        }

        // Restore backups of any replaced loose files
        var looseFileReplacements = Projects.Values
            .SelectMany(p => p.Instructions.OfType<ReplaceLooseFileBuildInstruction>());
        foreach (var instruction in looseFileReplacements)
        {
            instruction.Revert();
        }

        // Delete the patch directory containing all archive overrides
        var patchDirectory = Path.Combine(_profile.GameDataDirectory, "patch");
        if (Directory.Exists(patchDirectory))
        {
            Directory.Delete(patchDirectory, true);
        }

        // Delete all Flagrum-created non-patch archives
        var newArchives = Projects.Values
            .SelectMany(p => p.Archives.Where(a => a.Type == ModChangeType.Create))
            .Select(a => Path.Combine(_profile.GameDataDirectory, a.RelativePath));
        foreach (var file in newArchives)
        {
            IOHelper.DeleteFileIfExists(file);
        }

        // Mark all mods as disabled
        ModsState.SetAllInactive();

        return enabledMods;
    }
}