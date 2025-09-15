using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Generators;
using Flagrum.Application.Features.AssetExplorer.Indexing;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Features.ModManager.Project;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities;
using Flagrum.Application.Services;
using Flagrum.Application.Utilities;
using Flagrum.ApplicationHost;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Migrations;

[SteppedDataMigration(2)]
public partial class FileIndexMigration(
    FlagrumDbContext context,
    IModBuildInstructionFactory factory,
    LegacyModManagerServiceBase legacyModManager,
    ModManagerServiceBase modManager,
    IProfileService profile,
    IFileIndex fileIndex)
{
    [MigrationStep(0, "d2e4e56c-6e5b-4b57-9d33-11ccb8d3878e", MigrationScope.Profile)]
    private async Task MigrateFileIndex()
    {
        SplashViewModel.Instance.SetLoadingText("Migrating file index");

        // Load the node tree from the DB
        var nodeTree = context.AssetExplorerNodes
            .AsNoTracking()
            .ToList()
            .ToDictionary(n => n.Id, n => n);

        // Create the parent/child relationships
        foreach (var (_, node) in nodeTree)
        {
            if (node.ParentId > 0)
            {
                node.ParentNode = nodeTree[node.ParentId.Value];
                node.ParentNode.ChildNodes.Add(node);
            }
        }

        // Sort the children first by directory status, then by name
        foreach (var (_, node) in nodeTree)
        {
            if (node.ChildNodes?.Any() == true)
            {
                ((List<AssetExplorerNode>)node.ChildNodes).Sort((first, second) =>
                {
                    var typeDifference = first.ChildNodes.Any().CompareTo(second.ChildNodes.Any()) * -1;
                    return typeDifference == 0 ? string.CompareOrdinal(first.Name, second.Name) : typeDifference;
                });
            }
        }

        var rootNode = nodeTree
            .First(n => n.Value.ParentId == null).Value;

        var root = new FileIndexNode {Name = rootNode.Name};
        ProcessNodesRecursive(rootNode, root);
        fileIndex.RootNode = root;
        ((FileIndex)fileIndex).Archives = [];
        ((FileIndex)fileIndex).Files = [];

        var archiveLocations = context.ArchiveLocations
            .Select(a => new
            {
                a.Path,
                AssetUris = a.AssetUris.Select(u => u.Uri)
            })
            .ToList();

        foreach (var archiveLocation in archiveLocations)
        {
            var archive = new FileIndexArchive
            {
                RelativePath = archiveLocation.Path.Replace('\\', '/'),
                Files = new List<FileIndexFile>()
            };

            foreach (var assetUri in archiveLocation.AssetUris)
            {
                var file = new FileIndexFile
                {
                    Uri = assetUri,
                    Archive = archive
                };

                archive.Files.Add(file);
                ((FileIndex)fileIndex).AddFile(new AssetId(assetUri), file);
            }

            ((FileIndex)fileIndex).Archives.Add(Cryptography.Hash64(archive.RelativePath), archive);
        }

        ApplicationHost.SplashViewModel.Instance.SetLoadingText("Cleaning up old data");

        fileIndex.Save(profile.FileIndexPath);
        context.SetString(StateKey.CurrentAssetNode, null);
        await context.AssetExplorerNodes.ExecuteDeleteAsync();
        await context.AssetUris.ExecuteDeleteAsync();
        await context.ArchiveLocations.ExecuteDeleteAsync();

        // Clear the old state node as it uses IDs instead of URIs and won't work
        context.SetString(StateKey.CurrentAssetNode, null);
    }

    [MigrationStep(1, "6e57fe99-40e1-4e47-aabe-59ffcb21fafb", MigrationScope.Profile)]
    private async Task MigrateProjects()
    {
        ApplicationHost.SplashViewModel.Instance.SetLoadingText("Migrating mod projects");

        var guids = new List<string>();
        var modsToEnable = new List<Guid>();

        foreach (var mod in context.EarcMods
                     .Include(earcMod => earcMod.Earcs).ThenInclude(earcModEarc => earcModEarc.Files)
                     .Include(earcMod => earcMod.LooseFiles)
                     .ToList())
        {
            // Move any project files in other Flagrum-related directories into this project's directory
            foreach (var file in mod.Earcs.SelectMany(e => e.Files.Where(f => f.ReplacementFilePath != null)))
            {
                var filePath = file.ReplacementFilePath;
                if (filePath.StartsWith(profile.ModFilesDirectory) && !filePath.StartsWith($@"{profile.ModFilesDirectory}\{mod.Id}\"))
                {
                    var oldFolder = filePath.Replace(profile.ModFilesDirectory + '\\', "")
                        .Split('\\')[0];
                    var newPath = filePath.Replace($@"{profile.ModFilesDirectory}\{oldFolder}\",
                        $@"{profile.ModFilesDirectory}\{mod.Id}\");
                    IOHelper.EnsureDirectoriesExistForFilePath(newPath);
                    File.Move(filePath, newPath, true);
                    file.ReplacementFilePath = newPath;
                }
            }

            foreach (var file in mod.LooseFiles)
            {
                var filePath = file.FilePath;
                if (filePath.StartsWith(profile.ModFilesDirectory) && !filePath.StartsWith($@"{profile.ModFilesDirectory}\{mod.Id}\"))
                {
                    var oldFolder = filePath.Replace(profile.ModFilesDirectory + '\\', "")
                        .Split('\\')[0];
                    var newPath = filePath.Replace($@"{profile.ModFilesDirectory}\{oldFolder}\",
                        $@"{profile.ModFilesDirectory}\{mod.Id}\");
                    IOHelper.EnsureDirectoriesExistForFilePath(newPath);
                    File.Move(filePath, newPath, true);
                    file.FilePath = newPath;
                }
            }
            
            // Get guid for the mod
            var guid = mod.Identifier ?? Guid.NewGuid();
            guids.Add(guid.ToString());

            // If the mod is currently enabled, it needs to be disabled if it is for FFXV
            if (profile.Current.Type == LuminousGame.FFXV && mod.IsActive)
            {
                modsToEnable.Add(guid);
                legacyModManager.DisableMod(mod.Id);
            }

            // Rename mod directory to match the guid
            var originalDirectory = Path.Combine(profile.ModFilesDirectory, mod.Id.ToString());
            var newDirectory = Path.Combine(profile.ModFilesDirectory, guid.ToString());

            if (Directory.Exists(originalDirectory))
            {
                Directory.Move(originalDirectory, newDirectory);
            }
            else
            {
                Directory.CreateDirectory(newDirectory);
            }

            // Copy thumbnail to the mod directory and images directory
            var thumbnailPath = Path.Combine(profile.EarcModThumbnailDirectory, $"{mod.Id}.png");
            if (!File.Exists(thumbnailPath))
            {
                thumbnailPath = Path.Combine(profile.ModThumbnailWebDirectory, $"{mod.Id}.png");
            }

            if (File.Exists(thumbnailPath))
            {
                File.Copy(thumbnailPath, Path.Combine(profile.ImagesDirectory, $"{guid}.jpg"), true);
                File.Move(thumbnailPath, Path.Combine(profile.ModFilesDirectory, guid.ToString(), "thumbnail.jpg"),
                    true);
            }

            // Generate fproj file and put in the directory
            var project = new FlagrumProject
            {
                Identifier = guid,
                Flags = mod.Flags,
                Name = mod.Name,
                Author = mod.Author,
                Description = mod.Description,
                Readme = mod.Readme,
                Archives = mod.Earcs.Select(e => new FlagrumProjectArchive
                {
                    Type = e.Type,
                    RelativePath = e.EarcRelativePath.Replace('\\', '/'),
                    Flags = e.Flags,
                    Instructions = e.Files.Select(f =>
                    {
                        PackedBuildInstruction instruction = f.Type switch
                        {
                            LegacyModBuildInstruction.AddReference => factory.Create<AddReferenceBuildInstruction>(),
                            LegacyModBuildInstruction.AddPackedFile => factory.Create<AddPackedFileBuildInstruction>(),
                            LegacyModBuildInstruction.RemovePackedFile => factory
                                .Create<RemovePackedFileBuildInstruction>(),
                            LegacyModBuildInstruction.ReplacePackedFile => factory
                                .Create<ReplacePackedFileBuildInstruction>(),
                            LegacyModBuildInstruction.AddToPackedTextureArray => factory
                                .Create<AddToPackedTextureArrayBuildInstruction>(),
                            _ => throw new Exception($"Can't migrate build instruction {f.Type}")
                        };

                        instruction.Flags = f.Flags;
                        instruction.Uri = f.Uri;

                        if (instruction is PackedAssetBuildInstruction asset)
                        {
                            var comparison = $@"{profile.Current.Id}\{mod.Id}\";
                            var replacement = $@"{profile.Current.Id}\{guid}\";
                            asset.FilePath = f.ReplacementFilePath.Replace(comparison, replacement);
                            asset.FileLastModified = f.FileLastModified;
                        }

                        return instruction;
                    }).Cast<IPackedBuildInstruction>().ToList()
                }).Cast<IFlagrumProjectArchive>().ToList(),
                Instructions = mod.LooseFiles.Select(f =>
                {
                    LooseAssetBuildInstruction instruction = f.Type switch
                    {
                        ModChangeType.Change => factory.Create<ReplaceLooseFileBuildInstruction>(),
                        ModChangeType.Create => factory.Create<AddLooseFileBuildInstruction>(),
                        _ => throw new Exception($"Can't migrate loose build instruction {f.Type}")
                    };

                    var comparison = $@"{profile.Current.Id}\{mod.Id}\";
                    var replacement = $@"{profile.Current.Id}\{guid}\";
                    instruction.FilePath = f.FilePath.Replace(comparison, replacement);
                    instruction.RelativePath = f.RelativePath.Replace('\\', '/');
                    return (ModBuildInstruction)instruction;
                }).Cast<IModBuildInstruction>().ToList()
            };

            await project.Save(Path.Combine(profile.ModFilesDirectory, guid.ToString(), "project.fproj"));
            modManager.Projects.Add(guid, project);

            // Set the mod state accordingly (IsActive to false since all mods were disabled)
            modManager.ModsState.Add(guid, new ModState {IsPinned = mod.IsFavourite});

            // Update active Forspoken mods
            if (mod.IsActive && profile.Current.Type == LuminousGame.Forspoken)
            {
                using var archive = new EbonyArchive($@"{profile.GameDataDirectory}\c000.earc");

                // Change the reference path to use the new guid
                archive.RemoveFile($"data://mods/{mod.Id}.ebex@");
                archive.AddFile($"data://mods/{guid}.ebex@",
                    EbonyArchiveFileFlags.Autoload | EbonyArchiveFileFlags.Reference, Array.Empty<byte>());
                archive.WriteToSource(LuminousGame.Forspoken);

                // Rename the earc to match the new guid
                var modPath = Path.Combine(profile.GameDataDirectory, "mods", $"{mod.Id}.earc");
                var newModPath = Path.Combine(profile.GameDataDirectory, "mods", $"{guid}.earc");
                if (File.Exists(modPath))
                {
                    File.Move(modPath, newModPath);
                }
            }
        }

        ApplicationHost.SplashViewModel.Instance.SetLoadingText("Cleaning up old data");

        // Delete the old thumbnail directories
        Directory.Delete(profile.ModThumbnailWebDirectory, true);
        Directory.Delete(profile.EarcModThumbnailDirectory, true);

        // Delete any unused mod folders
        foreach (var directory in Directory.EnumerateDirectories(profile.ModFilesDirectory))
        {
            var name = directory.Split('\\', '/').Last();
            if (name != "backup" && !guids.Contains(name))
            {
                Directory.Delete(directory, true);
            }
        }

        // Clear cache as none of the file names will be valid anymore
        foreach (var file in Directory.EnumerateFiles(profile.CacheDirectory))
        {
            File.Delete(file);
        }

        // Clear mod tables in DB
        await context.EarcModReplacements.ExecuteDeleteAsync();
        await context.EarcModEarcs.ExecuteDeleteAsync();
        await context.EarcModLooseFile.ExecuteDeleteAsync();
        await context.EarcMods.ExecuteDeleteAsync();

        // Delete all backup files as the new system doesn't need to backup files due to using patch archives
        Directory.Delete(profile.EarcModBackupsDirectory, true);

        ApplicationHost.SplashViewModel.Instance.SetLoadingText("Reenabling mods");

        // Now that everything is migrated, we need to enable any mods that were disabled for the migration
        foreach (var project in modsToEnable.Select(m => modManager.Projects[m]))
        {
            if (project.AreReferencesValid())
            {
                await modManager.EnableMod(project);
            }
        }
    }

    [MigrationStep(2, "d11cb3e6-3679-4383-b345-3f308d3586fd", MigrationScope.Profile, MigrationStepMode.Retry)]
    private void Cleanup()
    {
        // Delete the mod staging directory and all its contents as it isn't used in 1.5+
        if (Directory.Exists(profile.ModStagingDirectory))
        {
            Directory.Delete(profile.ModStagingDirectory, true);
        }
    }

    private void ProcessNodesRecursive(AssetExplorerNode original, FileIndexNode copy)
    {
        copy.ChildNodes = new List<FileIndexNode>();

        foreach (var node in original.ChildNodes)
        {
            var newNode = new FileIndexNode
            {
                Name = node.Name,
                ParentNode = copy
            };

            copy.ChildNodes.Add(newNode);

            ProcessNodesRecursive(node, newNode);
        }
    }
}