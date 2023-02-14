using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Web.Features.ModManager.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flagrum.Web.Layout;

public class Bootstrapper : ComponentBase
{
    [Inject] private FlagrumDbContext Context { get; set; }
    [Inject] private AppStateService AppState { get; set; }
    [Inject] private UriMapper UriMapper { get; set; }
    [Inject] private ProfileService Profile { get; set; }
    [Inject] private ILogger<Bootstrapper> Logger { get; set; }
    [Inject] private BinmodTypeHelper BinmodTypeHelper { get; set; }

    [CascadingParameter] public MainLayout Parent { get; set; }

    protected override void OnInitialized()
    {
        HandleEarcModThumbnails();
        ScaleEarcModThumbnails();
        ConvertBackups();
        
        Logger.LogInformation("Bootstrapping Flagrum with {Profile} profile", Profile.Current.Type);

        if (Profile.Current.Type == LuminousGame.FFXV)
        {
            Task.Run(async () => await LoadBinmods());
        }
    }

    private async Task ScaleEarcModThumbnails()
    {
        if (!Context.GetBool(StateKey.HaveThumbnailsBeenResized))
        {
            var earcModDirectory = $@"{Profile.EarcImagesDirectory}";
            if (Directory.Exists(earcModDirectory))
            {
                foreach (var imagePath in Directory.GetFiles(earcModDirectory, "*.png"))
                {
                    var imageData = await File.ReadAllBytesAsync(imagePath);
                    var converter = new TextureConverter(Profile.Current.Type);
                    var image = converter.ProcessEarcModThumbnail(imageData);
                    await File.WriteAllBytesAsync(imagePath, image);
                }
            }

            Context.SetBool(StateKey.HaveThumbnailsBeenResized, true);
        }
    }

    private async Task LoadBinmods()
    {
        Logger.LogInformation("Checking if Steam Workshop mod list is initialised");
        
        if (!AppState.IsModListInitialized)
        {
            Logger.LogInformation("Loading metadata from FFXVBINMOD files");
            
            await Task.Run(() =>
            {
                var binmodList = ModlistEntry.FromFile(Profile.Current.BinmodListPath);
                var localMods =
                    Directory.GetFiles(Profile.ModDirectory, "*.ffxvbinmod", SearchOption.TopDirectoryOnly);
                IEnumerable<string> allMods;
                
                Logger.LogInformation("{Count} Local FFXVBINMOD files detected", localMods.Length);

                if (Directory.Exists(Profile.WorkshopDirectory))
                {
                    var workshopMods = Directory.GetFiles(Profile.WorkshopDirectory, "*.ffxvbinmod",
                        SearchOption.AllDirectories).ToList();
                    Logger.LogInformation("{Count} Steam Workshop FFXVBINMOD files detected", workshopMods.Count);
                    allMods = localMods.Union(workshopMods);
                }
                else
                {
                    allMods = localMods;
                }

                var mods = new ConcurrentBag<Binmod>();

                Parallel.ForEach(allMods, file =>
                {
                    using var unpacker = new EbonyArchive(file);
                    var modmetaBytes = unpacker.Files
                        .First(f => f.Value.Uri.EndsWith("index.modmeta")).Value
                        .GetReadableData();

                    var mod = Binmod.FromModmetaBytes(modmetaBytes, BinmodTypeHelper, Logger);
                    var previewBytes = unpacker.Files
                        .FirstOrDefault(f => f.Value.Uri.EndsWith("$preview.png.bin")).Value
                        ?.GetReadableData() ?? Array.Empty<byte>();

                    var binmodListing = binmodList.FirstOrDefault(e =>
                        file.Contains(e.Path.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase));

                    if (mod == null)
                    {
                        Logger.LogWarning("Could not read modmeta from {File}", file);
                        return;
                    }

                    if (binmodListing == null)
                    {
                        Logger.LogWarning("Could not find binmod.list entry for {File}", file);
                        return;
                    }

                    mod.Description = mod.Description?.Replace("\\n", "\n");
                    mod.GameMenuDescription = mod.GameMenuDescription?.Replace("\\n", "\n");
                    mod.IsWorkshopMod = binmodListing.IsWorkshopMod;
                    mod.Index = binmodListing.Index;
                    mod.IsApplyToGame = binmodListing.IsEnabled;
                    mod.Path = file;
                    File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\{mod.Uuid}.png", previewBytes);

                    mods.Add(mod);
                });

                AppState.Mods = mods.ToList();
                var paths = AppState.Mods.Select(m => m.Path);
                AppState.UnmanagedEntries =
                    binmodList.Where(e => !paths.Any(p => p.Contains(e.Path.Replace('/', '\\')))).ToList();
            });

            AppState.IsModListInitialized = true;
            ClearOldImages();
        }
    }

    private async void ClearOldImages()
    {
        await Task.Run(() =>
        {
            try
            {
                var exceptions = AppState.Mods
                    .Select(m => $"{IOHelper.GetWebRoot()}\\images\\{m.Uuid}.png")
                    .ToList();

                foreach (var image in Directory.EnumerateFiles($"{IOHelper.GetWebRoot()}\\images"))
                {
                    if (!exceptions.Contains(image))
                    {
                        File.Delete(image);
                    }
                }
            }
            catch
            {
                // Ignore, try again next time
            }
        });
    }

    private void ConvertBackups()
    {
        if (!Context.GetBool(StateKey.HasMigratedBackups))
        {
            var backupDirectory = $@"{Profile.EarcModBackupsDirectory}";
            foreach (var backup in Context.EarcModBackups)
            {
                var hash = Cryptography.HashFileUri64(backup.Uri);
                var backupPath = $@"{backupDirectory}\{hash}";

                if (File.Exists(backupPath))
                {
                    var data = File.ReadAllBytes(backupPath);
                    var fragment = new FmodFragment
                    {
                        OriginalSize = backup.Size,
                        ProcessedSize = (uint)data.Length,
                        Flags = backup.Flags,
                        Key = backup.Key,
                        RelativePath = backup.RelativePath,
                        Data = data
                    };

                    fragment.Write($@"{backupDirectory}\{hash}.ffg");
                    File.Delete(backupPath);
                }
            }

            Context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(Context.EarcModBackups)}");
            Context.SetBool(StateKey.HasMigratedBackups, true);
        }
    }

    private void HandleEarcModThumbnails()
    {
        var modIds = Context.EarcMods.Select(m => m.Id).ToList();
        var thumbnailPaths = Directory.EnumerateFiles(Profile.EarcModThumbnailDirectory).ToList();
        var thumbnailIds = thumbnailPaths.Select(p => p.Split('\\').Last().Split('.').First()).ToList();
        var wwwrootIds = Directory.EnumerateFiles(Profile.EarcImagesDirectory)
            .Select(p => p.Split('\\').Last().Split('.').First())
            .ToList();

        // Delete any thumbnails for mods that no longer exist
        foreach (var thumbnail in thumbnailPaths)
        {
            var thumbnailId = thumbnail.Split('\\').Last().Split('.').First();
            if (!modIds.Any(m => m.ToString() == thumbnailId))
            {
                File.Delete(thumbnail);
            }
        }

        // Clone any thumbnails that aren't in this folder yet
        foreach (var modId in modIds)
        {
            if (!thumbnailIds.Any(t => t == modId.ToString()))
            {
                var path = $@"{Profile.EarcImagesDirectory}\{modId}.png";
                if (File.Exists(path))
                {
                    var destination = $@"{Profile.EarcModThumbnailDirectory}\{modId}.png";
                    File.Copy(path, destination);
                }
            }
        }

        // Clone any thumbnails that exist in this folder, but not wwwroot
        foreach (var modId in modIds)
        {
            if (!wwwrootIds.Any(t => t == modId.ToString()))
            {
                var path = $@"{Profile.EarcModThumbnailDirectory}\{modId}.png";
                if (File.Exists(path))
                {
                    var destination = $@"{Profile.EarcImagesDirectory}\{modId}.png";
                    File.Copy(path, destination);
                }
            }
        }
    }
}