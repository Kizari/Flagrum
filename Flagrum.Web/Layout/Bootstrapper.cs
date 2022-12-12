using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Features.EarcMods.Data;
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
    [Inject] private SettingsService Settings { get; set; }
    [Inject] private ILogger<Bootstrapper> Logger { get; set; }
    [Inject] private BinmodTypeHelper BinmodTypeHelper { get; set; }

    [CascadingParameter] public MainLayout Parent { get; set; }

    protected override void OnInitialized()
    {
        Parallel.Invoke(() => Task.Run(async () =>
            {
                HandleEarcModThumbnails();
                await ScaleEarcModThumbnails();
            }),
            ConvertBackups,
            () => Task.Run(async () => await LoadBinmods()));
    }

    private async Task ScaleEarcModThumbnails()
    {
        if (!Context.GetBool(StateKey.HaveThumbnailsBeenResized))
        {
            var earcModDirectory = $@"{IOHelper.GetWebRoot()}\EarcMods";
            if (Directory.Exists(earcModDirectory))
            {
                foreach (var imagePath in Directory.GetFiles(earcModDirectory, "*.png"))
                {
                    var imageData = await File.ReadAllBytesAsync(imagePath);
                    var converter = new TextureConverter();
                    var image = converter.ProcessEarcModThumbnail(imageData);
                    await File.WriteAllBytesAsync(imagePath, image);
                }
            }

            Context.SetBool(StateKey.HaveThumbnailsBeenResized, true);
        }
    }

    // private void OnNodesLoaded()
    // {
    //     // Set the root node
    //     AppState.Node = Context.AssetExplorerNodes
    //         .FirstOrDefault(n => n.Id == 1);
    //
    //     Task.Run(() =>
    //     {
    //         using var context = new FlagrumDbContext(Settings);
    //
    //         // Get all model nodes
    //         var modelNodes = context.AssetExplorerNodes
    //             .Where(n => n.Name.EndsWith(".gmdl"))
    //             .ToList();
    //
    //         // Recursively populate parents for each node up the tree
    //         foreach (var node in modelNodes)
    //         {
    //             node.TraverseDescending(n =>
    //             {
    //                 if (n.ParentId != null)
    //                 {
    //                     n.Parent = context.AssetExplorerNodes.FirstOrDefault(m => m.Id == n.ParentId);
    //                     ((AssetExplorerNode)n.Parent).ChildNodes ??= new List<AssetExplorerNode>();
    //                     ((AssetExplorerNode)n.Parent).Children.Add(n);
    //                 }
    //             });
    //         }
    //
    //         // Get root node from arbitrary node
    //         var rootNode = modelNodes[0];
    //         while (rootNode.Parent != null)
    //         {
    //             rootNode = (AssetExplorerNode)rootNode.Parent;
    //         }
    //
    //         AppState.RootModelBrowserNode = rootNode;
    //     });
    // }

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

            ClearOldImages();
            AppState.IsModListInitialized = true;
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
            var backupDirectory = $@"{Context.Settings.FlagrumDirectory}\earc\backup";
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
        var thumbnailPaths = Directory.EnumerateFiles(Context.Settings.EarcModThumbnailDirectory).ToList();
        var thumbnailIds = thumbnailPaths.Select(p => p.Split('\\').Last().Split('.').First()).ToList();
        var wwwrootIds = Directory.EnumerateFiles($@"{IOHelper.GetWebRoot()}\EarcMods")
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
                var path = $@"{IOHelper.GetWebRoot()}\EarcMods\{modId}.png";
                if (File.Exists(path))
                {
                    var destination = $@"{Context.Settings.EarcModThumbnailDirectory}\{modId}.png";
                    File.Copy(path, destination);
                }
            }
        }

        // Clone any thumbnails that exist in this folder, but not wwwroot
        foreach (var modId in modIds)
        {
            if (!wwwrootIds.Any(t => t == modId.ToString()))
            {
                var path = $@"{Context.Settings.EarcModThumbnailDirectory}\{modId}.png";
                if (File.Exists(path))
                {
                    var destination = $@"{IOHelper.GetWebRoot()}\EarcMods\{modId}.png";
                    File.Copy(path, destination);
                }
            }
        }
    }
}