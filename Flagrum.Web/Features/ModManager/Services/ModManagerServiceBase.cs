using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Features.ModManager.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Features.ModManager.Services;

public abstract class ModManagerServiceBase
{
    protected readonly AppStateService _appState;
    protected readonly AssetConverter _assetConverter;
    protected readonly FlagrumDbContext _context;
    protected readonly ProfileService _profile;

    protected ModManagerServiceBase(
        ProfileService profile,
        FlagrumDbContext context,
        AppStateService appState,
        AssetConverter assetConverter)
    {
        _profile = profile;
        _context = context;
        _appState = appState;
        _assetConverter = assetConverter;
    }

    public abstract void BuildAndApplyMod(EarcMod mod, EbonyArchiveManager archiveManager);
    public abstract void RevertMod(EarcMod mod);

    public async Task SaveModCard(EarcMod mod)
    {
        if (mod.Id == 0)
        {
            await _context.AddAsync(mod);
        }
        else
        {
            _context.Update(mod);
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        UpdateThumbnail(mod.Id);
    }

    public async Task SaveBuildList(EarcMod mod)
    {
        // Delete any records that were removed from the build list
        var existingEarcs = _context.EarcModEarcs
            .Where(e => e.EarcModId == mod.Id)
            .Select(e => e.Id)
            .ToList();

        var existingFiles = _context.EarcModReplacements
            .Where(e => existingEarcs.Contains(e.EarcModEarcId))
            .Select(e => e.Id)
            .ToList();

        var existingLooseFiles = _context.EarcModLooseFile
            .Where(e => e.EarcModId == mod.Id)
            .Select(e => e.Id)
            .ToList();

        var newEarcs = mod.Earcs.Select(e => e.Id).ToList();
        var newFiles = mod.Earcs.SelectMany(e => e.Files.Select(f => f.Id)).ToList();
        var newLooseFiles = mod.LooseFiles.Select(e => e.Id).ToList();

        var filesToDelete = existingFiles.Except(newFiles);
        var earcsToDelete = existingEarcs.Except(newEarcs);
        var looseFilesToDelete = existingLooseFiles.Except(newLooseFiles);

        foreach (var id in filesToDelete)
        {
            var file = await _context.EarcModReplacements.FindAsync(id);
            if (file != null)
            {
                _context.Remove(file);
            }
        }

        foreach (var id in earcsToDelete)
        {
            var file = await _context.EarcModEarcs.FindAsync(id);
            if (file != null)
            {
                _context.Remove(file);
            }
        }

        foreach (var id in looseFilesToDelete)
        {
            var file = await _context.EarcModLooseFile.FindAsync(id);
            if (file != null)
            {
                _context.Remove(file);
            }
        }

        // This will add any new records from the new build list
        _context.Update(mod);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    public async Task DeleteMod(int modId)
    {
        // Grab umodified copy of the mod as the delete button is present in the editor where changes may have been made
        var mod = _context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .Include(m => m.LooseFiles)
            .Where(e => e.Id == modId)
            .AsNoTracking()
            .ToList()
            .First();

        if (mod.IsActive)
        {
            RevertMod(mod);
        }

        ClearCachedFilesForMod(modId);

        // Remove any ffg files that were installed with this mod
        foreach (var file in mod.Earcs
                     .SelectMany(e => e.Files
                         .Where(f => f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace)
                         .Select(f => f.ReplacementFilePath)))
        {
            var tokens = file.Split('\\');
            if (tokens[^2] == modId.ToString() && tokens[^1].EndsWith(".ffg"))
            {
                File.Delete(file);
            }
        }

        // Remove the project directory if it's empty
        var projectDirectory = $@"{_profile.EarcModsDirectory}\{modId}";
        if (Directory.Exists(projectDirectory)
            && !Directory.EnumerateDirectories(projectDirectory).Any()
            && !Directory.EnumerateFiles(projectDirectory).Any())
        {
            Directory.Delete(projectDirectory);
        }

        _context.Remove(mod);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    public async Task EnableMod(int modId)
    {
        // Need to pull this again with includes as they won't be there in the Mod Manager
        var mod = _context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .Include(m => m.LooseFiles)
            .Where(e => e.Id == modId)
            .AsNoTracking()
            .ToList()
            .First();

        using (var archiveManager = new EbonyArchiveManager())
        {
            if (mod.HaveFilesChanged || !CheckModIsCached(mod))
            {
                // Cache is invalid, rebuild it
                BuildAssetCache(mod, archiveManager);
            }

            BuildAndApplyMod(mod, archiveManager);
        }

        mod.IsActive = true;
        mod.UpdateLastModifiedTimestamps();

        _context.Update(mod);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    public async Task DisableMod(int modId)
    {
        // Need to pull this again with includes as they won't be there in the Mod Manager
        var mod = _context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .Include(m => m.LooseFiles)
            .Where(e => e.Id == modId)
            .AsNoTracking()
            .ToList()
            .First();

        RevertMod(mod);

        // Now that the mod has been reverted, it can be marked as disabled
        mod.IsActive = false;
        _context.Update(mod);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public void BuildAssetCache(EarcMod mod, EbonyArchiveManager archiveManager)
    {
        var imageMap = new ConcurrentDictionary<string, byte[]>();

        // Get metadata for replacement textures
        var textureMetadata = new ConcurrentDictionary<string, BtexHeader>();
        var replacementTextures = mod.Earcs
            .SelectMany(e => e.Files
                .Where(f => f.Type == EarcFileChangeType.Replace
                            && AssetExplorerItem.GetType(f.Uri) == ExplorerItemType.Texture
                            && !f.ReplacementFilePath.EndsWith(".btex")
                            && !f.ReplacementFilePath.EndsWith(".ffg")
                            && (f.FileLastModified != File.GetLastWriteTime(f.ReplacementFilePath).Ticks
                                || !File.Exists(
                                    $@"{_profile.CacheDirectory}\{mod.Id}{f.Id}{Cryptography.HashFileUri64(f.Uri)}.ffg"))));

        Parallel.ForEach(replacementTextures, file =>
        {
            var relativePath = _appState.GetArchiveRelativePathByUri(file.Uri);
            var path = $@"{_profile.GameDataDirectory}\{relativePath}";

            // Skip if 4K pack is missing so Flagrum doesn't crash
            if (!File.Exists(path) && (path!.Contains(@"\highimages\") || path.EndsWith("_$h2.earc")))
            {
                return;
            }

            var archive = archiveManager.Open(path);
            var data = archive[file.Uri].GetReadableData();
            var header = BtexConverter.ReadBtexHeader(data, _profile.Current.Type == LuminousGame.FFXV, false);
            textureMetadata[file.Uri] = header;
        });

        _assetConverter.SetTextureMetadata(textureMetadata);

        // Process image files
        foreach (var earc in mod.Earcs.Where(e => e.Files.Any(f =>
                     f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                         or EarcFileChangeType.AddToTextureArray
                     && AssetExplorerItem.GetType(f.Uri) == ExplorerItemType.Texture
                     && !f.ReplacementFilePath.EndsWith(".btex"))))
        {
            foreach (var file in earc.Files
                         .Where(f => f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                                         or EarcFileChangeType.AddToTextureArray
                                     && AssetExplorerItem.GetType(f.Uri) == ExplorerItemType.Texture
                                     && !f.ReplacementFilePath.EndsWith(".btex")))
            {
                var hash = Cryptography.HashFileUri64(file.Uri);
                var cachePath = $@"{_profile.CacheDirectory}\{mod.Id}{file.Id}{hash}.ffg";
                var needsRebuild = !file.ReplacementFilePath.EndsWith(".ffg")
                                   && (file.FileLastModified !=
                                       File.GetLastWriteTime(file.ReplacementFilePath).Ticks
                                       || !File.Exists(cachePath));

                if (needsRebuild)
                {
                    imageMap.TryAdd(file.Uri, _assetConverter.Convert(file));
                }
            }
        }

        // Process other assets
        Parallel.ForEach(mod.Earcs.Where(e => e.Files.Any()), earc =>
        {
            var path = $@"{_profile.GameDataDirectory}\{earc.EarcRelativePath}";

            EbonyArchive sourceArchive = null;
            if (File.Exists(path))
            {
                sourceArchive = archiveManager.Open(path);
            }

            Parallel.ForEach(earc.Files.Where(f => f.Type is EarcFileChangeType.Add
                    or EarcFileChangeType.Replace
                    or EarcFileChangeType.AddToTextureArray),
                file =>
                {
                    var hash = Cryptography.HashFileUri64(file.Uri);
                    var cachePath = $@"{_profile.CacheDirectory}\{mod.Id}{file.Id}{hash}.ffg";

                    // Only build files that are not already processed
                    if (!file.ReplacementFilePath.EndsWith(".ffg")
                        && (file.FileLastModified != File.GetLastWriteTime(file.ReplacementFilePath).Ticks
                            || !File.Exists(cachePath)))
                    {
                        FmodFragment fragment;
                        if (file.Type == EarcFileChangeType.Replace)
                        {
                            var original = sourceArchive![file.Uri];
                            var data = imageMap.ContainsKey(file.Uri)
                                ? imageMap[file.Uri]
                                : _assetConverter.Convert(file);

                            var processedData = ArchiveFile.GetProcessedData(file.Uri,
                                original.Flags,
                                data,
                                original.Key,
                                sourceArchive.IsProtectedArchive,
                                out _);

                            fragment = new FmodFragment
                            {
                                OriginalSize = (uint)data.Length,
                                ProcessedSize = (uint)processedData.Length,
                                Flags = original.Flags,
                                Key = original.Key,
                                RelativePath = original.RelativePath,
                                Data = processedData
                            };
                        }
                        else
                        {
                            var data = imageMap.ContainsKey(file.Uri)
                                ? imageMap[file.Uri]
                                : _assetConverter.Convert(file);

                            var processedData =
                                ArchiveFile.GetProcessedData(file.Uri, file.Flags, data, 0, true,
                                    out var archiveFile);

                            fragment = new FmodFragment
                            {
                                OriginalSize = (uint)data.Length,
                                ProcessedSize = (uint)processedData.Length,
                                Flags = archiveFile.Flags,
                                Key = archiveFile.Key,
                                RelativePath = archiveFile.RelativePath,
                                Data = processedData
                            };
                        }

                        fragment.Write(cachePath);
                    }
                });
        });
    }

    public void ClearCachedFilesForMod(int modId)
    {
        foreach (var path in Directory.EnumerateFiles(_profile.CacheDirectory)
                     .Where(f => f.Split('\\').Last().StartsWith(modId.ToString())))
        {
            File.Delete(path);
        }
    }

    public bool CheckModIsCached(EarcMod mod)
    {
        var isCached = true;
        foreach (var earc in mod.Earcs)
        {
            foreach (var file in earc.Files.Where(f =>
                         f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                             or EarcFileChangeType.AddToTextureArray))
            {
                if (!file.ReplacementFilePath.EndsWith(".ffg") &&
                    !File.Exists(
                        $@"{_profile.CacheDirectory}\{mod.Id}{file.Id}{Cryptography.HashFileUri64(file.Uri)}.ffg"))
                {
                    isCached = false;
                }
            }
        }

        return isCached;
    }

    public bool HasAnyCachedFiles(EarcMod mod)
    {
        return mod.Earcs.Any(e => e.Files
            .Any(f => f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                          or EarcFileChangeType.AddToTextureArray
                      && File.Exists(
                          $@"{_profile.CacheDirectory}\{mod.Id}{f.Id}{Cryptography.HashFileUri64(f.Uri)}.ffg")));
    }

    private void UpdateThumbnail(int modId)
    {
        File.Copy($@"{_profile.ImagesDirectory}\current_earc_preview.png",
            $@"{_profile.EarcImagesDirectory}\{modId}.png",
            true);
    }
}