using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Features.Shared;
using Flagrum.Application.Features.WorkshopMods.Data;
using Flagrum.Application.Features.WorkshopMods.Services;
using Flagrum.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Flagrum.Application.Features.Menu;

public class Bootstrapper : ComponentBase
{
    [Inject] private AppStateService AppState { get; set; }
    [Inject] private IProfileService Profile { get; set; }
    [Inject] private ILogger<Bootstrapper> Logger { get; set; }
    [Inject] private BinmodTypeHelper BinmodTypeHelper { get; set; }
    [Inject] private IConfiguration Configuration { get; set; }
    [Inject] private ModManagerServiceBase ModManager { get; set; }

    [CascadingParameter] public MainLayout Parent { get; set; }

    protected override void OnInitialized()
    {
        HandleEarcModThumbnails();

        Logger.LogInformation("Bootstrapping Flagrum with {Profile} profile", Profile.Current.Type);

        if (Profile.Current.Type == LuminousGame.FFXV)
        {
            Task.Run(LoadBinmods);
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
                    Directory.GetFiles(Profile.BinmodDirectory, "*.ffxvbinmod", SearchOption.TopDirectoryOnly);
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
                    try
                    {
                        using var archive = new EbonyArchive(file);

                        var modmetaFile = archive.Files
                            .FirstOrDefault(f => f.Value.Uri.EndsWith("index.modmeta")).Value;

                        if (modmetaFile == null)
                        {
                            Logger.LogWarning("There was no modmeta file in {File}", file);
                            return;
                        }

                        var modmetaBytes = modmetaFile.GetReadableData();

                        var mod = Binmod.FromModmetaBytes(modmetaBytes, BinmodTypeHelper, Logger);
                        var previewBytes = archive.Files
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
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "An exception occurred while trying to read {File}", file);
                    }
                });

                AppState.Mods = mods.ToList();
                var paths = AppState.Mods.Select(m => m.Path);
                AppState.UnmanagedEntries =
                    binmodList.Where(e => !paths.Any(p => p.Contains(e.Path.Replace('/', '\\')))).ToList();
            });

            AppState.IsModListInitialized = true;
            DeleteUnusedWorkshopThumbnails();
        }
    }

    private async void DeleteUnusedWorkshopThumbnails()
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

    private void HandleEarcModThumbnails()
    {
        var modIds = ModManager.Projects.Keys.ToList();
        var wwwrootIds = Directory.EnumerateFiles(Profile.ImagesDirectory)
            .Select(p => p.Split('\\').Last().Split('.').First())
            .ToList();

        // Clone any thumbnails that exist in the mod's folder, but not in wwwroot/images
        foreach (var modId in modIds)
        {
            if (!wwwrootIds.Any(t => t == modId.ToString()))
            {
                var path = Path.Combine(Profile.ModFilesDirectory, modId.ToString(), "thumbnail.jpg");
                if (File.Exists(path))
                {
                    var destination = Path.Combine(Profile.ImagesDirectory, $"{modId}.jpg");
                    File.Copy(path, destination);
                }
            }
        }
    }
}