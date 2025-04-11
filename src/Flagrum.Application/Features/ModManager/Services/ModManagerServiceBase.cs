using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Archive;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.AssetExplorer.Data;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Utilities;

namespace Flagrum.Application.Features.ModManager.Services;

public abstract class ModManagerServiceBase
{
    protected readonly AssetConverter _assetConverter;
    protected readonly IFileIndex _fileIndex;
    protected readonly IProfileService _profile;

    protected ModManagerServiceBase(
        IProfileService profile,
        IFileIndex fileIndex,
        AssetConverter assetConverter,
        IServiceProvider provider)
    {
        _profile = profile;
        _fileIndex = fileIndex;
        _assetConverter = assetConverter;

        ModsState = Repository.Load<ModsState>(_profile.ModStatePath) ?? new ModsState();
        ModsState.FilePath = _profile.ModStatePath;

        Projects = new Dictionary<Guid, IFlagrumProject>();

        // Load all mod projects into memory
        foreach (var directory in Directory.EnumerateDirectories(profile.ModFilesDirectory))
        {
            var projectPath = Path.Combine(directory, "project.fproj");
            if (File.Exists(projectPath))
            {
                var project = provider.LoadFlagrumProject(projectPath);
                Projects[project.Identifier] = project;

                if (!ModsState.Contains(project.Identifier))
                {
                    ModsState.Add(project.Identifier, new ModState());
                }
            }
        }
    }

    public Dictionary<Guid, IFlagrumProject> Projects { get; set; }
    public ModsState ModsState { get; set; }

    /// <summary>
    /// Reverts all applied mods and cleans up after the mod manager
    /// </summary>
    /// <returns>A list of mod IDs that were active before the reset</returns>
    public abstract Guid[] Reset();

    protected abstract Task ApplyMod(IFlagrumProject mod, EbonyArchiveManager archiveManager);
    protected abstract Task RevertMod(IFlagrumProject mod);

    public async Task SaveModCard(IFlagrumProject mod)
    {
        if (mod.Identifier == Guid.Empty)
        {
            mod.Identifier = Guid.NewGuid();
            Projects[mod.Identifier] = mod;
            ModsState.Add(mod.Identifier, new ModState());
        }

        var path = Path.Combine(_profile.ModFilesDirectory, mod.Identifier.ToString(), "project.fproj");
        IOHelper.EnsureDirectoriesExistForFilePath(path);
        await mod.Save(path);
        UpdateThumbnail(mod.Identifier);
    }

    public Task SaveBuildList(IFlagrumProject mod)
    {
        // Replace the mod with the updated clone
        Projects[mod.Identifier] = mod;

        // Save the clone over the previous version
        return mod.Save(Path.Combine(_profile.ModFilesDirectory, mod.Identifier.ToString(), "project.fproj"));
    }

    public async Task DeleteMod(IFlagrumProject mod)
    {
        if (ModsState.GetActive(mod.Identifier))
        {
            await RevertMod(mod);
        }

        ClearCachedFilesForMod(mod.Identifier);

        // Remove thumbnail for this mod
        try
        {
            File.Delete(Path.Combine(_profile.ImagesDirectory, mod.Identifier + ".jpg"));
        }
        catch
        {
            // Nothing to be done if Windows is trolling
            // Trying to delete again on restart was causing too many issues when multiple versions
            // of Flagrum were involved, so that method has been removed
            // Perhaps an alternative is needed? For example, storing a list
            // in the DB and only removing the entries once they're deleted successfully?
        }

        // Clean up the project files
        var projectDirectory = Path.Combine(_profile.ModFilesDirectory, mod.Identifier.ToString());
        if (Directory.Exists(projectDirectory))
        {
            var otherMods = Projects.Values
                .Where(p => p.Identifier != mod.Identifier)
                .ToList();

            var allModFiles = otherMods
                .SelectMany(p => p.Archives
                    .SelectMany(a => a.Instructions.OfType<PackedAssetBuildInstruction>()
                        .Select(i => i.FilePath)))
                .Union(otherMods
                    .SelectMany(p => p.Instructions.OfType<LooseAssetBuildInstruction>()
                        .Select(i => i.FilePath)))
                .ToList();

            foreach (var file in Directory.GetFiles(projectDirectory, "*.*", SearchOption.AllDirectories))
            {
                if (!allModFiles.Any(f => IOHelper.ComparePaths(file, f)))
                {
                    File.Delete(file);
                }
            }

            if (!Directory.GetFiles(projectDirectory, "*.*", SearchOption.AllDirectories).Any())
            {
                Directory.Delete(projectDirectory, true);
            }
        }

        ModsState.Remove(mod.Identifier);
        Projects.Remove(mod.Identifier);
    }

    public async Task EnableMod(IFlagrumProject mod)
    {
        using (var archiveManager = new EbonyArchiveManager())
        {
            if (mod.HaveFilesChanged || !CheckModIsCached(mod))
            {
                // Cache is invalid, rebuild it
                BuildAssetCache(mod, archiveManager);
            }

            await ApplyMod(mod, archiveManager);
        }

        ModsState.SetActive(mod.Identifier, true);
        mod.UpdateLastModifiedTimestamps();
        await mod.Save(Path.Combine(_profile.ModFilesDirectory, mod.Identifier.ToString(), "project.fproj"));
    }

    public async Task DisableMod(IFlagrumProject mod)
    {
        await RevertMod(mod);

        // Now that the mod has been reverted, it can be marked as disabled
        ModsState.SetActive(mod.Identifier, false);
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public void BuildAssetCache(IFlagrumProject mod, EbonyArchiveManager archiveManager)
    {
        var imageMap = new ConcurrentDictionary<string, byte[]>();

        // Get metadata for replacement textures
        var textureMetadata = new ConcurrentDictionary<string, BlackTexture>();
        var replacementTextures = mod.Archives
            .SelectMany(e => e.Instructions
                .Where(i => i is ReplacePackedFileBuildInstruction replace
                            && AssetExplorerItem.GetType(i.Uri) == ExplorerItemType.Texture
                            && !replace.FilePath.EndsWith(".btex")
                            && !replace.FilePath.EndsWith(".ffg")
                            && (replace.FileLastModified != File.GetLastWriteTime(replace.FilePath).Ticks
                                || !File.Exists(
                                    $@"{_profile.CacheDirectory}\{mod.Identifier}{Cryptography.HashFileUri64(i.Uri)}.ffg"))));

        Parallel.ForEach(replacementTextures, file =>
        {
            var relativePath = _fileIndex.GetArchiveRelativePathByUri(file.Uri);
            if (relativePath == null)
            {
                // Skip if 4K pack is missing so Flagrum doesn't crash
                if (file.Uri.Contains("/highimages/") || file.Uri.EndsWith("_$h2.autoext"))
                {
                    return;
                }

                throw new Exception($"Could not determine earc path for file {file.Uri}");
            }

            var path = $@"{_profile.GameDataDirectory}\{relativePath}";

            if (!File.Exists(path) && (path!.Contains(@"\highimages\") || path.EndsWith("_$h2.earc")))
            {
                return;
            }

            var archive = archiveManager.Open(path);
            var data = archive[file.Uri].GetReadableData();
            var binary = new BlackTexture(_profile.Current.Type);
            binary.Read(data);
            textureMetadata[file.Uri] = binary;
        });

        _assetConverter.SetTextureMetadata(textureMetadata);

        // Process image files
        foreach (var earc in mod.Archives.Where(e => e.Instructions
                     .Any(f => f is PackedAssetBuildInstruction asset
                               && AssetExplorerItem.GetType(f.Uri) == ExplorerItemType.Texture
                               && !asset.FilePath.EndsWith(".btex"))))
        {
            foreach (var file in earc.Instructions
                         .Where(f => f is PackedAssetBuildInstruction asset
                                     && AssetExplorerItem.GetType(f.Uri) == ExplorerItemType.Texture
                                     && !asset.FilePath.EndsWith(".btex"))
                         .Cast<PackedAssetBuildInstruction>())
            {
                var hash = Cryptography.HashFileUri64(file.Uri);
                var cachePath = $@"{_profile.CacheDirectory}\{mod.Identifier}{hash}.ffg";
                var needsRebuild = !file.FilePath.EndsWith(".ffg")
                                   && (file.FileLastModified !=
                                       File.GetLastWriteTime(file.FilePath).Ticks
                                       || !File.Exists(cachePath));

                if (needsRebuild)
                {
                    imageMap.TryAdd(file.Uri, _assetConverter.Convert(file));
                }
            }
        }

        // Process other assets
        Parallel.ForEach(mod.Archives.Where(e => e.Instructions.Any()), earc =>
        {
            var path = Path.Combine(_profile.GameDataDirectory, earc.RelativePath);

            EbonyArchive sourceArchive = null;
            if (File.Exists(path))
            {
                sourceArchive = archiveManager.Open(path);
            }

            Parallel.ForEach(earc.Instructions.OfType<PackedAssetBuildInstruction>(),
                file =>
                {
                    var hash = Cryptography.HashFileUri64(file.Uri);
                    var cachePath = $@"{_profile.CacheDirectory}\{mod.Identifier}{hash}.ffg";

                    // Only build files that are not already processed
                    if (file.Uri.EndsWith(".win32.bins") || // Always rebuild bins in case they need merging
                        (!file.FilePath.EndsWith(".ffg")
                         && (file.FileLastModified != File.GetLastWriteTime(file.FilePath).Ticks
                             || !File.Exists(cachePath))))
                    {
                        var fragment = file.Build(sourceArchive, imageMap).AwaitSynchronous();
                        fragment?.Write(cachePath);
                    }
                });
        });
    }

    public void ClearCachedFilesForMod(Guid modId)
    {
        if (Directory.Exists(_profile.CacheDirectory))
        {
            foreach (var path in Directory.EnumerateFiles(_profile.CacheDirectory)
                         .Where(f => f.Split('\\').Last().StartsWith(modId.ToString())))
            {
                File.Delete(path);
            }
        }
    }

    public bool CheckModIsCached(IFlagrumProject mod)
    {
        var isCached = true;
        foreach (var earc in mod.Archives)
        {
            foreach (var file in earc.Instructions.OfType<PackedAssetBuildInstruction>())
            {
                if (!file.FilePath.EndsWith(".ffg") &&
                    !File.Exists(
                        $@"{_profile.CacheDirectory}\{mod.Identifier}{Cryptography.HashFileUri64(file.Uri)}.ffg"))
                {
                    isCached = false;
                }
            }
        }

        return isCached;
    }

    public bool HasAnyCachedFiles(IFlagrumProject mod)
    {
        return mod.Archives.Any(e => e.Instructions
            .Any(f => f is PackedAssetBuildInstruction
                      && File.Exists(
                          $@"{_profile.CacheDirectory}\{mod.Identifier}{Cryptography.HashFileUri64(f.Uri)}.ffg")));
    }

    private void UpdateThumbnail(Guid modId)
    {
        var previewPath = Path.Combine(_profile.ImagesDirectory, "current_earc_preview.png");
        var webPath = Path.Combine(_profile.ImagesDirectory, $"{modId}.jpg");
        var projectPath = Path.Combine(_profile.ModFilesDirectory, modId.ToString(), "thumbnail.jpg");
        File.Copy(previewPath, webPath, true);
        File.Copy(previewPath, projectPath, true);
    }

    public void DeleteFlagrumCreatedEarcsFromDataDirectoryForMod(Guid modId)
    {
        var earcsToDelete = Projects.Values
            .Where(m => m.Identifier == modId)
            .SelectMany(m => m.Archives
                .Where(e => e.Type == ModChangeType.Create)
                .Select(e => Path.Combine(_profile.GameDataDirectory, e.RelativePath)))
            .ToList();

        foreach (var earc in earcsToDelete.Where(File.Exists))
        {
            IOHelper.SetFileAttributesNormal(earc); // Ensures the delete isn't stopped by readonly flags
            File.Delete(earc);
        }
    }

    public void DeleteAllFlagrumCreatedEarcsFromDataDirectory()
    {
        var earcsToDelete = Projects.Values
            .SelectMany(m => m.Archives
                .Where(e => e.Type == ModChangeType.Create)
                .Select(e => Path.Combine(_profile.GameDataDirectory, e.RelativePath)))
            .ToList();

        foreach (var earc in earcsToDelete.Where(File.Exists))
        {
            IOHelper.SetFileAttributesNormal(earc); // Ensures the delete isn't stopped by readonly flags
            File.Delete(earc);
        }
    }
}