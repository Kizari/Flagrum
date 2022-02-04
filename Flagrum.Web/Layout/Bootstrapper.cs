using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
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
    [Inject] private SettingsService Settings { get; set; }
    [Inject] private ILogger<Bootstrapper> Logger { get; set; }
    [Inject] private BinmodTypeHelper BinmodTypeHelper { get; set; }

    [CascadingParameter] public MainLayout Parent { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await Context.Database.MigrateAsync();
        await LoadBinmods();
        await LoadNodes();
        Parent.IsReady = true;
        Parent.CallStateHasChanged();
    }

    private async Task LoadNodes()
    {
        if (!Context.AssetExplorerNodes.Any())
        {
            await Task.Run(() =>
            {
                UriMapper.RegenerateMap();
                AppState.Node = Context.AssetExplorerNodes
                    .FirstOrDefault(n => n.Id == 1);
            });
        }
        else
        {
            AppState.Node = Context.AssetExplorerNodes
                .FirstOrDefault(n => n.Id == 1);
        }
    }

    private async Task LoadBinmods()
    {
        if (!AppState.IsModListInitialized)
        {
            await Task.Run(() =>
            {
                var binmodList = ModlistEntry.FromFile(Settings.BinmodListPath);
                var localMods =
                    Directory.GetFiles(Settings.ModDirectory, "*.ffxvbinmod", SearchOption.TopDirectoryOnly);
                IEnumerable<string> allMods;

                if (Directory.Exists(Settings.WorkshopDirectory))
                {
                    var workshopMods = Directory.GetFiles(Settings.WorkshopDirectory, "*.ffxvbinmod",
                        SearchOption.AllDirectories);
                    allMods = localMods.Union(workshopMods);
                }
                else
                {
                    allMods = localMods;
                }

                var mods = new ConcurrentBag<Binmod>();

                Parallel.ForEach(allMods, file =>
                {
                    using var unpacker = new Unpacker(file);
                    var modmetaBytes = unpacker.UnpackFileByQuery("index.modmeta", out _);
                    var mod = Binmod.FromModmetaBytes(modmetaBytes, BinmodTypeHelper, Logger);
                    var previewBytes = unpacker.UnpackFileByQuery("$preview.png.bin", out _);

                    var binmodListing = binmodList.FirstOrDefault(e => file.Contains(e.Path.Replace('/', '\\')));

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

            ClearOldImages();
            AppState.IsModListInitialized = true;
        }
    }

    private async void ClearOldImages()
    {
        await Task.Run(() =>
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
        });
    }
}